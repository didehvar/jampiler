using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jampiler.Core
{
    public struct Token
    {
        public Regex Regex { get; set; }
        public string Type { get; set; }
    }
}
