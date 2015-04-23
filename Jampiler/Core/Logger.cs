using System;
using System.Diagnostics;

namespace Jampiler.Core
{
    public class Logger
    {
        private static Logger _instance;

        private Logger() {}

        public static Logger Instance => _instance ?? (_instance = new Logger());

        public void Debug(string message = "", params object[] args)
        {
#if DEBUG
            if (args.Length > 0)
            {
                Console.WriteLine(message, args);
            }
            else
            {
                Console.WriteLine(message);
            }
#endif
        }
    }
}