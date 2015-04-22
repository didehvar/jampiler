using System;
using System.Net.Mime;
using System.Runtime.InteropServices;

namespace Jampiler.Code
{
    public enum DataType
    {
        Asciz,
        Operator
    }

    public class Data
    {
        public DataType? Type;
        public string Value;
        public string Name;

        public Data() { }

        public string Text()
        {
            if (Type == null || Value == null || Name == null)
            {
                return "";
            }

            return string.Format("{0}: {1}\t{2}", Name, Type, Value);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}\t{2}", Name, Type, Value);
        }

        public string StringType()
        {
            if (Type == null)
            {
                throw new Exception("No type");
            }

            return Type.ToString().ToLower();
        }
    }
}