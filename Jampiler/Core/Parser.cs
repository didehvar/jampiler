using System;
using System.Collections.Generic;
using System.Linq;
using Jampiler.AST;

namespace Jampiler.Core
{
    public class Parser
    {
        private Token _currentToken;
        private Token _lastToken;
        private int _currentIndex;

        private IEnumerable<Token> _tokens;

        public List<Node> Parse(IEnumerable<Token> tokens)
        {
            _tokens = tokens;
            _currentIndex = -1;

            NextToken();

            var tree = new List<Node>();
            while (_currentToken.Type == TokenType.Function || _currentToken.Type == TokenType.Identifier)
            {
                tree.Add(_currentToken.Type == TokenType.Function ? Function() : Statement(true));
            }

            return tree;
        }

        private void NextToken()
        {
            if (_currentIndex >= _tokens.Count())
            {
                // Do something when the last token is reached
                throw new Exception("TODO");
            }

            // Skip whitespace and comments
            do
            {
                // Only update the last token if we aren't throwing away the next token
                if (_currentToken != null && _currentToken.Type != TokenType.Whitespace &&
                    _currentToken.Type != TokenType.Comment)
                {
                    _lastToken = _currentToken;
                }

                _currentToken = _tokens.ElementAt(++_currentIndex);
            } while (_currentToken.Type == TokenType.Whitespace || _currentToken.Type == TokenType.Comment);
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

        private bool Accept(List<TokenType> types)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            if (!types.Contains(_currentToken.Type))
            {
                return false;
            }

            NextToken();
            return true;
        }

        private void Expect(TokenType type)
        {
            if (!Accept(type))
            {
                throw new Exception("Unexpected token");
            }
        }

        private void Expect(List<TokenType> types)
        {
            if (!Accept(types))
            {
                throw new Exception("Unexpected token");
            }
        }

        private Node Expression()
        {
            // expression = 'nil' | 'false' | 'true' | number | string | identifier, [ operator, expression ];

            var left = _currentToken;
            if (left.Type == TokenType.Nil || left.Type == TokenType.False || left.Type == TokenType.True ||
                left.Type == TokenType.Number || left.Type == TokenType.String || left.Type == TokenType.Identifier)
            {
                // Consume the token for the first part of the expression
                NextToken();

                // If the next token is an operator, then there is also an expression()
                // Last token would be the operator as accept consumes a token
                return Accept(TokenType.Operator) ? new Node(_lastToken, new Node(left), Expression()) : new Node(left);
            }

            throw new Exception("Unexpected token");
        }

        private Node Statement(bool global = false)
        {
            // statement =  [ ‘local’ ], identifier, [ ( ‘=‘, expression ) | arg list ];
            //      | identifier, arg list
            //      | 'if', expression, block, [ 'else', block ], 'end if';

            var left = _currentToken;
            if (Accept(TokenType.Local) && !global)
            {
                // [ ‘local’ ], identifier, ‘=‘, expression
                var identifier = _currentToken;
                Expect(TokenType.Identifier);

                var equals = _currentToken;

                // '=', expression
                if (Accept(TokenType.Equals))
                {
                    return new Node(identifier, new Node(equals, new Node(left), Expression()), null);
                }

                // arg list
                if (_currentToken.Type == TokenType.OpenBracket)
                {
                    return new Node(identifier, new Node(equals, new Node(left), ArgumentList()), null);
                }

                // [ ‘local’ ], identifier
                return new Node(identifier, new Node(left), null);
            }

            if (Accept(TokenType.Identifier))
            {
                // '=', expression
                if (Accept(TokenType.Equals))
                {
                    // expression
                    return new Node(left, new Node(_lastToken, null, Expression()), null);
                }

                if (global)
                {
                    throw new Exception("Can only perform an assignment for a global");
                }

                // arg list
                if (_currentToken.Type == TokenType.OpenBracket)
                {
                    return new Node(left, ArgumentList(), null);
                }

                // identifier
                return new Node(left);
            }

            if (Accept(TokenType.If))
            {
                // First if branch
                var comparison = Expression();

                Expect(TokenType.Then);

                var node = new Node(TokenType.If, "if") { Left = Block() };
                node.Left.Left = comparison;

                Expect(TokenType.EndIf);

                return node;
            }

            throw new Exception("Unexpected token");
        }

        private Node Block()
        {
            // block = { statement }, [ return statement ];

            // If this isn't a statement/return/end then the token is unexpected
            if (_currentToken.Type != TokenType.Local && _currentToken.Type != TokenType.Identifier &&
                _currentToken.Type != TokenType.Return && _currentToken.Type != TokenType.End)
            {
                throw new Exception("Unexpected token");
            }

            var node = new Node(TokenType.Block, "");
            var nextNode = node;

            // Parse statements until the end is reached
            while (_currentToken.Type != TokenType.End && _currentToken.Type != TokenType.EndIf)
            {
                switch (_currentToken.Type) {
                    case TokenType.Return:
                        nextNode.Right = ReturnStatement();
                        continue; // Exit loop, nothing after return statement

                    case TokenType.Local:
                    case TokenType.Identifier:
                    case TokenType.If:
                        nextNode.Right = Statement();
                        nextNode = nextNode.Right;
                        break;

                    case TokenType.OpenBracket:
                        nextNode.Right = ArgumentList();
                        nextNode = nextNode.Right;
                        break;

                    default:
                        throw new Exception("Unexpected token");
                }
            }

            return node;
        }

        private Node Function()
        {
            // function = 'function’, identifier, arg list, block, 'end';

            Expect(TokenType.Function);
            var func = _lastToken;

            Expect(TokenType.Identifier);
            var identifier = _lastToken;

            var args = ArgumentList();

            var block = Block();
            block.Left = args;

            Expect(TokenType.End);

            return new Node(func, new Node(identifier), block);
        }

        private Node ReturnStatement()
        {
            // return statement = 'return’ expression;

            Expect(TokenType.Return);
            return new Node(_lastToken, Expression(), null);
        }

        private Node ArgumentList()
        {
            // arg list = '(', [ args ], ')’;

            Expect(TokenType.OpenBracket);

            var args = new Node(TokenType.Whitespace, "");
            if (_currentToken.Type != TokenType.CloseBracket)
            {
                args = Arguments();
            }

            Expect(TokenType.CloseBracket);

            return args;
        }

        private Node Argument()
        {
            // argument = identifier | string | number;

            Expect(new List<TokenType>() { TokenType.Identifier, TokenType.String, TokenType.Number });
            return new Node(_lastToken);
        }

        private Node Arguments()
        {
            // args = argument, { ',’, argument };

            var node = Argument();
            var nextNode = node;

            while (Accept(TokenType.Comma))
            {
                nextNode.Right = Argument();
                nextNode = nextNode.Right;
            }

            return node;
        }
    }
}
