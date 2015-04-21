namespace Jampiler.Code
{
    public class Data
    {
        public string Type;
        public string Value;
        public string Register;
        public string Name;

        private Data() { }

        public Data(string type, string value, string name = "")
        {
            Type = type;
            Value = value;
            Register = register;
            Name = name;
        }
    }
}