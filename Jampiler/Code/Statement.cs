using System.Collections.Generic;

namespace Jampiler.Code
{
    /// <summary>
    /// Representation of a statement.
    /// </summary>
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

        /// <summary>
        /// Load the statement into a register that was previously assigned by the parent.
        /// </summary>
        /// <returns>Assembly code for initialisation of this statement</returns>
        public string LoadText()
        {
            // Can't convert a statement with no assigned register or value into assembly
            if (Register == null || Value == null)
            {
                return null;
            }

            // If this statement contains a data list then the list must be assembled by the parent
            if (Datas != null)
            {
                Parent.AssembleData(Datas, true, false, Register);
                return "";
            }

            // ldr must be used for strings
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