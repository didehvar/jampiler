using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using Jampiler.Core;

namespace Jampiler.Code
{
    public class Function
    {
        public string Name { get; set; }

        private readonly List<string> _lines = new List<string>();
        private readonly List<Data> _data = new List<Data>();

        private int count = 0;

        private Function() { }

        public Function(string name)
        {
            Name = name;
        }

        public void AddLine(Instruction instruction, params string[] parts)
        {
            var line = string.Format("\t{0}\t", instruction.ToString().ToLower());

            for (var i = 0; i < parts.Length; i++)
            {
                if (i != 0)
                {
                    line += ", ";
                }

                line += string.Format("{0}", parts[i]);
            }

            _lines.Add(line + "\n");
        }

        public Data AddData(Data data)
        {
            if (data.Name == "")
            {
                data.Name = Name + count++;
            }

            var find = _data.Find(d => d.Name == data.Name);
            if (find != null)
            {
                return find;
            }

            _data.Add(data);
            return data;
        }

        public string Text()
        {
            return string.Format(
                "{0}\tbx\tlr\n\n{1}",
                _lines.Aggregate(string.Format(".global\t{0}\n{0}:\n", Name), (current, line) => current + line),
                _data.Aggregate("", (current, d) => current + string.Format("addr_{0} : .word {0}", d.Name)));
        }

        public string Data()
        {
            return _data.Aggregate(
                "", (current, data) => string.Format("{0}.balign\t4\n{1}: .{2} {3}\n", current, data.Name, data.Type, data.Value));
        }
    }
}