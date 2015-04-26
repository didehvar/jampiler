using System.Linq;
using Jampiler.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JampilerTest
{
    [TestClass]
    public class LexerTest
    {
        private readonly Lexer _lexer = new Lexer();

        public LexerTest()
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

            Assert.AreEqual(tokens.ElementAt(0).Type, TokenType.Function);
            Assert.AreEqual(tokens.ElementAt(1).Type, TokenType.Identifier);
            Assert.AreEqual(tokens.ElementAt(2).Type, TokenType.OpenBracket);
            Assert.AreEqual(tokens.ElementAt(3).Type, TokenType.CloseBracket);
            Assert.AreEqual(tokens.ElementAt(4).Type, TokenType.End);
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

            Assert.AreEqual(tokens.ElementAt(0).Type, TokenType.Function);
            Assert.AreEqual(tokens.ElementAt(1).Type, TokenType.Identifier);
            Assert.AreEqual(tokens.ElementAt(2).Type, TokenType.OpenBracket);
            Assert.AreEqual(tokens.ElementAt(3).Type, TokenType.CloseBracket);
            Assert.AreEqual(tokens.ElementAt(4).Type, TokenType.Local);
            Assert.AreEqual(tokens.ElementAt(5).Type, TokenType.Identifier);
            Assert.AreEqual(tokens.ElementAt(5).Value, "bob");
            Assert.AreEqual(tokens.ElementAt(6).Type, TokenType.Equals);
            Assert.AreEqual(tokens.ElementAt(7).Type, TokenType.Number);
            Assert.AreEqual(tokens.ElementAt(7).Value, "1");
            Assert.AreEqual(tokens.ElementAt(8).Type, TokenType.End);
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

            Assert.AreEqual(tokens.ElementAt(0).Type, TokenType.Function);
            Assert.AreEqual(tokens.ElementAt(1).Type, TokenType.Identifier);
            Assert.AreEqual(tokens.ElementAt(2).Type, TokenType.OpenBracket);
            Assert.AreEqual(tokens.ElementAt(3).Type, TokenType.CloseBracket);
            Assert.AreEqual(tokens.ElementAt(4).Type, TokenType.Local);
            Assert.AreEqual(tokens.ElementAt(5).Type, TokenType.Identifier);
            Assert.AreEqual(tokens.ElementAt(5).Value, "bob");
            Assert.AreEqual(tokens.ElementAt(6).Type, TokenType.Equals);
            Assert.AreEqual(tokens.ElementAt(7).Type, TokenType.Number);
            Assert.AreEqual(tokens.ElementAt(7).Value, "1");
            Assert.AreEqual(tokens.ElementAt(8).Type, TokenType.Return);
            Assert.AreEqual(tokens.ElementAt(9).Type, TokenType.Number);
            Assert.AreEqual(tokens.ElementAt(9).Value, "0");
            Assert.AreEqual(tokens.ElementAt(10).Type, TokenType.End);
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

            Assert.AreEqual(tokens.ElementAt(0).Type, TokenType.Function);
            Assert.AreEqual(tokens.ElementAt(1).Type, TokenType.Identifier);
            Assert.AreEqual(tokens.ElementAt(2).Type, TokenType.OpenBracket);
            Assert.AreEqual(tokens.ElementAt(3).Type, TokenType.Identifier);
            Assert.AreEqual(tokens.ElementAt(3).Value, "trevor");
            Assert.AreEqual(tokens.ElementAt(4).Type, TokenType.Comma);
            Assert.AreEqual(tokens.ElementAt(5).Type, TokenType.Identifier);
            Assert.AreEqual(tokens.ElementAt(5).Value, "bob");
            Assert.AreEqual(tokens.ElementAt(6).Type, TokenType.CloseBracket);
            Assert.AreEqual(tokens.ElementAt(7).Type, TokenType.Identifier);
            Assert.AreEqual(tokens.ElementAt(7).Value, "bob");
            Assert.AreEqual(tokens.ElementAt(8).Type, TokenType.Equals);
            Assert.AreEqual(tokens.ElementAt(9).Type, TokenType.Number);
            Assert.AreEqual(tokens.ElementAt(9).Value, "1");
            Assert.AreEqual(tokens.ElementAt(10).Type, TokenType.Return);
            Assert.AreEqual(tokens.ElementAt(11).Type, TokenType.Number);
            Assert.AreEqual(tokens.ElementAt(11).Value, "0");
            Assert.AreEqual(tokens.ElementAt(12).Type, TokenType.End);
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

            Assert.AreEqual(tokens.ElementAt(0).Type, TokenType.Function);
            Assert.AreEqual(tokens.ElementAt(1).Type, TokenType.Identifier);
            Assert.AreEqual(tokens.ElementAt(2).Type, TokenType.OpenBracket);
            Assert.AreEqual(tokens.ElementAt(3).Type, TokenType.CloseBracket);
            Assert.AreEqual(tokens.ElementAt(4).Type, TokenType.Comment);
            Assert.AreEqual(tokens.ElementAt(5).Type, TokenType.Return);
            Assert.AreEqual(tokens.ElementAt(6).Type, TokenType.Number);
            Assert.AreEqual(tokens.ElementAt(6).Value, "0");
            Assert.AreEqual(tokens.ElementAt(7).Type, TokenType.End);
        }
    }
}
