﻿using System;

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

        public override string ToString()
        {
            return
                string.Format("Position: {{@Column: '{0}'@Index: '{1}'@Line: '{2}' }}", Column, Index, Line)
                    .Replace("@", Environment.NewLine + "\t");
        }
    }
}
