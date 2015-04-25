using System;
using System.Collections.Generic;
using System.Linq;
using Jampiler.AST;
using Jampiler.Core;

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

                // Check that this data doesn't already have a register
                var existingReg = Registers.FirstOrDefault(r => r.Name == s.Name);
                if (existingReg != null)
                {
                    // If it does don't use the same register
                    s.Register = ((Statement) existingReg).Register;
                }
                else
                {
                    s.Register = AddRegister(data);
                }

                if (data.Type != DataType.Function)
                {
                    Lines.Add(s.LoadText());
                }

                Statements.Add(s);
            }
        }

        public string Text()
        {
            var s = "";

            if (Name == "main")
            {
                s += ".global main\n";
            }

            s += string.Format("{0}:\n", Name);

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
            s += string.Format("end{0}:\n", Name);
            s += pop;
            s += "\tbx lr\n";

            return s;
        }

        public void StoreLR(Data data, int register)
        {
            // Store lr before puts replaces it
            Lines.Add(string.Format("\n\tldr r{0}, addr_{1}\n", register, data.Name));
            Lines.Add(string.Format("\tstr lr, [r{0}]\n\n", register));
        }

        public void LoadLR(Data data, int register)
        {
            // Restore lr
            Lines.Add(string.Format("\n\tldr lr, addr_{1}\n", register, data.Name));
            Lines.Add(string.Format("\tldr lr, [lr]\n", register));
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
            var regCount = Registers.Count;
            if (regCount >= 4 && _regMax < regCount)
            {
                _regMax = regCount;
            }
        }

        public void AssembleData(List<Data> data, bool storeRegisters = true, bool moveToFirst = true, int? moveToSpecific = null)
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
            Lines.Add("\n\t/* start assembledata() */\n");
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

            if (moveToSpecific != null)
            {
                Lines.Add(string.Format("\tmov r{0}, r{1}\n", moveToSpecific, startRegister));
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

            Lines.Add("\t/* end assembledata() */\n\n");
        }

        public void AssembleDataIf(CodeGenerator codeGenerator,
            Node ifnode,
            List<Data> data,
            bool storeRegisters = true,
            bool moveToFirst = true,
            bool isWhile = false)
        {
            // Left is block
            // Left left is comparison

            if (data == null)
            {
                throw new NotImplementedException("Missing if data");
            }

            if (storeRegisters)
            {
                StartRegisterStore();
            }

            var startRegister = RegisterCount();

            Console.WriteLine();
            Console.WriteLine("DATA FOR ASSEMBLEDATAIF");
            foreach (var d in data)
            {
                Console.WriteLine(d?.ToString());
            }
            Console.WriteLine();

            var ifLabelNumber = Lines.Count;

            var first = data.ElementAt(0);
            var firstData = ParseData(first);

            var loopName = isWhile ? "while" : "if";
            Lines.Add(string.Format("\n\t/* start assembledata{0}() */\n", loopName));

            // For a while loop, label the start so control can return
            if (isWhile)
            {
                Lines.Add(string.Format("start{0}{1}:\n", loopName, ifLabelNumber));
            }

            // If is before
            if (!isWhile)
            {
                Lines.Add(firstData);

                if (data.Count > 1)
                {
                    // Increment start register for if, cannot use left hand side of comparison
                    var ifRegister = startRegister + 1;
                    var currentRegister = ifRegister;

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
                                Lines.Add(string.Format("\tadd r{0}, r{0}, r{1}\n", ifRegister, ++currentRegister));
                                break;

                            case "*":
                                Lines.Add(string.Format("\tmul r{0}, r{0}, r{1}\n", ifRegister, ++currentRegister));
                                break;

                            case "-":
                                Lines.Add(string.Format("\tsub r{0}, r{0}, r{1}\n", ifRegister, ++currentRegister));
                                break;
                        }
                    }
                }

                // Calculations have been worked out, check compares
                if (data.Count > 1 && !isWhile)
                {
                    var currentRegister = startRegister;
                    for (var i = 1; i < data.Count - 1; i = i + 2) // Exclude first and last element
                    {
                        var op = data.ElementAt(i);

                        switch (op.Value)
                        {
                            case "<":
                                Lines.Add(string.Format("\tcmp r{0}, r{1}\n", startRegister, ++currentRegister));
                                Lines.Add(string.Format("\tbge end{0}{1}\n", loopName, ifLabelNumber));
                                break;

                            case ">":
                                Lines.Add(string.Format("\tcmp r{0}, r{1}\n", startRegister, ++currentRegister));
                                Lines.Add(string.Format("\tble end{0}{1}\n", loopName, ifLabelNumber));
                                break;

                            case ">=":
                                Lines.Add(string.Format("\tcmp r{0}, r{1}\n", startRegister, ++currentRegister));
                                Lines.Add(string.Format("\tblt end{0}{1}\n", loopName, ifLabelNumber));
                                break;

                            case "<=":
                                Lines.Add(string.Format("\tcmp r{0}, r{1}\n", startRegister, ++currentRegister));
                                Lines.Add(string.Format("\tbgt end{0}{1}\n", loopName, ifLabelNumber));
                                break;

                            case "==":
                                Lines.Add(string.Format("\tcmp r{0}, r{1}\n", startRegister, ++currentRegister));
                                Lines.Add(string.Format("\tbne end{0}{1}\n", loopName, ifLabelNumber));
                                break;

                            case "!=":
                                Lines.Add(string.Format("\tcmp r{0}, r{1}\n", startRegister, ++currentRegister));
                                Lines.Add(string.Format("\tbeq end{0}{1}\n", loopName, ifLabelNumber));
                                break;
                        }
                    }
                }
            }

            // If register 0 isn't used as the final data store for the operation, the data must be moved into r0
            if (startRegister != 0 && storeRegisters && firstData != null && moveToFirst)
            {
                Lines.Add(string.Format("\tmov r0, r{0}\n", startRegister));
            }

            // Navigate through the control structure and parse the statements within
            var ifStatement = ifnode.Left.Right;
            do
            {
                // If there is a return in the control structure we may need to jump to the end of this function
                if (ifStatement.Type == TokenType.Return)
                {
                    AddReturn(codeGenerator.ParseReturn(this, ifStatement));
                    Lines.Add(string.Format("\tb end{0}\n", Name));
                }
                else
                {
                    AddData(codeGenerator.ParseStatement(this, ifStatement));
                }

                ifStatement = ifStatement.Right;
            } while (ifStatement != null);

            // While is after
            // Calculations have been worked out, check compares
            if (isWhile)
            {
                Lines.Add(firstData);

                if (data.Count > 1)
                {
                    // Increment start register for if, cannot use left hand side of comparison
                    var ifRegister = startRegister + 1;
                    var currentRegister = ifRegister;

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
                                Lines.Add(string.Format("\tadd r{0}, r{0}, r{1}\n", ifRegister, ++currentRegister));
                                break;

                            case "*":
                                Lines.Add(string.Format("\tmul r{0}, r{0}, r{1}\n", ifRegister, ++currentRegister));
                                break;

                            case "-":
                                Lines.Add(string.Format("\tsub r{0}, r{0}, r{1}\n", ifRegister, ++currentRegister));
                                break;
                        }
                    }
                }

                if (data.Count > 1)
                {
                    var currentRegister = startRegister;
                    for (var i = 1; i < data.Count - 1; i = i + 2) // Exclude first and last element
                    {
                        var op = data.ElementAt(i);

                        switch (op.Value)
                        {
                            case "<":
                                Lines.Add(string.Format("\tcmp r{0}, r{1}\n", startRegister, ++currentRegister));
                                Lines.Add(string.Format("\tbge end{0}{1}\n", loopName, ifLabelNumber));
                                break;

                            case ">":
                                Lines.Add(string.Format("\tcmp r{0}, r{1}\n", startRegister, ++currentRegister));
                                Lines.Add(string.Format("\tble end{0}{1}\n", loopName, ifLabelNumber));
                                break;

                            case ">=":
                                Lines.Add(string.Format("\tcmp r{0}, r{1}\n", startRegister, ++currentRegister));
                                Lines.Add(string.Format("\tblt end{0}{1}\n", loopName, ifLabelNumber));
                                break;

                            case "<=":
                                Lines.Add(string.Format("\tcmp r{0}, r{1}\n", startRegister, ++currentRegister));
                                Lines.Add(string.Format("\tbgt end{0}{1}\n", loopName, ifLabelNumber));
                                break;

                            case "==":
                                Lines.Add(string.Format("\tcmp r{0}, r{1}\n", startRegister, ++currentRegister));
                                Lines.Add(string.Format("\tbne end{0}{1}\n", loopName, ifLabelNumber));
                                break;

                            case "!=":
                                Lines.Add(string.Format("\tcmp r{0}, r{1}\n", startRegister, ++currentRegister));
                                Lines.Add(string.Format("\tbeq end{0}{1}\n", loopName, ifLabelNumber));
                                break;
                        }
                    }
                }

                // For a while loop, continue from the start of this loop
                Lines.Add(string.Format("\tb start{0}{1}\n", loopName, ifLabelNumber));
            }

            if (storeRegisters)
            {
                EndRegisterStore();
            }

            // After loop label
            Lines.Add(string.Format("\nend{0}{1}:\n", loopName, ifLabelNumber));
            Lines.Add(string.Format("\t/* end assembledata{0}() */\n", loopName));
        }

        private Statement TryParseStatement(Data data)
        {
            if (!(data is Statement))
            {
                return null;
            }

            var statement = (Statement) data;
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

            var register = AddRegister(data);

            // Check for existing register
            var secondReg = data;
            var statement = TryParseStatement(data);
            if (statement != null)
            {
                secondReg = statement;

                Console.WriteLine("IS STATEMENT: {0}", statement);
                if (statement.Register != null)
                {
                    // Statement already exists in register, simply load it into the required register
                    return string.Format("\tmov r{0}, r{1}\n", register, statement.Register);
                }
            }

            switch (data.Type)
            {
                case DataType.Asciz:
                    return string.Format(
                        "\tldr r{0}, addr_{1}\n", isReturn ? 0 : register,
                        secondReg.Name);

                case DataType.Number:
                    if (Convert.ToInt32(secondReg.Value) >= 0 && Convert.ToInt32(secondReg.Value) <= 255)
                    {
                        return string.Format(
                            "\tmov r{0}, #{1}\n", isReturn ? 0 : register, secondReg.Value);
                    }

                    return string.Format("\tldr r{0}, ={1}\n", isReturn ? 0 : register, secondReg.Value);

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