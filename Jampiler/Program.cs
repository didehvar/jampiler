using System;
using System.Globalization;
using Jampiler.Core;
using System.IO;
using System.Linq;

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
            var nodes = parser.Parse(tokens);

            Console.WriteLine("----- NODES -----");
            nodes.Print();
            Console.WriteLine("--- END NODES ---");

            Console.ReadLine();
        }
    }
}
