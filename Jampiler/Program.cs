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
            if (args.Length < 1 || args.Length > 2)
            {
                Logger.Instance.Error("Invalid arguments, correct usage: jampiler.exe {file} {ip}");
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

#if DEBUG
            var program = File.ReadAllText(@"../../test.jam");
#else
            var program = File.ReadAllText(args.ElementAtOrDefault(0));
#endif

            Logger.Instance.Debug(program);

            var lexTokens = lexer.Tokenize(program);
            var tokens = lexTokens as Token[] ?? lexTokens.ToArray();

            Logger.Instance.Debug("\n----- TOKENS -----");
            foreach (var token in tokens)
            {
                Logger.Instance.Debug(token.ToString());
            }
            Logger.Instance.Debug("--- END TOKENS ---");

            var parser = new Parser();
            var nodes = parser.Parse(tokens);

            Logger.Instance.Debug("\n----- NODES -----");
            nodes.ForEach(n => n.Print());
            Logger.Instance.Debug("--- END NODES ---");

            Logger.Instance.Debug("\n----- OUTPUT -----");

            var codeGenerator = new CodeGenerator();
            codeGenerator.Generate(nodes);
            var codeGenOutput = codeGenerator.Output();
            Logger.Instance.Debug(codeGenOutput);

            // Write assembly to file
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

            var arguments = "/k arm-linux-gnueabihf-gcc -march=armv6 -mfloat-abi=hard -mfpu=vfp -o jam.out jam.s";
#if DEBUG
            arguments +=
                string.Format(
                    @" & pscp -pw raspberry jam.out pi@{0}:/home/pi & putty -pw raspberry -m chmod pi@{0}",
                    args.ElementAtOrDefault(1) ?? "192.168.1.34");
#else
           arguments +=
                string.Format(
                    @" & pscp -pw raspberry jam.out pi@{0}:/home/pi & " +
                    @"putty -pw raspberry -m chmod pi@{0}",
                    args.ElementAtOrDefault(1) ?? "192.168.1.34");
#endif

            var processStartInfo = new ProcessStartInfo()
            {
                //WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                WorkingDirectory = directory,
                Arguments = arguments,
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
