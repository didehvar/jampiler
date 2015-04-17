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
        public Regex Regex { get; set; }

        public string Type { get; set; }

        public bool Ignore { get; set; }
    }
}
