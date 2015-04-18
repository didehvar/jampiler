using Jampiler.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jampiler
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

            var lexer = new Lexer();

            foreach (var pair in TokenRegex.Instance.Regexes)
            {
                lexer.AddDefinition(new TokenDefinition(pair.Key, pair.Value));
            }

            do
            {
                var lexTokens = lexer.Tokenize(Console.ReadLine());
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
            } while (true);
        }
    }
}
