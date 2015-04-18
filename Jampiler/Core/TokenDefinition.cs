using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jampiler.Core
{
    public class TokenDefinition
    {
        public TokenDefinition(TokenTypes type, Regex regex, bool ignore = false)
        {
            Type = type;
            Regex = regex;
            Ignore = ignore;
        }

        public Regex Regex { get; set; }

        public TokenTypes Type { get; set; }

        public bool Ignore { get; set; }
    }
}
