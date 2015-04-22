using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Jampiler.Core
{
    public class TokenRegex
    {
        private static TokenRegex _instance;

        private TokenRegex()
        {
            Regexes.Add(TokenType.Comment, new Regex(@"//.*"));

            Regexes.Add(TokenType.Whitespace, new Regex(@"\s+"));
            Regexes.Add(TokenType.String, new Regex(@"""[^""]+"""));
            Regexes.Add(TokenType.Number, new Regex(@"[0-9]+"));
            Regexes.Add(TokenType.Operator, new Regex(@"\+|-|\*|\/|<|>|>=|<=|==|!=|and|or|AND|OR"));

            // Characters
            Regexes.Add(TokenType.Equals, new Regex(@"="));
            Regexes.Add(TokenType.OpenBracket, new Regex(@"\("));
            Regexes.Add(TokenType.CloseBracket, new Regex(@"\)"));
            Regexes.Add(TokenType.Comma, new Regex(@","));

            // Keywords
            Regexes.Add(TokenType.Nil, new Regex(@"nil|NIL"));
            Regexes.Add(TokenType.False, new Regex(@"false|FALSE"));
            Regexes.Add(TokenType.True, new Regex(@"true|TRUE"));
            Regexes.Add(TokenType.Local, new Regex(@"local"));
            Regexes.Add(TokenType.Return, new Regex(@"return"));
            Regexes.Add(TokenType.End, new Regex(@"end"));
            Regexes.Add(TokenType.Function, new Regex(@"function"));

            Regexes.Add(TokenType.Identifier, new Regex(@"[a-zA-Z_]\w*")); // Must come after keywords
        }

        public static TokenRegex Instance => _instance ?? (_instance = new TokenRegex());

        public Dictionary<TokenType, Regex> Regexes = new Dictionary<TokenType, Regex>();

        public Regex GetRegex(TokenType type)
        {
            return Regexes[type];
        }
    }
}
