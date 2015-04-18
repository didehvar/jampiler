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
            Regexes.Add(TokenType.Whitespace, new Regex(@"\s+"));
            Regexes.Add(TokenType.Digit, new Regex(@"[0-9]"));
            Regexes.Add(TokenType.Operator, new Regex(@"\+|-|\*|\/|<|>|>=|<=|==|!=|and|or", RegexOptions.IgnoreCase));
        }

        public static TokenRegex Instance => _instance ?? (_instance = new TokenRegex());

        public Dictionary<TokenType, Regex> Regexes = new Dictionary<TokenType, Regex>();

        public Regex GetRegex(TokenType type)
        {
            return Regexes[type];
        }
    }
}
