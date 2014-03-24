// © Mike Murphy

using System;

namespace EMU7800.Services.Dto
{
    public class ErrorInfo
    {
        public string Message { get; private set; }
        public Exception OriginatingException { get; private set; }
        public ErrorInfo InnerErrorInfo { get; private set; }

        public ErrorInfo()
        {
        }

        public ErrorInfo(string format, params object[] args)
        {
            Message = (args != null) ? string.Format(format, args) : format;
        }

        public ErrorInfo(Exception exception)
        {
            OriginatingException = exception;
            Message = "An exception occurred.";
        }

        public ErrorInfo(Exception exception, string format, params object[] args) : this(format, args)
        {
            OriginatingException = exception;
        }

        public ErrorInfo(ErrorInfo innerErrorInfo, string format, params object[] args) : this(format, args)
        {
            InnerErrorInfo = innerErrorInfo;
        }

        public ErrorInfo(ErrorInfo innerErrorInfo, Exception exception, string format, params object[] args) : this(format, args)
        {
            InnerErrorInfo = innerErrorInfo;
            OriginatingException = exception;
        }
    }
}