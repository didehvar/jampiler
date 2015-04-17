using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jampiler.Core
{
    public class Token
    {
        public Token(string type, string value, TokenPosition tokenPosition)
        {
            Type = type;
            Value = value;
            Position = tokenPosition;
        }

        public TokenPosition Position { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }
    }
    }
    }
}
