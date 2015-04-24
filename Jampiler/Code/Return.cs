using System.Collections.Generic;

namespace Jampiler.Code
{
    public class Return
    {
        public Function Parent { get; set; }

        public List<Data> Data { get; set; }

        public Return(Function parent)
        {
            Parent = parent;
        }
    }
}