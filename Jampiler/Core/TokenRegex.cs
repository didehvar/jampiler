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
            Regexes.Add(TokenTypes.Whitespace, new Regex(@"\s+"));
            Regexes.Add(TokenTypes.Digit, new Regex(@"[0-9]"));
            Regexes.Add(TokenTypes.Operator, new Regex(@"\+|-|\*|\/|<|>|>=|<=|==|!=|and|or", RegexOptions.IgnoreCase));
        }

        public static TokenRegex Instance => _instance ?? (_instance = new TokenRegex());

        public Dictionary<TokenTypes, Regex> Regexes = new Dictionary<TokenTypes, Regex>();

        public Regex GetRegex(TokenTypes type)
        {
            return Regexes[type];
        }
    }
}
