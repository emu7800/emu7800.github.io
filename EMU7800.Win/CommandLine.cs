/*
 * CommandLine.cs
 *
 * Supporting class for executable command lines.
 *
 * Copyright © 2003 Mike Murphy
 *
 */
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace EMU7800.Win
{
    #region CommandLine Parameter Class

    public class CommandLineParameter
    {
        private readonly NumberStyles _NumberStyle;

        public string StrValue { get; private set; }

        public int IntValue
        {
            get { return IsInteger ? Int32.Parse(StrValue, _NumberStyle, CultureInfo.InvariantCulture) : 0; }
        }

        public bool IsInteger
        {
            get
            {
                int outVal;
                return Int32.TryParse(StrValue, _NumberStyle, CultureInfo.InvariantCulture, out outVal);
            }
        }

        public bool IsHelp { get { return StrValue.Equals("?"); } }

        public bool Equals(string val)
        {
            return StrValue.Equals(val, StringComparison.OrdinalIgnoreCase);
        }
        public bool Equals(int i)
        {
            return IntValue.Equals(i);
        }

        private CommandLineParameter()
        {
        }

        public CommandLineParameter(string value) : this()
        {
            if (value.Substring(0, 1) == "$")
            {
                StrValue = value.Substring(1);
                _NumberStyle = NumberStyles.HexNumber;
            }
            else if (value.Length >= 2 && value.Substring(0, 2) == "0x")
            {
                StrValue = value.Substring(2);
                _NumberStyle = NumberStyles.HexNumber;
            }
            else
            {
                StrValue = value;
                _NumberStyle = NumberStyles.Number;
            }
        }
    }

    #endregion

    public class CommandLine
    {
        private readonly CommandLineParameter[] _Parms;
        private ReadOnlyCollection<CommandLineParameter> _ParmsCollection;

        public ReadOnlyCollection<CommandLineParameter> Parms
        {
            get { return _ParmsCollection ?? (_ParmsCollection = new ReadOnlyCollection<CommandLineParameter>(_Parms)); }
        }

        public bool HasMatched { get; private set; }
        public bool IsHelp { get; private set; }

        public bool CommandEquals(string input)
        {
            if (IsHelp)
                return true;
            if (HasMatched)
                return false;
            if (!Parms.Count.Equals(0) && Parms[0].Equals(input))
            {
                HasMatched = true;
                return true;
            }
            return false;
        }

        public bool IsCommandHelp
        {
            get { return IsHelp || _Parms.Length > 1 && _Parms[1].Equals("?"); }
        }

        public bool CheckParms(string chkstr)
        {
            if (chkstr == null)
                throw new ArgumentNullException("chkstr");

            if (chkstr.Length != _Parms.Length)
                return false;
            for (var i = 1; i < chkstr.Length; i++)
            {
                if (chkstr.Substring(i, 1) == "i" && !Parms[i].IsInteger)
                    return false;
            }
            return true;
        }

        public CommandLine(string commandLine)
        {
            if (commandLine == null)
                throw new ArgumentNullException("commandLine");

            _Parms = commandLine.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries).Select(tok => new CommandLineParameter(tok)).ToArray();
            IsHelp = !Parms.Count.Equals(0) && Parms[0].Equals("?");
        }
    }
}
