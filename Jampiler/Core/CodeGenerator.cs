using System;
using System.Collections.Generic;
using System.Linq;
using Jampiler.AST;
using Jampiler.Code;

namespace Jampiler.Core
{
    public class CodeGenerator
    {
        public List<Data> Data { get; set; }

        public List<Function> Functions { get; set; }

        private readonly List<string> _externals = new List<string>(); 

        public CodeGenerator()
        {
            Data = new List<Data>();
            Functions = new List<Function>();
        }

        public void Generate(List<Node> nodes)
        {
            // First node is top of tree, work out what token each node is
            foreach (var n in nodes.Where(n => n.Type == TokenType.Identifier))
            {
                Console.WriteLine("PARSING NON-FUNCTION {0}", n.ToString());
                ParseStatement(null, n);
            }

            foreach (var n in nodes.Where(n => n.Type == TokenType.Function))
            {
                Functions.Add(ParseFunction(n));
            }
        }

        public Function ParseFunction(Node funcNode)
        {
            // Left node is identifier, right node is funcNode
            Expect(funcNode.Left, TokenType.Identifier);
            Expect(funcNode.Right, TokenType.Block);

            var func = new Function(funcNode, funcNode.Left.Value);

            if (funcNode.Right.Right == null)
            {
                return func;
            }

            var statement = funcNode.Right.Right;
            do
            {
                if (statement.Type == TokenType.Return)
                {
                    func.AddReturn(ParseReturn(func, statement));
                }
                else
                {
                    func.AddData(ParseStatement(func, statement));
                }

                statement = statement.Right;
            } while (statement != null);

            return func;
        }

        public string Output()
        {
            var data = Data.Aggregate(".data\n\n", (current, d) => current + d.Text());
            var text = Functions.Aggregate(".text\n\n", (current, f) => current + f.Text());
            var addr = Data.Where(a => a.Name != null)
                .Aggregate("", (current, a) => string.Format("{0}\naddr_{1}: .word {1}", current, a.Name));
            var externals = _externals.Aggregate("/* externals */\n", (current, e) => current + string.Format(".global {0}\n", e));

            return string.Format("{0}\n{1}\n{2}\n\n{3}", data, text, addr, externals);
        }

        private static void Expect(Node node, TokenType type)
        {
            if (node == null || node.Type != type)
            {
                throw new Exception("Unexpected node type");
            }
        }

        private static void Expect(Node node, List<TokenType> types)
        {
            if (node == null || types.All(t => node.Type != t))
            {
                throw new Exception("Unexpected node type");
            }
        }


        private Data ParseStatement(Function parent, Node node)
        {
            // statement = 'local’, identifier, [ '=', (string | number | identifier)]
            //           | identifier, '=', expression
            //           | identifier, arg list;

            Logger.Instance.Debug("ParseStatement:\n{0}\n{1}\n{2}\n{3}", parent, node, node.Left, node.Left.Left);
            var statement = new Statement(parent);

            // 'local’, identifier, [ '=', (string | number | identifier)]
            if (parent != null && node.Left.Type == TokenType.Equals && node.Left.Left != null)
            {
                statement.Name = node.Value;
                statement.Value = node.Left.Right.Value;

                switch (node.Left.Right.Type)
                {
                    case TokenType.Number:
                        statement.Type = DataType.Number;
                        break;

                    case TokenType.String:
                        Data.Add(new Data(DataType.Asciz) { Name = statement.Name, Value = statement.Value });
                        statement.Type = DataType.Asciz;
                        break;

                    default:
                        throw new NotImplementedException("Unsupported type");
                }
            }
            // identifier, '=', expression
            else if (node.Left.Type == TokenType.Equals)
            {
                // Must add to the globals first as parse expression will try to find the data
                var global = Globals.Instance.List.FirstOrDefault(g => g.Name == node.Value);
                if (global == null)
                {
                    global = new Global { Name = node.Value };
                    Globals.Instance.List.Add(global);

                    global.Datas = ParseExpression(parent, node.Left.Right, node);
                }
                return global;
            }
            else if (parent == null)
            {
                throw new Exception("Expected global");
            }
            // identifier, arg list;
            else
            {
                statement.Name = node.Value;

                switch (node.Value)
                {
                    case "print":
                        // Need to load as an external to call the C runtime function
                        if (!_externals.Contains("printf"))
                        {
                            _externals.Add("printf");
                        }

                        var data = new Data(DataType.Return)
                        {
                            Name = "return" + Data.Count,
                            Type = DataType.Word,
                            Value = "0"
                        };
                        Data.Add(data);

                        var register = parent.AddRegister(data);

                        // Store lr before puts replaces it
                        parent.Lines.Add(string.Format("\n\tldr r{0}, addr_{1}\n", register, data.Name));
                        parent.Lines.Add(string.Format("\tstr lr, [r{0}]\n\n", register));

                        // Store initial printf string
                        var name = "printf" + Data.Count;
                        Data.Add(new Data(DataType.Asciz) { Name = name, Value = node.Left.Value });

                        // Load string into register
                        parent.Lines.Add(string.Format("\tldr r0, addr_{0}\n\n", name));

                        var nextNode = node.Left.Right;
                        var count = 1; // Registers used for printf
                        while (nextNode != null)
                        {
                            Console.WriteLine("TYPE {0}", nextNode);

                            switch (nextNode.Type)
                            {
                                case TokenType.Identifier:
                                    // Local variables will already be loaded
                                    // Global variables are not loaded

                                    Console.WriteLine("LOCDATA_ {0}", nextNode.ToString());
                                    var locData = GetDataWithName(parent, nextNode.Value);
                                    /*int regNum;

                                    // If its a global variable load it into a register 
                                    if (locData is Global)
                                    {
                                        Console.WriteLine("GLOBAL");
                                        regNum = parent.RegisterCount();
                                        parent.Lines.Add(parent.ParseData(locData));

                                        switch (locData.Type)
                                        {
                                            case DataType.Number:

                                                break;

                                            default:
                                                throw new NotImplementedException("Globals do not support this type");
                                        }
                                    }*/

                                    if (locData.Type == DataType.Global)
                                    {
                                        // Global variables may need loading in
                                        var global = (Global) locData;
                                        parent.AssembleData(global.Datas, false, false);
                                    }
                                    else
                                    {
                                        Console.WriteLine("DATA TYPE: {0}", locData.ToString());
                                        parent.Lines.Add(
                                            locData.Type == DataType.Number
                                                ? string.Format("\tmov r{0}, #{1}\n", count++, locData.Value)
                                                : string.Format("\tldr r{0}, addr_{1}\n", count++, locData.Name));
                                    }

                                    break;

                                case TokenType.String:
                                    var strName = "printfstr" + Data.Count;
                                    Data.Add(new Data(DataType.Asciz) { Name = strName, Value = nextNode.Value });

                                    parent.Lines.Add(string.Format("\tldr r{0}, addr_{1}\n", count++, strName));
                                    break;

                                case TokenType.Number:
                                    parent.Lines.Add(string.Format("\tmov r{0}, #{1}\n", count++, nextNode.Value));
                                    break;

                                default:
                                    throw new NotImplementedException("Other types currently not supported");
                            }

                            nextNode = nextNode.Right;
                        }

                        parent.Lines.Add("\tbl printf\n\n");

                        // Restore lr
                        parent.Lines.Add(string.Format("\tldr r{0}, addr_{1}\n", register, data.Name));
                        parent.Lines.Add(string.Format("\tldr lr, [r{0}]\n\n", register));

                        break;

                    default:
                        throw new NotImplementedException("Undefined functions not supported");
                }
            }

            return statement;
        }

        private Return ParseReturn(Function parent, Node node)
        {
            // return statement = ‘return’ expression;

            var ret = new Return(parent);

            if (node.Left == null) // Return may have just been used to exit function early
            {
                return ret;
            }

            ret.Data = ParseExpression(parent, node.Left);
            return ret;
        }

        private List<Data> ParseExpression(Function parent, Node node, Node optionalIdentifier = null)
        {
            // expression = 'nil' | 'false' | 'true' | number | string, [ operator, expression ];

            // As there can be an infinite amount of [ operator, expression ] we must traverse the tree
            // If a node is an operator we must parse the element to the left (nil, false, etc) and then move onto the right
            // The operator is stored in the data list so we can go back through it and calculate the value

            var currentNode = node;
            var data = new List<Data>();

            while (currentNode.Type == TokenType.Operator)
            {
                data.Add(ParseExpressionData(parent, currentNode.Left, optionalIdentifier));
                data.Add(new Data(DataType.Operator) { Value = currentNode.Value });

                currentNode = currentNode.Right;
            }

            data.Add(ParseExpressionData(parent, currentNode, optionalIdentifier));
            return data;
        }

        private Data ParseExpressionData(Function parent, Node node, Node topNode = null)
        {
            // Just left hand of expression
            switch (node.Type)
            {
                case TokenType.Number:
                    // Mov no into r0 register
                    return parent == null ? AddData(new Data(DataType.Number) { Name = topNode.Value, Value = node.Value }) : new Data(DataType.Number) { Value = node.Value };

                case TokenType.String:
                    return AddData(new Data(DataType.Asciz) { Value = node.Value, Name = parent == null ? topNode.Value : parent.DataName() });

                case TokenType.Identifier:
                    // Identifier in an expression, must get identifier value
                    return GetDataWithName(parent, node.Value);

                case TokenType.Nil:
                    return null;

                default:
                    throw new NotImplementedException("Data type not supported");
            }
        }

        private Data AddData(Data data)
        {
            if (data.Type == null || data.Value == null)
            {
                throw new Exception("Cannot add data without a type");
            }

            Data.Add(data);
            return data;
        }

        private Data GetDataWithName(Function parent, string name)
        {
            var statement = parent.Statements.FirstOrDefault(s => s.Name == name);
            if (statement != null)
            {
                return statement;
            }

            var global = Globals.Instance.List.FirstOrDefault(g => g.Name == name);
            if (global != null)
            {
                return global;
            }

            throw new Exception("Identifier not found");
        }
    }
}
