using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jampiler.Core
{
    public abstract class Symbol
    {
        public List<Symbol> RequiredSymbols { get; set; }

        protected Symbol(params object[] symbols)
        {
            var requiredSymbols = new List<Symbol>();

            foreach (var symbol in symbols)
            {
                var item = symbol as Symbol;
                if (item != null)
                {
                    requiredSymbols.Add(item);
                }
                else
                {
                    var enumerable = symbol as IEnumerable<Symbol>;
                    if (enumerable != null)
                    {
                        requiredSymbols.AddRange(enumerable);
                    }
                    else
                    {
                        throw new Exception("Couldn't parse symbols (not symbol/list of symbols)");
                    }
                }
            }

            RequiredSymbols = requiredSymbols;
        }

        public override string ToString()
        {
            return string.Concat(RequiredSymbols.Select(sym => sym.ToString()));
        }
    }
}
