using System;
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

        public List<string> Lines()
        {
            if (Data == null)
            {
                throw new NotImplementedException("TODO: exit functions");
            }

            if (Data.Count == 1) // If there is only one data piece there's nothing to calculate
            {
                _lines.Add(ParseData(Data.First(), true));
                return _lines;
            }

            Parent.StartRegisterStore();
            var startRegister = Parent.Registers.Count;
            var currentRegister = startRegister;

            Console.WriteLine();
            foreach (var d in Data)
            {
                Console.WriteLine(d.ToString());
            }
            Console.WriteLine();

            var first = Data.ElementAt(0);
            _lines.Add(ParseData(first));

            for (var i = 1; i < Data.Count - 1; i = i + 2) // Exclude first and last element
            {
                var right = Data.ElementAt(i + 1);
                var op = Data.ElementAt(i);

                _lines.Add(ParseData(right));

                // Left element is in register [start register]
                // Right element is in register [start register + 1]
                AddLine(op.Value, startRegister, ++currentRegister);
            }

            Parent.EndRegisterStore();
            return _lines;
        }

        private string ParseData(Data data, bool isReturn = false)
        {
            // If the data has a type, we must use its address
            if (!string.IsNullOrEmpty(data.Type))
            {
                Parent.Registers.Add(data);
                var count = isReturn ? 0 : Parent.Registers.Count - 1;

                return string.Format("\tldr r{0}, addr_{1}\n" + "\tldr r{0}, [r{0}]\n", count, data.Name);
            }

            // Data without a type is just a number
            if (!string.IsNullOrEmpty(data.Value))
            {
                Parent.Registers.Add(data);
                var count = isReturn ? 0 : Parent.Registers.Count - 1;

                return string.Format("\tmov r{0}, #{1}\n", count, data.Value);
            }

            throw new NotImplementedException("Data not supported");
        }

        private void AddLine(string oper, int firstRegister, int lastRegister)
        {
            switch (oper)
            {
                case "+":
                    _lines.Add(string.Format("\tadd r{0}, r{0}, r{1}\n", firstRegister, lastRegister));
                    break;
            }
        }
    }
}