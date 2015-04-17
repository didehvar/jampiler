using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jampiler.Core
{
    public class TokenRegex
    {
        private static TokenRegex _instance;

        private TokenRegex()
        {
            Regexes.Add("whitespace", new Regex(@"\s+"));
            Regexes.Add("digit", new Regex(@"[0-9]"));
            Regexes.Add("operator", new Regex(@"\+|-|\*|\/|<|>|>=|<=|==|!=|and|or", RegexOptions.IgnoreCase));
        }

        public static TokenRegex Instance => _instance ?? (_instance = new TokenRegex());

        public Dictionary<string, Regex> Regexes = new Dictionary<string, Regex>();

        public Regex GetRegex(string name)
        {
            return Regexes[name];
        }
    }
}
