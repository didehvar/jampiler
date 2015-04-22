using System;
using System.Net.Mime;
using System.Security.Cryptography;
using Jampiler.Core;

namespace Jampiler.Code
{
    public class Statement : Data
    {
        public Function Parent { get; set; }

        public int? Register { get; set; }

        public Statement(Function parent)
        {
            Parent = parent;

            Register = null;
            Type = null;
        }

        public string LoadText()
        {
            if (Register == null || Value == null)
            {
                return "";
            }

            return string.Format(
                "\tmov r{0}, {1}\n", Register,
                Type == DataType.Asciz ? string.Format("addr_{0}", Name) : string.Format("#{0}", Value));
        }

        public override string ToString()
        {
            return string.Format("{0}\n{1}\n{2}\n{3}", Register, Name, Value, Type);
        }
    }
}