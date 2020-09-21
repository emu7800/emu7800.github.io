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

        public ErrorInfo(string message)
        {
            Message = message;
        }

        public ErrorInfo(Exception exception)
        {
            OriginatingException = exception;
            Message = "An exception occurred.";
        }

        public ErrorInfo(Exception exception, string message) : this(message)
        {
            OriginatingException = exception;
        }

        public ErrorInfo(ErrorInfo innerErrorInfo, string message) : this(message)
        {
            InnerErrorInfo = innerErrorInfo;
        }

        public ErrorInfo(ErrorInfo innerErrorInfo, Exception exception, string message) : this(message)
        {
            InnerErrorInfo = innerErrorInfo;
            OriginatingException = exception;
        }
    }
}