using System;
using System.Collections.Generic;
using System.Linq;
using Jampiler.AST;

namespace Jampiler.Code
{
    public class Function
    {
        public Node StartNode { get; set; }

        public string Name { get; set; }

        public readonly List<Statement> Statements;

        /// <summary>
        /// Element position represents the register in use.
        /// </summary>
        public readonly List<Data> Registers;

        public readonly List<string> Lines = new List<string>();

        private int _count;
        private int _regDeleteStart;
        private int _regMax;

        public Function(Node startNode, string name)
        {
            StartNode = startNode;
            Name = name;
            Registers = new List<Data>();
            Statements = new List<Statement>();
        }

        public void AddReturn(Return ret)
        {
            AssembleData(ret.Data);
        }

        public void AddData(Data data)
        {
            // Assign the statement a register so that it can load its data
            // In order to load a statement, check if it needs setup text in assembly (e.g. for assignment)
            // Then add it to this functions statement list for later use

            if (string.IsNullOrEmpty(data.Value))
            {
                return;
            }

            if (data is Statement)
            {
                var s = (Statement) data;
                s.Register = AddRegister(data);
                Lines.Add(s.LoadText());

                Statements.Add(s);
            }
        }

        public string Text()
        {
            var s = string.Format(".global\t{0}\n{0}:\n", Name);
            var pop = "";

            // Store result of r4-r12 if used
            if (_regMax > 3)
            {
                s += "\tpush {r4";
                pop += "\n\tpop {r4";

                for (var i = 5; i < _regMax; i++)
                {
                    var format = string.Format(", r{0}", i);

                    s += format;
                    pop += format;
                }

                s += "}\n\n";
                pop += "}\n";
            }

            s += string.Format("{0}\n", Lines.Aggregate("", (current, line) => current + line));
            s += pop;
            s += "\tbx lr\n";

            return s;
        }

        public string DataName()
        {
            return string.Format("{0}{1}", Name, _count++.ToString());
        }

        public void StartRegisterStore()
        {
            _regDeleteStart = Registers.Count;
        }

        public void EndRegisterStore()
        {
            UpdateRegCount();
            Registers.RemoveRange(_regDeleteStart, Registers.Count - _regDeleteStart);
        }

        public int RegisterCount()
        {
            return Registers.Count;
        }

        public int AddRegister(Data register)
        {
            Console.WriteLine("ADDREGISTER: {0} [[{1}]]", Registers.Count, register.ToString());
            Registers.Add(register);
            return Registers.Count - 1;
        }

        public void DeleteRegister(int index)
        {
            UpdateRegCount();
            Registers.RemoveAt(index);
        }

        private void UpdateRegCount()
        {
            var regCount = Registers.Count - 1;
            if (regCount >= 4 && _regMax < regCount)
            {
                _regMax = regCount;
            }
        }

        public void AssembleData(List<Data> data, bool storeRegisters = true, bool moveToFirst = true)
        {
            if (data == null)
            {
                throw new NotImplementedException("TODO: exit functions");
            }

            if (storeRegisters)
            {
                StartRegisterStore();
            }

            var startRegister = RegisterCount();

            Console.WriteLine();
            Console.WriteLine("DATA FOR ASSEMBLEDATA");
            foreach (var d in data)
            {
                Console.WriteLine(d?.ToString());
            }
            Console.WriteLine();

            var first = data.ElementAt(0);
            var firstData = ParseData(first);
            Lines.Add(firstData);

            if (data.Count > 1)
            {
                var currentRegister = startRegister;
                for (var i = 1; i < data.Count - 1; i = i + 2) // Exclude first and last element
                {
                    var right = data.ElementAt(i + 1);
                    var op = data.ElementAt(i);

                    // Left element is in register [start register]
                    // Right element is in register [start register + 1]
                    Lines.Add(ParseData(right));

                    switch (op.Value)
                    {
                        case "+":
                            Lines.Add(string.Format("\tadd r{0}, r{0}, r{1}\n", startRegister, ++currentRegister));
                            break;

                        case "*":
                            Lines.Add(string.Format("\tmul r{0}, r{0}, r{1}\n", startRegister, ++currentRegister));
                            break;
                    }
                }
            }

            // If register 0 isn't used as the final data store for the operation, the data must be moved into r0
            if (startRegister != 0 && storeRegisters && firstData != null && moveToFirst)
            {
                Lines.Add(string.Format("\tmov r0, r{0}\n", startRegister));
            }

            if (storeRegisters)
            {
                EndRegisterStore();
            }
        }

        private Statement TryParseStatement(Data data)
        {
            if (!(data is Statement))
            {
                return null;
            }

            var statement = (Statement)data;
            if (statement.Register == null)
            {
                throw new Exception("Can't add to null");
            }

            return statement;
        }

        public string ParseData(Data data, bool isReturn = false)
        {
            if (data == null)
            {
                return null;
            }

            var secondReg = data;
            var statement = TryParseStatement(data);
            if (statement != null)
            {
                secondReg = statement;
            }

            switch (data.Type)
            {
                case DataType.Asciz:
                    return string.Format(
                        "\tldr r{0}, addr_{1}\n", isReturn ? 0 : AddRegister(data),
                        secondReg.Name);
                case DataType.Number:
                    if (Convert.ToInt32(secondReg.Value) >= 0 && Convert.ToInt32(secondReg.Value) <= 255)
                    {
                        return string.Format(
                            "\tmov r{0}, #{1}\n", isReturn ? 0 : AddRegister(data), secondReg.Value);
                    }

                    return string.Format("\tldr r{0}, ={1}\n", isReturn ? 0 : AddRegister(data), secondReg.Value);

                case DataType.Global:
                    // Locate the global value then assign it to the correct register
                    if (!(data is Global))
                    {
                        throw new Exception("Data should be an instance of Global");
                    }

                    AssembleData(((Global) data).Datas, false);
                    return "";

                default:
                    throw new NotImplementedException(string.Format("Data ({0}) not supported", data.Type));
            }
        }
    }
}