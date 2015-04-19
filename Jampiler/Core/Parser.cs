using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Xsl;
using Jampiler.AST;

namespace Jampiler.Core
{
    public class Parser
    {
        private Token _currentToken = null;
        private Token _lastToken = null;
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

        private bool Expect(List<TokenType> types)
        {
            if (types.All(t => t != _currentToken.Type)) {
                throw new Exception("Unexpected token");
            }

            NextToken();
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
                    ? new Node(_lastToken, new Node(left), Expression())
                    : new Node(left);
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

                return new Node(equals, new Node(identifier, new Node(left), null), new Node(_lastToken));
            }
            else if (Accept(TokenType.Identifier))
            {
                // '=', expression
                if (Accept(TokenType.Equals))
                {
                    // expression
                    return new Node(_lastToken, new Node(left), Expression());
                }

                // arg list
                return new Node(left) { Right = ArgumentList() };
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

            Node node = null;

            // Parse statements until the end is reached
            while (_currentToken.Type != TokenType.End)
            {
                switch (_currentToken.Type) {
                    case TokenType.Return:
                        if (node == null)
                        {
                            node = ReturnStatement();
                        }
                        else
                        {
                            node.Right = ReturnStatement();
                        }

                        continue; // Exit loop, nothing after return statement
                    case TokenType.Local:
                    case TokenType.Identifier:
                        if (node == null)
                        {
                            node = Statement();
                        }
                        else
                        {
                            node.Right = Statement();
                        }

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
            // function = ‘function’, identifier, function body;
            Expect(TokenType.Function);
            Expect(TokenType.Identifier);

            return FunctionBody();
        }

        private Node FunctionBody()
        {
            // function body = arg list, block;
            var args = ArgumentList();
            var block = Block();

            block.Left = args;
            return block;
        }

        private Node ReturnStatement()
        {
            // return statement = ‘return’, { args };

            Expect(TokenType.Return);
            return new Node(_lastToken, Statement(), Arguments());
        }

        private Node ArgumentList()
        {
            // arg list = ‘(‘, [ args ], ‘)’;

            Expect(TokenType.OpenBracket);
            var openBracket = _lastToken;

            if (_currentToken.Type != TokenType.Identifier) // Empty argument list ()
            {
                Expect(TokenType.CloseBracket);
                return null; // No node
            }

            var args = Arguments();
            Expect(TokenType.CloseBracket);

            return args;
        }

        private Node Arguments()
        {
            // args = identifier, { ‘,’, identifier };

            Expect(TokenType.Identifier);

            var node = new Node(_lastToken); // The first identifier is stored in a node
            var comma = _currentToken;

            while (Accept(TokenType.Comma))
            {
                Expect(TokenType.Identifier);

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
                    var topNode = new Node(_lastToken) { Left = node };
                    node = topNode;
                }
                else if (node.Right == null)
                {
                    node.Right = new Node(_lastToken);
                }
                else
                {
                    Node prevNode = null;
                    var nextNode = node;

                    do
                    {
                        prevNode = nextNode;
                        nextNode = nextNode.Right;
                    } while (nextNode.Left != null && nextNode.Right != null);

                    if (nextNode.Left == null)
                    {
                        var topNode = new Node(_lastToken) { Left = nextNode };
                        prevNode.Right = topNode;
                    }
                    else
                    {
                        nextNode.Right = new Node(_lastToken);
                    }
                }
            }

            return node;
        }
    }
}
