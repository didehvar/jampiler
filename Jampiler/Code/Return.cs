using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Jampiler.Code
{
    public class Return
    {
        public Function Parent { get; set; }

        public List<Data> Data { get; set; }

        private readonly List<string> _lines = new List<string>();

        private Return() { }

        public Return(Function parent)
        {
            Parent = parent;
        }
    }
}