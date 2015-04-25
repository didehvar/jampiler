using System;

namespace Jampiler.Core
{
    /// <summary>
    /// Type of token, used by the lexer and parser to determine correct source code.
    /// </summary>
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
        Return,
        End,
        Function,
        Block,
        Comment,
        If,
        EndIf,
        Else,
        Then,
        While,
        EndWhile,
        EndOfFile
    };

    /// <summary>
    /// A token is the representation of a non-terminal symbol.
    /// </summary>
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
