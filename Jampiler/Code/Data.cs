using System;

namespace Jampiler.Code
{
    public enum DataType
    {
        Number,
        Asciz,
        Operator,
        Global,
        Statement,
        Nil,
        Return,
        Word
    }

    public class Data
    {
        public DataType? Type;
        public string Value;
        public string Name;

        public Data(DataType? type)
        {
            Type = type;
        }

        public string Text()
        {
            if (Type == null || Value == null || Name == null)
            {
                return "";
            }

            var type = Type.ToString().ToLower();
            if (Type == DataType.Word)
            {
                type = "word";
            }

            return string.Format("{0}: .{1}\t{2}\n", Name, type, Value);
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