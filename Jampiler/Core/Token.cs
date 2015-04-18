using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jampiler.Core
{
    public enum TokenType
    {
        Operator,
        Whitespace,
        Number,
        String,
        Nil,
        False,
        True,
        Identifier,
        Local,
        Equals,
        OpenBracket,
        CloseBracket,
        Comma,
        EndOfFile
    };

    public class Token
    {
        public Token(TokenType type, string value, TokenPosition tokenPosition)
        {
            Type = type;
            Value = value;
            Position = tokenPosition;
        }

        public TokenPosition Position { get; set; }

        public TokenType Type { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return
                string.Format(
                    "Token: {{@Type: '{0}'@Value: '{1}'@{2} }}", Type, Value, Position.ToString().Replace("\t", "\t\t"))
                    .Replace("@", Environment.NewLine + "\t");
        }
    }
}
