using System;

namespace Jampiler.Core
{
    /// <summary>
    /// Singleton used for outputting messages.
    /// </summary>
    public class Logger
    {
        private static Logger _instance;

        private Logger() {}

        public static Logger Instance => _instance ?? (_instance = new Logger());

        public void Debug(string message = "", params object[] args)
        {
#if DEBUG
            Error(message, args);
#endif
        }

        public void Error(string message = "", params object[] args)
        {
            if (args.Length > 0)
            {
                Console.WriteLine(message, args);
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}