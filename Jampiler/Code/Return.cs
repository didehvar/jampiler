using System.Collections.Generic;

namespace Jampiler.Code
{
    /// <summary>
    /// Representation of a return statement.
    /// </summary>
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