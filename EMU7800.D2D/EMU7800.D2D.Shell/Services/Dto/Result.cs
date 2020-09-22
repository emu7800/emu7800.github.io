namespace EMU7800.Services.Dto
{
    public class Result
    {
        public bool IsOk { get; } = true;

        public bool IsFail => !IsOk;

        public bool IsRetryable { get; }

        public string ErrorMessage { get; } = string.Empty;

        public Result() { }

        public Result(string message, bool isRetryable = false)
        {
            IsOk = false;
            IsRetryable = isRetryable;
            ErrorMessage = message;
        }
    }
}
