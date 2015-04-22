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
            var startRegister = Parent.RegisterCount();
            var currentRegister = startRegister;

            Console.WriteLine();
            foreach (var d in Data)
            {
                Console.WriteLine(d.ToString());
            }
            Console.WriteLine();

            var first = Data.ElementAt(0);

            var firstStatement = TryParseStatement(first);
            _lines.Add(
                firstStatement != null
                    ? string.Format("\tmov r{0}, r{1}\n", startRegister, firstStatement)
                    : ParseData(first));

            for (var i = 1; i < Data.Count - 1; i = i + 2) // Exclude first and last element
            {
                var right = Data.ElementAt(i + 1);
                var op = Data.ElementAt(i);

                var statementReg = TryParseStatement(right);
                if (statementReg != null)
                {
                    AddLine(op.Value, startRegister, statementReg.Value);
                }
                else
                {
                    // Left element is in register [start register]
                    // Right element is in register [start register + 1]
                    _lines.Add(ParseData(right));

                    AddLine(op.Value, startRegister, ++currentRegister);
                }
            }

            // If register 0 isn't used as the final data store for the operation, the data must be moved into r0
            if (startRegister != 0)
            {
                _lines.Add(string.Format("\tmov r0, r{0}", startRegister));
            }

            Parent.EndRegisterStore();
            return _lines;
        }

        private int? TryParseStatement(Data data)
        {
            if (!(data is Statement))
            {
                return null;
            }

            var statement = (Statement) data;
            if (statement.Register == null)
            {
                throw new Exception("Can't add to null");
            }

            return statement.Register;

            // Statement is already in a register, we need to add to that register
        }

        private string ParseData(Data data, bool isReturn = false)
        {
            // If the data has a type, we must use its address
            if (data.Type != null)
            {
                return string.Format(
                    "\tldr r{0}, addr_{1}\n" + "\tldr r{0}, [r{0}]\n", isReturn ? 0 : Parent.AddRegister(data),
                    data.Name);
            }

            // Data without a type is just a number
            if (!string.IsNullOrEmpty(data.Value))
            {
                return string.Format("\tmov r{0}, #{1}\n", isReturn ? 0 : Parent.AddRegister(data), data.Value);
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