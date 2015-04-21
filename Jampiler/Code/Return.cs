using System;

namespace Jampiler.Code
{
    public class Return
    {
        public Function Parent { get; set; }

        public Data Data { get; set; }

        private Return() { }

        public Return(Function parent)
        {
            Parent = parent;
        }

        public string Text()
        {
            if (Data == null)
            {
                throw new NotImplementedException("TODO: exit functions");
            }

            // If the data has a type, we must use its address
            if (!string.IsNullOrEmpty(Data.Type))
            {
                Parent.Registers.Add(Data);

                var count = Parent.Registers.Count - 1;
                var s = string.Format("\tldr r{0}, addr_{1}\n" + "\tldr lr, [r{0}]\n", count, Data.Name);

                Parent.Registers.RemoveAt(count);

                return s;
            }

            // Data without a type is just a number
            if (!string.IsNullOrEmpty(Data.Value))
            {
                return string.Format("\tmov r0, {0}\n", Data.Value);
            }

            throw new NotImplementedException("Data not supported");
        }
    }
}