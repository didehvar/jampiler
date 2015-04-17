using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jampiler.Symbol
{
    public class TerminalSymbol : Symbol
    {
        public string Value { get; set; }

        public TerminalSymbol(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
