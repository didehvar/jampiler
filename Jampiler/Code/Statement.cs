namespace Jampiler.Code
{
    public class Statement : Data
    {
        public Function Parent { get; set; }

        public int? Register { get; set; }

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

            return string.Format(
                "\tmov r{0}, {1}\n", Register,
                Type == DataType.Asciz ? string.Format("addr_{0}", Name) : string.Format("#{0}", Value));
        }

        public override string ToString()
        {
            return string.Format("{0}\t{1}", base.ToString(), Register);
        }
    }
}