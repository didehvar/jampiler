using System;
using System.Collections.Generic;

namespace Jampiler.Code
{
    public class Statement : Data
    {
        public Function Parent { get; set; }

        public int? Register { get; set; }

        public List<Data> Datas { get; set; }

        public Statement(Function parent) : base(DataType.Statement)
        {
            Parent = parent;

            Register = null;
            Type = null;
        }

        public string LoadText()
        {
            if (Register == null || Value == null)
            {
                return null;
            }

            if (Datas != null)
            {
                Console.WriteLine("ASSEMBLING DATA INTO {0}", Register);
                Parent.AssembleData(Datas, true, false, Register);
                return "";
            }

            if (Type == DataType.Asciz)
            {
                return string.Format("\tldr r{0}, addr_{1}\n", Register, Name);
            }

            return string.Format("\tmov r{0}, {1}\n", Register, string.Format("#{0}", Value));
        }

        public override string ToString()
        {
            return string.Format("{0}\t{1}", base.ToString(), Register);
        }
    }
}