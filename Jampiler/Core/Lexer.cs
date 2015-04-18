using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jampiler.Core
{
    public class Lexer
    {
        private readonly Regex _lineEnd = new Regex(@"\r\n|\r|\n");
        private readonly IList<TokenDefinition> _tokenDefinitions = new List<TokenDefinition>(); 

        public void AddDefinition(TokenDefinition definition)
        {
            _tokenDefinitions.Add(definition);
        }

        public IEnumerable<Token> Tokenize(string source)
        {
            var currentIndex = 0;
            var currentLine = 1;
            var currentColumn = 0;

            while (currentIndex < source.Length)
            {
                TokenDefinition matchedDefinition = null;
                var matchLength = 0;

                foreach (var rule in _tokenDefinitions)
                {
                    var match = rule.Regex.Match(source, currentIndex);

                    if (!match.Success || (match.Index - currentIndex) != 0)
                        continue;

                    matchedDefinition = rule;
                    matchLength = match.Length;

                    break;
                }

                if (matchedDefinition == null)
                {
                    throw new Exception(string.Format("Unrecognised input :> {0} <: at {1} (line: {2}:{3})", source[currentIndex], currentIndex, currentLine, currentColumn));
                }
                else
                {
                    var value = source.Substring(currentIndex, matchLength);

                    if (!matchedDefinition.Ignore)
                    {
                        yield return new Token(matchedDefinition.Type, value, new TokenPosition(currentIndex, currentLine, currentColumn));
                    }

                    var lineEndMatch = _lineEnd.Match(value);
                    if (lineEndMatch.Success)
                    {
                        currentLine++;
                        currentColumn = value.Length - (lineEndMatch.Index + lineEndMatch.Length);
                    }
                    else
                    {
                        currentColumn += matchLength;
                    }

                    currentIndex += matchLength;
                }
            }

            yield return new Token(TokenType.EndOfFile, null, new TokenPosition(currentIndex, currentLine, currentColumn));
        }
    }
}
