using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jampiler
{
    class Compiler
    {
        StringWriter output = new StringWriter();

        public Compiler()
        {
            Compile(42);

            Console.WriteLine(output.ToString());
            Console.ReadLine();
        }

        private void Compile(int x)
        {
            output.WriteLine(String.Format("mov\tr3, #{0}", x));
            output.WriteLine("mov\tr0, r3");
            output.WriteLine("bx\tlr");
        }
    }
}
