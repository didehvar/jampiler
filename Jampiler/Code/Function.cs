﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
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
        private readonly List<Data> _registers;

        private readonly List<string> _lines = new List<string>();

        private int _count = 0;
        private int _regDeleteStart = 0;
        private int _regMax = 0;

        private Function() { }

        public Function(Node startNode, string name)
        {
            StartNode = startNode;
            Name = name;
            _registers = new List<Data>();
            Statements = new List<Statement>();
        }

        public void AddReturn(Return ret)
        {
            AssembleData(ret.Data);
        }

        public void AddStatement(Statement statement)
        {
            // Assign the statement a register so that it can load its data
            // In order to load a statement, check if it needs setup text in assembly (e.g. for assignment)
            // Then add it to this functions statement list for later use

            if (string.IsNullOrEmpty(statement.Value))
            {
                return;
            }

            statement.Register = AddRegister(statement);
            _lines.Add(statement.LoadText());

            Statements.Add(statement);
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

            s += string.Format("{0}\n", _lines.Aggregate("", (current, line) => current + line));
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
            _regDeleteStart = _registers.Count;
        }

        public void EndRegisterStore()
        {
            UpdateRegCount();
            _registers.RemoveRange(_regDeleteStart, _registers.Count - _regDeleteStart);
        }

        public int RegisterCount()
        {
            return _registers.Count;
        }

        public int AddRegister(Data register)
        {
            Console.WriteLine("ADDREGISTER: {0} [[{1}]]", _registers.Count, register.ToString());
            _registers.Add(register);
            return _registers.Count - 1;
        }

        public void DeleteRegister(int index)
        {
            UpdateRegCount();
            _registers.RemoveAt(index);
        }

        private void UpdateRegCount()
        {
            var regCount = _registers.Count - 1;
            if (regCount >= 4 && _regMax < regCount)
            {
                _regMax = regCount;
            }
        }

        public void AssembleData(List<Data> data, bool storeRegisters = true)
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
                Console.WriteLine(d.ToString());
            }
            Console.WriteLine();

            var first = data.ElementAt(0);
            _lines.Add(ParseData(first));

            if (data.Count > 1)
            {
                var currentRegister = startRegister;
                for (var i = 1; i < data.Count - 1; i = i + 2) // Exclude first and last element
                {
                    var right = data.ElementAt(i + 1);
                    var op = data.ElementAt(i);

                    // Left element is in register [start register]
                    // Right element is in register [start register + 1]
                    _lines.Add(ParseData(right));

                    AddLine(op.Value, startRegister, (++currentRegister).ToString());
                }
            }

            // If register 0 isn't used as the final data store for the operation, the data must be moved into r0
            if (startRegister != 0 && storeRegisters)
            {
                _lines.Add(string.Format("\tmov r0, r{0}\n", startRegister));
            }

            if (storeRegisters)
            {
                EndRegisterStore();
            }
        }

        private string TryParseStatement(Data data)
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

            return statement.Register.ToString();
        }

        private string ParseData(Data data, bool isReturn = false)
        {
            var statement = TryParseStatement(data);
            if (statement != null)
            {
                statement = "r" + statement;
            }

            switch (data.Type)
            {
                case DataType.Asciz:
                    return string.Format(
                        "\tldr r{0}, addr_{1}\n" + "\tldr r{0}, [r{0}]\n", isReturn ? 0 : AddRegister(data), statement ?? data.Name);
                case DataType.Number:
                    return string.Format("\tmov r{0}, {1}\n", isReturn ? 0 : AddRegister(data), statement ?? "#" + data.Value);

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

        private void AddLine(string oper, int firstRegister, string lastRegister)
        {
            switch (oper)
            {
                case "+":
                    _lines.Add(string.Format("\tadd r{0}, r{0}, r{1}\n", firstRegister, lastRegister));
                    break;
            }
        }
    }
}