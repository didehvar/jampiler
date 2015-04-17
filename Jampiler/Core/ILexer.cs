using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jampiler.Core
{
    public interface ILexer
    {
        void AddDefinition(Token definition);

        IEnumerable<Token> Tokenize(string source);
    }
}
