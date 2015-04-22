﻿using System;
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

        public Node Parse(IEnumerable<Token> tokens)
        {
            _tokens = tokens;
            _currentIndex = -1;

            NextToken();

            return Function();
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

        private void Expect(TokenType type)
        {
            if (!Accept(type))
            {
                throw new Exception("Unexpected token");
            }
        }

        private void Expect(List<TokenType> types)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            if (types.All(t => t != _currentToken.Type)) {
                throw new Exception("Unexpected token");
            }

            NextToken();
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

        private Node Statement()
        {
            // statement = 'local’, identifier, [ '=', (string | number | identifier)]
            //           | identifier, '=', expression
            //           | identifier, arg list;

            var left = _currentToken;
            if (Accept(TokenType.Local))
            {
                // identifier, [ '=', (string | number | identifier)]
                var identifier = _currentToken;
                Expect(TokenType.Identifier);

                var equals = _currentToken;
                if (!Accept(TokenType.Equals)) // 'local’, identifier
                {
                    return new Node(identifier, new Node(left), null);
                }

                // (string | number | identifier)
                Expect(new List<TokenType>
                {
                    TokenType.String,
                    TokenType.Number,
                    TokenType.Identifier
                });

                return new Node(identifier, new Node(equals, new Node(left), new Node(_lastToken)), null);
            }
            else if (Accept(TokenType.Identifier))
            {
                // '=', expression
                if (Accept(TokenType.Equals))
                {
                    // expression
                    return new Node(left, new Node(_lastToken, null, Expression()), null);
                }

                // arg list
                return new Node(left, ArgumentList(), null);
            }

            throw new Exception("Unexpected token");
        }

        private Node Block()
        {
            // block = { statement }, [ return statement ], 'end';
            // statement = 'local’, identifier, [ '=', (string | number | identifier)]
            //           | identifier, '=', expression
            //           | identifier, arg list;

            // If this isn't a statement/return/end then the token is unexpected
            if (_currentToken.Type != TokenType.Local && _currentToken.Type != TokenType.Identifier &&
                _currentToken.Type != TokenType.Return && _currentToken.Type != TokenType.End)
            {
                throw new Exception("Unexpected token");
            }

            var node = new Node(TokenType.Block, "");
            var nextNode = node;

            // Parse statements until the end is reached
            while (_currentToken.Type != TokenType.End)
            {
                switch (_currentToken.Type) {
                    case TokenType.Return:
                        nextNode.Right = ReturnStatement();
                        continue; // Exit loop, nothing after return statement

                    case TokenType.Local:
                    case TokenType.Identifier:
                        nextNode.Right = Statement();
                        nextNode = nextNode.Right;
                        break;

                    default:
                        throw new Exception("Unexpected token");
                }
            }

            Expect(TokenType.End);

            return node;
        }

        private Node Function()
        {
            // function = ‘function’, identifier, arg list, block;

            Expect(TokenType.Function);
            var func = _lastToken;

            Expect(TokenType.Identifier);
            var identifier = _lastToken;

            var args = ArgumentList();

            var block = Block();
            block.Left = args;

            return new Node(func, new Node(identifier), block); // block
        }

        private Node ReturnStatement()
        {
            // return statement = ‘return’ expression;

            Expect(TokenType.Return);
            return new Node(_lastToken, Expression(), null);
        }

        private Node ArgumentList()
        {
            // arg list = ‘(‘, [ args ], ‘)’;

            Expect(TokenType.OpenBracket);

            if (_currentToken.Type != TokenType.Identifier) // Empty argument list ()
            {
                Expect(TokenType.CloseBracket);
                return null; // No node
            }

            var args = Arguments();
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
            // args = argument, { ‘,’, argument };

            var node = Argument();

            while (Accept(TokenType.Comma))
            {
                var newNode = Argument();

                // Need to complete the tree properly, where 1 is the first identifier and so on
                //
                // 1 =>
                //              2
                //          1
                // >=
                //              2
                //          1       3               
                // >=
                //              2
                //          1       4
                //                3
                // >=
                //              2
                //          1       4
                //                3   5
                // >=
                //              2
                //          1       4
                //                3   5
                //                   6
                //
                // And so on

                if (node.Left == null)
                {
                    newNode.Left = node;
                    node = newNode;
                }
                else if (node.Right == null)
                {
                    node.Right = newNode;
                }
                else
                {
                    Node prevNode;
                    var nextNode = node;

                    do
                    {
                        prevNode = nextNode;
                        nextNode = nextNode.Right;
                    } while (nextNode.Left != null && nextNode.Right != null);

                    if (nextNode.Left == null)
                    {
                        newNode.Left = nextNode;
                        prevNode.Right = newNode;
                    }
                    else
                    {
                        nextNode.Right = newNode;
                    }
                }
            }

            return node;
        }
    }
}
