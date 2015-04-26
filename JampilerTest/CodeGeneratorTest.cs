using System.Linq;
using Jampiler.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JampilerTest
{
    [TestClass]
    public class CodeGeneratorTest
    {
        private readonly Lexer _lexer = new Lexer();
        private readonly Parser _parser = new Parser();
        private readonly CodeGenerator _codeGenerator = new CodeGenerator();

        public CodeGeneratorTest()
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

            _codeGenerator.Generate(tree);
            var codeGenOutput = _codeGenerator.Output();

            Assert.AreEqual(codeGenOutput, @".data


.text

.global main
main:

endmain:
	bx lr



/* externals */
");
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

            _codeGenerator.Generate(tree);
            var codeGenOutput = _codeGenerator.Output();

            Assert.AreEqual(codeGenOutput, @".data

return0: .word	0

.text

.global main
main:

	ldr r0, addr_return0
	str lr, [r0]

	mov r1, #1

	ldr lr, addr_return0
	ldr lr, [lr]

endmain:
	bx lr


addr_return0: .word return0

/* externals */
");
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

            _codeGenerator.Generate(tree);
            var codeGenOutput = _codeGenerator.Output();

            Assert.AreEqual(codeGenOutput, @".data

return0: .word	0

.text

.global main
main:

	ldr r0, addr_return0
	str lr, [r0]

	mov r1, #1

	/* start assembledata() */
	mov r2, #0
	mov r0, r2
	/* end assembledata() */


	ldr lr, addr_return0
	ldr lr, [lr]

endmain:
	bx lr


addr_return0: .word return0

/* externals */
");
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

            _codeGenerator.Generate(tree);
            var codeGenOutput = _codeGenerator.Output();

            // Comments don't affect the program
            Assert.AreEqual(codeGenOutput, @".data

return0: .word	0

.text

.global main
main:

	ldr r0, addr_return0
	str lr, [r0]


	/* start assembledata() */
	mov r1, #0
	mov r0, r1
	/* end assembledata() */


	ldr lr, addr_return0
	ldr lr, [lr]

endmain:
	bx lr


addr_return0: .word return0

/* externals */
");
        }
    }
}
