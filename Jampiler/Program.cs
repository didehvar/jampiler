using Jampiler.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

            var tokens = lexer.Tokenize("1 * 1");

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }

            Console.ReadLine();
        }
    }
}
