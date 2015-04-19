using System.Text.RegularExpressions;

namespace Jampiler.Core
{
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
