using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Jampiler.Core
{
    /// <summary>
    /// Convert jam code into tokens.
    /// </summary>
    public class Lexer
    {
        // Variations of file line endings.
        private readonly Regex _lineEnd = new Regex(@"\r\n|\r|\n");
        private readonly IList<TokenDefinition> _tokenDefinitions = new List<TokenDefinition>(); 

        public void AddDefinition(TokenDefinition definition)
        {
            _tokenDefinitions.Add(definition);
        }

        /// <summary>
        /// Convert jam source into tokens.
        /// </summary>
        /// <param name="source">Source code</param>
        /// <returns>Tokens representing source code</returns>
        public IEnumerable<Token> Tokenize(string source)
        {
            var currentIndex = 0;
            var currentLine = 1;
            var currentColumn = 0;

            while (currentIndex < source.Length) // Scan all of the source code, match each section with regex
            {
                TokenDefinition matchedDefinition = null;
                var matchLength = 0;

                // Search for a match in the lexers token definitions
                foreach (var rule in _tokenDefinitions)
                {
                    var match = rule.Regex.Match(source, currentIndex);

                    // Match is only valid if it starts at the current index
                    if (!match.Success || (match.Index - currentIndex) != 0)
                        continue;

                    // If a match is found, store it and the length
                    matchedDefinition = rule;
                    matchLength = match.Length;

                    break;
                }

                if (matchedDefinition == null) // If the source code matched nothing then it is invalid
                {
                    throw new Exception(
                        string.Format(
                            "Unrecognised input {0}\nFound at line {2}:{3} (current index: {1})", source[currentIndex],
                            currentIndex, currentLine, currentColumn));
                }
                else
                {
                    // Copy the source code matching the regex into the tokens value
                    var value = source.Substring(currentIndex, matchLength);

                    if (!matchedDefinition.Ignore) // Don't return token if it is meant to be ignored (e.g. whitespace at start/end of line)
                    {
                        yield return new Token(matchedDefinition.Type, value, new TokenPosition(currentIndex, currentLine, currentColumn));
                    }

                    // If we've reached the end of the line move onto the next one
                    // Otherwise continue through the source code columns
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

            // Return an end of file token to terminate the token list
            yield return new Token(TokenType.EndOfFile, null, new TokenPosition(currentIndex, currentLine, currentColumn));
        }
    }
}
