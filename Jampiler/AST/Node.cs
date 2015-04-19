using System;
using Jampiler.Core;

namespace Jampiler.AST
{
    public class Node
    {
        public TokenType Type { get; set; }

        public string Value { get; set; }

        public Node Left { get; set; }

        public Node Right { get; set; }

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
            return string.Format("{{ {0} }} {{ {1} }}", Type, Value);
        }

        public void Print(string indent = "", string prefix = "N")
        {
            Console.WriteLine(string.Format("{0}{1} {2}", indent, prefix, ToString()));

            indent += "  ";
            var copy = indent.Clone().ToString();

            Left?.Print(indent, "L");
            Right?.Print(copy, "R");
        }
    }
}
