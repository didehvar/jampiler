using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jampiler.Core
{
    public class Parser
    {
        private Token _currentToken;
        private int _currentIndex;

        private readonly IEnumerable<Token> _tokens;

        public Parser(IEnumerable<Token> tokens)
        {
            _tokens = tokens;
            _currentIndex = -1;

            NextToken();

            Expression();
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

        private void Expression()
        {
            if (Accept(TokenType.Digit))
            {
                Expect(TokenType.Operator);
                Expect(TokenType.Digit);
            }
            else
            {
                throw new Exception("Unexpected token");
            }
        }
    }
}
