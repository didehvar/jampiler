using System;

namespace Jampiler.Code
{
    /// <summary>
    /// Represents the type of data that the data class is carrying.
    /// </summary>
    public enum DataType
    {
        Number,
        Asciz,
        Operator,
        Global,
        Statement,
        Nil,
        Return,
        Word,
        Function
    }

    /// <summary>
    /// A data element: the result of a token being converted in code generation.
    /// Used to construct assembly in the .data section.
    /// </summary>
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
            switch (Type)
            {
                case DataType.Word:
                case DataType.Number:
                    type = "word";
                    break;
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