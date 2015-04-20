using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Jampiler.AST;

namespace Jampiler.Core
{
    public struct CodeGeneratorBlock
    {
        public string Before;
        public string Body;
        public string After;

        public int LcNumber;
        public int LNumber;

        public CodeGeneratorBlock(int lc, int l, string funcName)
        {
            LcNumber = lc;
            LNumber = l;

            Before = string.Format(".LC{0}:\n", lc);
            Body = string.Format("{0}:\n", funcName);
            After = string.Format(".L{0}:\n", l);
        }
    }

    public class CodeGenerator
    {
        public string Output { get; set; }

        private int _lCNumber;
        private int _lNumber;

        private enum Instruction
        {
            Mov
        }

        public void Generate(Node tree)
        {
            Output = "";
            _lCNumber = 0;
            _lNumber = 2;

            // First node is top of tree, work out what token each node is
            if (tree.Type == TokenType.Function)
            {
                Console.WriteLine("Function");

                // Left node is identifier, right node is block
                Expect(tree.Left, TokenType.Identifier);
                Expect(tree.Right, TokenType.Block);

                var block = new CodeGeneratorBlock(_lCNumber, _lNumber, tree.Left.Value);

                // If block is empty, then this function is super useless
                if (tree.Right.Right == null)
                {
                    return;
                }

                // Iterate through statements
                var statement = tree.Right.Right;
                do
                {
                    if (statement.Type == TokenType.Return)
                    {
                        // Last statement in this block, all others are ignored
                        Return(statement, ref block);
                        break;
                    }

                    //Statement(statement);
                    statement = statement.Right;
                } while (statement != null);

                Console.WriteLine("Add block to output");
                AddBlockToOutput(block);
            }
        }

        private void AddBlockToOutput(CodeGeneratorBlock block)
        {
            Output += block.Before;
            Output += block.Body;
            Output += block.After;
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

        private void AddAssembly(ref CodeGeneratorBlock block, Instruction instruction, string assembly)
        {
            block.Body += string.Format("\t{0}\t{1}\n", instruction.ToString().ToLower(), assembly);
        }

        private void AddAssemblyString(ref CodeGeneratorBlock block, string str)
        {
            block.Before += string.Format("\t.asciz\t{0}\n", str);
            block.After += string.Format("\t.word\t.LC{0}", block.LcNumber);
        }

        private void Return(Node node, ref CodeGeneratorBlock block)
        {
            Console.WriteLine("Return");

            // Return may have just been used to exit function early
            if (node.Left == null)
            {
                return;
            }

            Expression(node.Left, ref block);
            AddAssembly(ref block, Instruction.Mov, "r0, r3");
        }

        private void Expression(Node node, ref CodeGeneratorBlock block)
        {
            Console.WriteLine("Expression");
            Console.WriteLine("{0} | {1}", node.Type, node.Value);

            if (node.Type != TokenType.Operator)
            {
                // Just left hand of expression
                switch (node.Type)
                {
                    case TokenType.Number:
                        AddAssembly(ref block, Instruction.Mov, string.Format("r3, #{0}", node.Value));
                        break;

                    case TokenType.String:
                        AddAssemblyString(ref block, node.Value);
                        AddAssembly(ref block, Instruction.Mov, string.Format("r3, #{0}", node.Value));
                        break;
                }

                return;
            }

            // Left is required part of expression
            // Right is another expression
            Expression(node.Right, ref block);
        }
    }
}
