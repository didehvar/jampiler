using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Jampiler.AST;
using Jampiler.Code;

namespace Jampiler.Core
{
    public class CodeGenerator
    {
        public List<Data> Data { get; set; }

        public List<Function> Functions { get; set; }

        public CodeGenerator()
        {
            Data = new List<Data>();
            Functions = new List<Function>();
        }

        public void Generate(Node tree)
        {
            // First node is top of tree, work out what token each node is
            if (tree.Type == TokenType.Function)
            {
                Functions.Add(ParseFunction(tree));
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

                //Statement(statement);
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

            return string.Format("{0}\n{1}\n{2}", data, text, addr);
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

        private List<Data> ParseExpression(Function parent, Node node)
        {
            // expression = 'nil' | 'false' | 'true' | number | string, [ operator, expression ];

            // As there can be an infinite amount of [ operator, expression ] we must traverse the tree
            // If a node is an operator we must parse the element to the left (nil, false, etc) and then move onto the right
            // The operator is stored in the data list so we can go back through it and calculate the value

            var currentNode = node;
            var data = new List<Data>();

            while (currentNode.Type == TokenType.Operator)
            {
                data.Add(ParseExpressionData(parent, currentNode.Left));
                data.Add(new Data() { Type = "OPERATOR", Value = currentNode.Value });

                currentNode = currentNode.Right;
            }

            data.Add(ParseExpressionData(parent, currentNode));
            return data;
        }

        private Data ParseExpressionData(Function parent, Node node)
        {
            // Just left hand of expression
            switch (node.Type)
            {
                case TokenType.Number:
                    // Mov no into r0 register
                    return new Data() { Value = node.Value };

                case TokenType.String:
                    return AddData(new Data() { Type = "asciz", Value = node.Value, Name = parent.DataName() });

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
    }
}
