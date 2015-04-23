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
#if !DEBUG
            if (args.Length != 1)
            {
                Logger.Instance.Debug("Invalid arguments, correct usage: jampiler.exe {file}");
                return;
            }

            Logger.Instance.Debug(args[0]);
#endif

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

            var lexer = new Lexer();

            foreach (var pair in TokenRegex.Instance.Regexes)
            {
                lexer.AddDefinition(new TokenDefinition(pair.Key, pair.Value));
            }

            var program = File.ReadAllText(@"../../test.jam");
            Logger.Instance.Debug(program);

            var lexTokens = lexer.Tokenize(program);
            var tokens = lexTokens as Token[] ?? lexTokens.ToArray();

            Logger.Instance.Debug();
            Logger.Instance.Debug("----- TOKENS -----");
            foreach (var token in tokens)
            {
                Logger.Instance.Debug(token.ToString());
            }
            Logger.Instance.Debug("--- END TOKENS ---");

            Logger.Instance.Debug();

            var parser = new Parser();
            var node = parser.Parse(tokens);

            Logger.Instance.Debug("----- NODES -----");
            node.Print();
            Logger.Instance.Debug("--- END NODES ---");

            Logger.Instance.Debug();
            Logger.Instance.Debug("----- OUTPUT -----");

            var codeGenerator = new CodeGenerator();
            codeGenerator.Generate(node);
            var codeGenOutput = codeGenerator.Output();
            Logger.Instance.Debug(codeGenOutput);

            var file = new StreamWriter(@"jam.s");
            file.WriteLine(codeGenOutput);
            file.Close();

            Logger.Instance.Debug("--- END OUTPUT ---");

#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}
