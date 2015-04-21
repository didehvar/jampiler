using System.Net.Mime;
using System.Runtime.InteropServices;

namespace Jampiler.Code
{
    public class Data
    {
        public string Type;
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
    }
}