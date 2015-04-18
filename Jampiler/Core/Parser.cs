using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jampiler.AST;

namespace Jampiler.Core
{
    public class Parser
    {
        private Token _currentToken = null;
        private Token _lastToken = null;
        private int _currentIndex;

        private readonly IEnumerable<Token> _tokens;

        public Parser(IEnumerable<Token> tokens)
        {
            _tokens = tokens;
            _currentIndex = -1;

            NextToken();

            Console.WriteLine(Expression().ToString());
        }

        private void NextToken()
        {
            if (_currentIndex >= _tokens.Count())
            {
                // Do something when the last token is reached
                throw new Exception("TODO");
            }

            do
            {
                // Only update the last token if we aren't throwing away the next token
                if (_currentToken != null && _currentToken.Type != TokenType.Whitespace)
                {
                    _lastToken = _currentToken;
                }

                _currentToken = _tokens.ElementAt(++_currentIndex);
            } while (_currentToken.Type == TokenType.Whitespace); // Skip whitespace tokens
        }

        private bool Accept(TokenType type)
        {
            if (_currentToken.Type != type)
            {
                return false;
            }

            NextToken();
            return true;
        }

        private bool Expect(TokenType type)
        {
            if (!Accept(type))
            {
                throw new Exception("Unexpected token");
            }

            return true;
        }

        private Node Expression()
        {
            // expression = 'nil' | 'false' | 'true' | number | string, [ operator, expression ];

            var left = _currentToken;
            if (left.Type == TokenType.Nil || left.Type == TokenType.False || left.Type == TokenType.True ||
                left.Type == TokenType.Number || left.Type == TokenType.String)
            {
                // Consume the token for the first part of the expression
                NextToken();

                // If the next token is an operator, then there is also an expression()
                // Last token would be the operator as accept consumes a token
                return Accept(TokenType.Operator)
                    ? new Node(_lastToken.Type, _lastToken.Value, new Node(left.Type, left.Value), Expression())
                    : new Node(left.Type, left.Value);
            }
            else
            {
                throw new Exception("Unexpected token");
            }
        }
    }
}
