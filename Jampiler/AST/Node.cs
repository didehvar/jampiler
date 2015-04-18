using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jampiler.Core;

namespace Jampiler.AST
{
    public class Node
    {
        public TokenType Type { get; set; }

        public string Value { get; set; }

        public Node Left { get; set; } = null;

        public Node Right { get; set; } = null;

        public Node(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public Node(TokenType type, string value, Node left, Node right) : this(type, value)
        {
            Left = left;
            Right = right;
        }

        public Node(Token token) : this(token.Type, token.Value) { }

        public Node(Token token, Node left, Node right) : this(token)
        {
            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return
                string.Format(
                    "Node: {{@Type: '{0}'@Value: '{1}'@Left: {2} @Right: {3}}}", Type, Value,
                    Left?.ToString().Replace("\t", "\t\t") ?? "null", Right?.ToString().Replace("\t", "\t\t") ?? "null")
                    .Replace("@", Environment.NewLine + "\t");
        }
    }
}
