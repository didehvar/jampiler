using System;
using System.Collections.Generic;
using System.Globalization;
using Jampiler.Core;
using System.IO;
using System.Linq;
using Jampiler.Code;

namespace Jampiler
{
    class Program
    {
        private static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

            var lexer = new Lexer();

            foreach (var pair in TokenRegex.Instance.Regexes)
            {
                lexer.AddDefinition(new TokenDefinition(pair.Key, pair.Value));
            }

            var program = File.ReadAllText(@"../../test.jam");
            Console.WriteLine(program);

            var lexTokens = lexer.Tokenize(program);
            var tokens = lexTokens as Token[] ?? lexTokens.ToArray();

            Console.WriteLine();
            Console.WriteLine("----- TOKENS -----");
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
            Console.WriteLine("--- END TOKENS ---");

            Console.WriteLine();

            var parser = new Parser();
            var node = parser.Parse(tokens);

            Console.WriteLine("----- NODES -----");
            node.Print();
            Console.WriteLine("--- END NODES ---");

            Console.WriteLine();
            Console.WriteLine("----- OUTPUT -----");

            var codeGenerator = new CodeGenerator();
            codeGenerator.Generate(node);
            Console.WriteLine(codeGenerator.Output());

            Console.WriteLine("--- END OUTPUT ---");

            Console.ReadLine();
        }
    }
}
