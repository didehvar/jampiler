using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jampiler.Core
{
    public enum TokenTypes
    {
        Operator,
        Whitespace,
        Digit,
        EndOfFile
    };

    public class Token
    {
        public Token(TokenTypes type, string value, TokenPosition tokenPosition)
        {
            Type = type;
            Value = value;
            Position = tokenPosition;
        }

        public TokenPosition Position { get; set; }

        public TokenTypes Type { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return
                string.Format((string) "Token: {{@Type: '{0}'@Value: '{1}'@{2} }}", Type, Value, Position.ToString().Replace("\t", "\t\t"))
                    .Replace("@", Environment.NewLine + "\t");
        }
    }
}
