using System.Text.RegularExpressions;

namespace Jampiler.Core
{
    /// <summary>
    /// Definition for a token. Matches the regex for a token to a specific token type.
    /// </summary>
    public class TokenDefinition
    {
        public TokenDefinition(TokenType type, Regex regex, bool ignore = false)
        {
            Type = type;
            Regex = regex;
            Ignore = ignore;
        }

        public Regex Regex { get; set; }

        public TokenType Type { get; set; }

        public bool Ignore { get; set; }
    }
}
