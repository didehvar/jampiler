using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Jampiler.Core;

namespace Jampiler
{
    class Program
    {
        private static void Main(string[] args)
        {
#if !DEBUG
            if (args.Length != 2)
            {
                Logger.Instance.Debug("Invalid arguments, correct usage: jampiler.exe {file} {pi ip}");
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
            var nodes = parser.Parse(tokens);

            Logger.Instance.Debug("----- NODES -----");
            nodes.ForEach(n => n.Print());
            Logger.Instance.Debug("--- END NODES ---");

            Logger.Instance.Debug();
            Logger.Instance.Debug("----- OUTPUT -----");

            var codeGenerator = new CodeGenerator();
            codeGenerator.Generate(nodes);
            var codeGenOutput = codeGenerator.Output();
            Logger.Instance.Debug(codeGenOutput);

            var file = new StreamWriter(@"jam.s");
            file.WriteLine(codeGenOutput);
            file.Close();

            Logger.Instance.Debug("--- END OUTPUT ---");

            // Run assembler and linker and copy to pi
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (directory == null)
            {
                throw new Exception("Failed to find working directory");
            }

            var processStartInfo = new ProcessStartInfo()
            {
                //WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                WorkingDirectory = directory,
                Arguments =
                    string.Format(
                        @"/k C:\SysGCC\raspberry\bin\arm-linux-gnueabihf-gcc.exe -march=armv6 -mfloat-abi=hard -mfpu=vfp -o jam.out jam.s & " +
                        @"C:\Users\James\Documents\pscp.exe -pw raspberry jam.out pi@{0}:/home/pi & " +
                        @"C:\Users\James\Documents\putty.exe -pw raspberry -m C:\Users\James\Documents\GitHub\Jampiler\chmod pi@192.168.1.34",
                        args.ElementAtOrDefault(1) ?? "192.168.1.34"),
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            Process.Start(processStartInfo);

#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}
