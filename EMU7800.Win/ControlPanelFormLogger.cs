using System;
using System.Text;
using EMU7800.Core;

namespace EMU7800.Win
{
    public class ControlPanelFormLogger : ILogger
    {
        readonly StringBuilder _messageSpool = new StringBuilder();

        public string GetMessages()
        {
            var messages = _messageSpool.ToString().Replace("\n", Environment.NewLine);
            _messageSpool.Length = 0;
            return messages;
        }

        #region ILogger Members

        public void Write(object value)
        {
            _messageSpool.Append(value);
        }

        public void Write(string format, params object[] args)
        {
            _messageSpool.AppendFormat(format, args);
        }

        public void WriteLine(object value)
        {
           Write(value);
           _messageSpool.AppendLine();
        }

        public void WriteLine(string format, params object[] args)
        {
            Write(format, args);
            _messageSpool.AppendLine();
        }

        #endregion
    }
}
