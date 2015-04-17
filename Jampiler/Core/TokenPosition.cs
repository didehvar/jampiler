using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jampiler.Core
{
    public class TokenPosition
    {
        public TokenPosition(int currentIndex, int currentLine, int currentColumn)
        {
            Column = currentIndex;
            Index = currentLine;
            Line = currentColumn;
        }

        public int Column { get; set; }

        public int Index { get; set; }

        public int Line { get; set; }
    }
}
