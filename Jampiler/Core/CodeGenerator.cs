﻿using System;
using System.Collections.Generic;
using System.Linq;
using Jampiler.AST;
using Jampiler.Code;

namespace Jampiler.Core
{
    /// <summary>
    /// Generates assembly code based on the abstract syntax tree provided by the parser.
    /// </summary>
    public class CodeGenerator
    {
        /// <summary>
        /// Variables used in the program.
        /// </summary>
        public List<Data> Data { get; set; }

        /// <summary>
        /// Functions used in the program.
        /// </summary>
        public List<Function> Functions { get; set; }

        /// <summary>
        /// External C runtime functions.
        /// </summary>
        private readonly List<string> _externals = new List<string>(); 

        public CodeGenerator()
        {
            Data = new List<Data>();
            Functions = new List<Function>();
        }

        /// <summary>
        /// Generate assembly code.
        /// </summary>
        /// <param name="nodes">Abstract syntax tree entry points</param>
        public void Generate(List<Node> nodes)
        {
            // First node is top of tree, work out what token each node is
            // Parse statements and functions differently
            foreach (var n in nodes.Where(n => n.Type == TokenType.Identifier))
            {
                Logger.Instance.Debug("PARSING NON-FUNCTION {0}", n.ToString());
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

            // Create a data label for this functions lr store
            var data = new Data(DataType.Return)
            {
                Name = "return" + Data.Count,
                Type = DataType.Word,
                Value = "0"
            };
            Data.Add(data);

            var lrRegister = func.AddRegister(data);
            func.StoreLr(data, lrRegister);

            // Iterate through statements in this functions block
            // Parse return separately
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

            func.LoadLr(data, lrRegister);

            return func;
        }

        /// <summary>
        /// Constructs the assembly output for the generation process based on data stored by the output.
        /// </summary>
        /// <returns>Assembly code representation</returns>
        public string Output()
        {
            var data = Data.Aggregate(".data\n\n", (current, d) => current + d.Text());
            var text = Functions.Aggregate(".text\n\n", (current, f) => current + f.Text());
            var addr = Data.Where(a => a.Name != null)
                .Aggregate("", (current, a) => string.Format("{0}\naddr_{1}: .word {1}", current, a.Name));
            var externals = _externals.Aggregate("/* externals */\n", (current, e) => current + string.Format(".global {0}\n", e));

            return string.Format("{0}\n{1}\n{2}\n\n{3}", data, text, addr, externals);
        }

        /// <summary>
        /// Fail if the node doesn't have the correct token type.
        /// </summary>
        private static void Expect(Node node, TokenType type)
        {
            if (node == null || node.Type != type)
            {
                throw new Exception("Unexpected node type");
            }
        }

        public Data ParseStatement(Function parent, Node node)
        {
            // statement = [ ‘local’ ], identifier, [ ( ‘=‘, expression ) | arg list ]
            //           | 'if', expression, 'then', block, [ 'else', block],  'end if'
            //           | 'while', expression, 'then', block, 'end while';

            var statement = new Statement(parent);

            // [ ‘local’ ], identifier, ‘=‘, expression;
            if (parent != null && node.Left != null && node.Left.Type == TokenType.Equals &&
                ((node.Left.Left != null && node.Left.Left.Type == TokenType.Local) || node.Left.Type == TokenType.Local))
            {
                statement.Name = node.Value;
                statement.Value = node.Left.Right.Value;

                // Parse differently based on expression type.
                switch (node.Left.Right.Type)
                {
                    case TokenType.Number:
                        statement.Type = DataType.Number;
                        break;

                    case TokenType.String:
                        Data.Add(new Data(DataType.Asciz) { Name = statement.Name, Value = statement.Value });
                        statement.Type = DataType.Asciz;
                        break;

                    case TokenType.Identifier:
                        statement.Type = DataType.Function;
                        break;

                    case TokenType.Operator: // Expression
                        statement.Type = DataType.Operator;
                        statement.Datas = ParseExpression(parent, node.Left.Right);
                        break;

                    default:
                        throw new NotImplementedException("Unsupported type");
                }
            }
            // identifier, '=', expression
            else if (node.Left != null && node.Left.Type == TokenType.Equals)
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
            // 'if', expression, block, [ 'else', block ], 'end if';
            else if (node.Type == TokenType.If)
            {
                // Left is block
                // Left left is comparison
                parent.AssembleDataIf(this, node, ParseExpression(parent, node.Left.Left), true, false);
            }
            // 'while', expression, 'then', block, 'end while'
            else if (node.Type == TokenType.While)
            {
                // Same node structure as if
                parent.AssembleDataIf(this, node, ParseExpression(parent, node.Left.Left), true, false, true);
            }
            // identifier, arg list;
            else
            {
                statement.Name = node.Value;
                Logger.Instance.Debug("PS() => (id, arg) => {0}", node);

                if (node.Type == TokenType.Whitespace)
                {
                    return statement;
                }

                switch (node.Value)
                {
                    case "print":
                        // Need to load as an external to call the C runtime function
                        if (!_externals.Contains("printf"))
                        {
                            _externals.Add("printf");
                        }

                        parent.Lines.Add("\t/* print */\n");

                        // Push all registers that printf will use
                        var nextNode = node.Left.Right;
                        var count = 1; // Registers used for printf
                        while (nextNode != null)
                        {
                            count++;
                            nextNode = nextNode.Right;
                        }

                        var push = "\tpush {r0";
                        var pop = "\n\tpop {r0";

                        for (var i = 1; i < count; i++)
                        {
                            var format = string.Format(", r{0}", i);

                            push += format;
                            pop += format;
                        }

                        push += "}\n\n";
                        pop += "}\n";
                        
                        parent.Lines.Add(push);

                        // Store initial printf string
                        var name = "printf" + Data.Count;
                        Data.Add(new Data(DataType.Asciz) { Name = name, Value = node.Left.Value });

                        nextNode = node.Left.Right;
                        count = 1; // Registers used for printf
                        while (nextNode != null)
                        {
                            Logger.Instance.Debug("TYPE {0}", nextNode);

                            switch (nextNode.Type)
                            {
                                case TokenType.Identifier:
                                    // Local variables will already be loaded
                                    // Global variables are not loaded
                                    LoadIdentifier(nextNode, parent, count++);

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

                        // Load string into register
                        parent.Lines.Add(string.Format("\tldr r0, addr_{0}\n\n", name));
                        parent.Lines.Add("\tbl printf\n");

                        parent.Lines.Add(pop);
                        parent.Lines.Add("\t/* end print */\n\n");

                        break;

                    default:
                        // Assume this is an identifier
                        // Find function with the name of this identifier
                        var func = Functions.FirstOrDefault(f => f.Name == node.Value);
                        if (func == null)
                        {
                            throw new Exception("Attempt to call undefined function");
                        }

                        parent.Lines.Add(string.Format("\tbl {0}\n\n", node.Value));

                        break;
                }
            }

            return statement;
        }

        public Return ParseReturn(Function parent, Node node)
        {
            // return statement = 'return’ expression;

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
                    Logger.Instance.Debug("RETURNNIL");
                    return new Data(DataType.Number) { Value = "0" };

                default:
                    throw new NotImplementedException("Data type not supported");
            }
        }

        /// <summary>
        /// Add data but fail if the data is invalid.
        /// </summary>
        private Data AddData(Data data)
        {
            if (data.Type == null || data.Value == null)
            {
                throw new Exception("Cannot add data without a type");
            }

            Data.Add(data);
            return data;
        }

        /// <summary>
        /// Retrieve a variable/function based on its identifier.
        /// </summary>
        /// <param name="parent">Parent to scan before scanning globals</param>
        /// <param name="name">Name of identifier</param>
        /// <returns>Data containing identifier with this name</returns>
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

        /// <summary>
        /// Generate assembly code to load an identifier into a register.
        /// </summary>
        /// <param name="identifierNode">Node to generate for</param>
        /// <param name="parent">Parent containing assembly code</param>
        /// <param name="register">Register to load into</param>
        private void LoadIdentifier(Node identifierNode, Function parent, int register)
        {
            Logger.Instance.Debug("LoadIdentifier() {0}", identifierNode.ToString());
            var locData = GetDataWithName(parent, identifierNode.Value);

            // If this is an identifier we just load it into the right register
            var data = locData as Statement;
            if (data != null && data.Type != DataType.Function)
            {
                var s = data;
                if (register != s.Register) // Don't load it in if it is already in the right register
                {
                    parent.Lines.Add(string.Format("\tmov r{0}, r{1}\n", register, s.Register));
                }

                return;
            }

            // Otherwise, take a different action dpeending on data type
            switch (locData.Type)
            {
                case DataType.Global: // if global, find the global and assemble from this parent
                    var global = (Global)locData;
                    parent.AssembleData(global.Datas, false, false);
                    break;

                case DataType.Number:
                    parent.Lines.Add(string.Format("\tmov r{0}, #{1}\n", register, locData.Value));
                    break;

                case DataType.Asciz:
                    parent.Lines.Add(string.Format("\tldr r{0}, addr_{1}\n", register, locData.Name));
                    break;

                case DataType.Function:
                    // Check if anything is already in r0 (function return)
                    int? backupReg = null;
                    var backupData = parent.Registers.ElementAtOrDefault(0);
                    if (backupData != null)
                    {
                        // Move into an unused register
                        backupReg = parent.AddRegister(backupData) + 1;
                        parent.Lines.Add(string.Format("\tmov r{0}, r0\n", backupReg));
                    }

                    // Call function
                    parent.Lines.Add(string.Format("\tbl {0}\n", locData.Value));

                    // Result of function is in r0
                    // Move that into the required register
                    if (register != 0)
                    {
                        parent.Lines.Add(string.Format("\tmov r{0}, r0\n", register));
                    }

                    // If data was backed up restore it
                    if (backupReg != null)
                    {
                        parent.Lines.Add(string.Format("\tmov r0, r{0}\n", backupReg));
                    }

                    break;

                case DataType.Operator:
                    // Data is in a register in parent, find it and load it
                    var regData = parent.Registers.FirstOrDefault(r => r.Name == locData.Name);
                    if (regData == null)
                    {
                        throw new Exception("Missing variable");
                    }

                    // Useless moving data into a register its already in
                    var existingRegister = ((Statement) regData).Register;
                    if (existingRegister != register)
                    {
                        parent.Lines.Add(string.Format("\tmov r{0}, r{1}\n", register, existingRegister));
                    }

                    break;

                default:
                    throw new NotImplementedException("Cannot load this data type as an identifier");
            }
        }
    }
}
