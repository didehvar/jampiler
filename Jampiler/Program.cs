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

                foreach (var token in tokens)
                {
                    Console.WriteLine(token);
                }

                Console.WriteLine();
                Console.WriteLine();

                var parser = new Parser(tokens);
            } while (true);
        }
    }
}
