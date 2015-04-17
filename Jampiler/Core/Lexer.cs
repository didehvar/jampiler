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
        private Regex _lineEnd = new Regex(@"\r\n|\r|\n");
        private IList<TokenDefinition> _tokenDefinitions = new List<TokenDefinition>(); 

        void AddDefinition(TokenDefinition definition)
        {
            _tokenDefinitions.Add(definition);
        }

        IEnumerable<Token> Tokenize(string source)
        {
            int currentIndex = 0;
            int currentLine = 1;
            int currentColumn = 0;

            yield return new Token();
        }
    }
}
