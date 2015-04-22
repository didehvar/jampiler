using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Jampiler.AST;
using Jampiler.Core;

namespace Jampiler.Code
{
    public class Function
    {
        public Node StartNode { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Element position represents the register in use.
        /// </summary>
        private readonly List<Data> _registers;

        private readonly List<string> _lines = new List<string>();

        private int _count = 0;
        private int _regDeleteStart = 0;
        private int _regMax = 0;

        private Function() { }

        public Function(Node startNode, string name)
        {
            StartNode = startNode;
            Name = name;
            _registers = new List<Data>();
        }

        public void AddReturn(Return ret)
        {
            foreach (var l in ret.Lines())
            {
                _lines.Add(l);
            }
        }

        public string Text()
        {
            var s = string.Format(".global\t{0}\n{0}:\n", Name);
            var pop = "";

            // Store result of r4-r12 if used
            if (_regMax > 3)
            {
                s += "\tpush {r4";
                pop += "\tpop {r4";

                for (var i = 5; i < _regMax; i++)
                {
                    var format = string.Format(", r{0}", i);

                    s += format;
                    pop += format;
                }

                s += "}\n\n";
                pop += "}\n";
            }

            s += string.Format("{0}\n", _lines.Aggregate("", (current, line) => current + line));
            s += pop;
            s += "\tbx lr\n";

            return s;
        }

        public string DataName()
        {
            return string.Format("{0}{1}", Name, _count++.ToString());
        }

        public void StartRegisterStore()
        {
            _regDeleteStart = _registers.Count;
        }

        public void EndRegisterStore()
        {
            UpdateRegCount();
            _registers.RemoveRange(_regDeleteStart, _registers.Count - _regDeleteStart);
        }

        public int RegisterCount()
        {
            return _registers.Count;
        }

        public int AddRegister(Data register)
        {
            _registers.Add(register);
            return _registers.Count - 1;
        }

        public void DeleteRegister(int index)
        {
            UpdateRegCount();
            _registers.RemoveAt(index);
        }

        private void UpdateRegCount()
        {
            var regCount = _registers.Count - 1;
            if (regCount >= 4 && _regMax < regCount)
            {
                _regMax = regCount;
            }
        }
    }
}