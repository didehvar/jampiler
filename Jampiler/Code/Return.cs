using System;

namespace Jampiler.Code
{
    public class Return
    {
        public Data Data { get; set; }

        public string Value { get; set; }

        public string Text()
        {
            if (Data != null)
            {
                return string.Format("\tldr {0}, addr_{1}\n" + "\tldr, lr [{0}]", Data.Name, Data.Register);
            }

            if (!string.IsNullOrEmpty(Value))
            {
                return string.Format("\tmov r0, {0}", Value); ;
            }

            throw new Exception("Cannot convert empty return");
        }
    }
}