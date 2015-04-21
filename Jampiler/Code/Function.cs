using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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
        public List<Data> Registers { get; set; }

        private readonly List<string> _lines = new List<string>();

        private int _count = 0;

        private Function() { }

        public Function(Node startNode, string name)
        {
            StartNode = startNode;
            Name = name;
            Registers = new List<Data>();
        }

        public void AddReturn(Return ret)
        {
            _lines.Add(ret.Text());
        }

        public string Text()
        {
            return string.Format(
                "{0}\tbx lr\n",
                _lines.Aggregate(string.Format(".global\t{0}\n{0}:\n", Name), (current, line) => current + line));
        }

        public string DataName()
        {
            return string.Format("{0}{1}", Name, _count++.ToString());
        }
    }
}