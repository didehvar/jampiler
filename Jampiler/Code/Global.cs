using System.Collections.Generic;

namespace Jampiler.Code
{
    /// <summary>
    /// Used for management of global variables.
    /// </summary>
    public class Global : Data
    {
        public List<Data> Datas { get; set; }

        public Global() : base(DataType.Global) {}

        public Global(List<Data> data) : base(DataType.Global)
        {
            Datas = data;
        }
    }
}