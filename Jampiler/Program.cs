using Jampiler.Core;
using System;
using System.Collections.Generic;
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
            var lexer = new Lexer();

            lexer.AddDefinition(new TokenDefinition("operator", new Regex(@"^\+|-|\*|\/|<|>|>=|<=|==|!=|and|or", RegexOptions.IgnoreCase)));

            var tokens = lexer.Tokenize(Console.ReadLine());

            foreach (var token in tokens)
            {
                Console.WriteLine(token.Value);
                Console.WriteLine(token.Type);
                Console.WriteLine(token.Position.Index);
                Console.WriteLine(token.Position.Column);
                Console.WriteLine(token.Position.Line);
            }

            Console.ReadLine();
        }
    }
}
