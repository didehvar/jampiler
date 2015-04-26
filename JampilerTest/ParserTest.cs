using System;
using System.Linq;
using Jampiler.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JampilerTest
{
    [TestClass]
    public class ParserTest
    {
        private readonly Lexer _lexer = new Lexer();
        private readonly Parser _parser = new Parser();

        public ParserTest()
        {
            // Initialise lexer definitions
            foreach (var pair in TokenRegex.Instance.Regexes)
            {
                _lexer.AddDefinition(new TokenDefinition(pair.Key, pair.Value));
            }
        }

        [TestMethod]
        public void TestFunction()
        {
            var lexTokens = _lexer.Tokenize(@"
                function main()
                end
            ");

            // Filter out whitespace tokens, no need to test that whitespace is parsed
            var tokens = lexTokens.Where(t => t.Type != TokenType.Whitespace).ToList();
            var tree = _parser.Parse(tokens);

            var topNode = tree.ElementAt(0);

            Assert.AreEqual(topNode.Type, TokenType.Function);
            Assert.AreEqual(topNode.Left.Type, TokenType.Identifier);
            Assert.AreEqual(topNode.Left.Value, "main");
            Assert.AreEqual(topNode.Right.Type, TokenType.Block);
        }

        [TestMethod]
        public void TestBlock()
        {
            var lexTokens = _lexer.Tokenize(@"
                function main()
                    local bob = 1
                end
            ");

            // Filter out whitespace tokens, no need to test that whitespace is parsed
            var tokens = lexTokens.Where(t => t.Type != TokenType.Whitespace).ToList();
            var tree = _parser.Parse(tokens);

            var topNode = tree.ElementAt(0);

            Assert.AreEqual(topNode.Type, TokenType.Function);
            Assert.AreEqual(topNode.Left.Type, TokenType.Identifier);
            Assert.AreEqual(topNode.Left.Value, "main");
            Assert.AreEqual(topNode.Right.Type, TokenType.Block);
            Assert.AreEqual(topNode.Right.Right.Type, TokenType.Identifier);
            Assert.AreEqual(topNode.Right.Right.Value, "bob");
            Assert.AreEqual(topNode.Right.Right.Left.Type, TokenType.Equals);
            Assert.AreEqual(topNode.Right.Right.Left.Left.Type, TokenType.Local);
            Assert.AreEqual(topNode.Right.Right.Left.Right.Type, TokenType.Number);
            Assert.AreEqual(topNode.Right.Right.Left.Right.Value, "1");
        }

        [TestMethod]
        public void TestReturn()
        {
            var lexTokens = _lexer.Tokenize(@"
                function main()
                    local bob = 1
                    return 0
                end
            ");

            // Filter out whitespace tokens, no need to test that whitespace is parsed
            var tokens = lexTokens.Where(t => t.Type != TokenType.Whitespace).ToList();
            var tree = _parser.Parse(tokens);

            var topNode = tree.ElementAt(0);

            Assert.AreEqual(topNode.Type, TokenType.Function);
            Assert.AreEqual(topNode.Left.Type, TokenType.Identifier);
            Assert.AreEqual(topNode.Left.Value, "main");
            Assert.AreEqual(topNode.Right.Type, TokenType.Block);
            Assert.AreEqual(topNode.Right.Right.Type, TokenType.Identifier);
            Assert.AreEqual(topNode.Right.Right.Value, "bob");
            Assert.AreEqual(topNode.Right.Right.Left.Type, TokenType.Equals);
            Assert.AreEqual(topNode.Right.Right.Left.Left.Type, TokenType.Local);
            Assert.AreEqual(topNode.Right.Right.Left.Right.Type, TokenType.Number);
            Assert.AreEqual(topNode.Right.Right.Left.Right.Value, "1");
            Assert.AreEqual(topNode.Right.Right.Right.Type, TokenType.Return);
            Assert.AreEqual(topNode.Right.Right.Right.Left.Type, TokenType.Number);
            Assert.AreEqual(topNode.Right.Right.Right.Left.Value, "0");
        }

        [TestMethod]
        public void TestArguments()
        {
            var lexTokens = _lexer.Tokenize(@"
                function main(trevor, bob)
                    bob = 1
                    return 0
                end
            ");

            // Filter out whitespace tokens, no need to test that whitespace is parsed
            var tokens = lexTokens.Where(t => t.Type != TokenType.Whitespace).ToList();
            var tree = _parser.Parse(tokens);

            var topNode = tree.ElementAt(0);

            Assert.AreEqual(topNode.Type, TokenType.Function);
            Assert.AreEqual(topNode.Left.Type, TokenType.Identifier);
            Assert.AreEqual(topNode.Left.Value, "main");
            Assert.AreEqual(topNode.Right.Type, TokenType.Block);
            Assert.AreEqual(topNode.Right.Left.Type, TokenType.Identifier);
            Assert.AreEqual(topNode.Right.Left.Value, "trevor");
            Assert.AreEqual(topNode.Right.Left.Right.Type, TokenType.Identifier);
            Assert.AreEqual(topNode.Right.Left.Right.Value, "bob");
            Assert.AreEqual(topNode.Right.Right.Type, TokenType.Identifier);
            Assert.AreEqual(topNode.Right.Right.Value, "bob");
            Assert.AreEqual(topNode.Right.Right.Left.Type, TokenType.Equals);
            Assert.AreEqual(topNode.Right.Right.Left.Right.Type, TokenType.Number);
            Assert.AreEqual(topNode.Right.Right.Left.Right.Value, "1");
            Assert.AreEqual(topNode.Right.Right.Right.Type, TokenType.Return);
            Assert.AreEqual(topNode.Right.Right.Right.Left.Type, TokenType.Number);
            Assert.AreEqual(topNode.Right.Right.Right.Left.Value, "0");
        }

        [TestMethod]
        public void TestComment()
        {
            var lexTokens = _lexer.Tokenize(@"
                function main()
                    // bob = 1
                    return 0
                end
            ");

            // Filter out whitespace tokens, no need to test that whitespace is parsed
            var tokens = lexTokens.Where(t => t.Type != TokenType.Whitespace).ToList();
            var tree = _parser.Parse(tokens);

            var topNode = tree.ElementAt(0);

            // Comments should not appear in the syntax tree
            Assert.AreEqual(topNode.Type, TokenType.Function);
            Assert.AreEqual(topNode.Left.Type, TokenType.Identifier);
            Assert.AreEqual(topNode.Left.Value, "main");
            Assert.AreEqual(topNode.Right.Type, TokenType.Block);
            Assert.AreEqual(topNode.Right.Right.Type, TokenType.Return);
            Assert.AreEqual(topNode.Right.Right.Left.Type, TokenType.Number);
            Assert.AreEqual(topNode.Right.Right.Left.Value, "0");
        }
    }
}
