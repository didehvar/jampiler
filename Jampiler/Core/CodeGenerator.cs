using System;
using System.Collections.Generic;
using System.Linq;
using Jampiler.AST;
using Jampiler.Code;

namespace Jampiler.Core
{
    public enum Instruction
    {
        Mov,
        Ldr,
        Str
    }

    public class CodeGenerator
    {
        public List<Function> Functions = new List<Function>();

        public void Generate(Node tree)
        {
            // First node is top of tree, work out what token each node is
            if (tree.Type == TokenType.Function)
            {
                Console.WriteLine("Function");

                // Left node is identifier, right node is func
                Expect(tree.Left, TokenType.Identifier);
                Expect(tree.Right, TokenType.Block);

                // If func is empty, then this function is super useless
                if (tree.Right.Right == null)
                {
                    return;
                }

                var func = new Function(tree.Left.Value);

                // Iterate through statements
                var statement = tree.Right.Right;
                do
                {
                    if (statement.Type == TokenType.Return)
                    {
                        // Last statement in this func, all others are ignored
                        Return(statement, ref func);
                        break;
                    }

                    //Statement(statement);
                    statement = statement.Right;
                } while (statement != null);

                Console.WriteLine("Add func to output");
                Functions.Add(func);
            }
        }

        public string Output()
        {
            var data = ".data\n\n";
            var text = ".text\n\n";

            foreach (var f in Functions)
            {
                data += f.Data();
                text += f.Text();
            }

            return string.Format("{0}\n{1}", data, text);
        }

        private void Expect(Node node, TokenType type)
        {
            if (node == null || node.Type != type)
            {
                throw new Exception("Unexpected node type");
            }
        }

        private void Expect(Node node, List<TokenType> types)
        {
            if (node == null || types.All(t => node.Type != t))
            {
                throw new Exception("Unexpected node type");
            }
        }

        private void AddAssembly(ref Function func, Instruction instruction, string assembly)
        {
            func.AddLine(instruction, assembly);
        }

        private string AddAssemblyString(ref Function func, string str)
        {
            return func.AddData(new Data("asciz", str));
        }

        private void Return(Node node, ref Function func)
        {
            Console.WriteLine("Return");

            // Return may have just been used to exit function early
            if (node.Left == null)
            {
                return;
            }

            Expression(node.Left, ref func);
        }

        private void Expression(Node node, ref Function func)
        {
            Console.WriteLine("Expression");
            Console.WriteLine("{0} | {1}", node.Type, node.Value);

            if (node.Type != TokenType.Operator)
            {
                // Just left hand of expression
                switch (node.Type)
                {
                    case TokenType.Number:
                        AddAssembly(ref func, Instruction.Mov, string.Format("r3, #{0}", node.Value));
                        break;

                    case TokenType.String:
                        var strName = AddAssemblyString(ref func, node.Value);
                        AddAssembly(ref func, Instruction.Ldr, string.Format("r0, {0}", strName));
                        AddAssembly(ref func, Instruction.Str, string.Format("lr, [r0]", strName));
                        break;
                }

                return;
            }

            // Left is required part of expression
            // Right is another expression
            Expression(node.Right, ref func);
        }
    }
}
