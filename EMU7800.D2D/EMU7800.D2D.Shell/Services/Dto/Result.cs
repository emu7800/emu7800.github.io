using System;
using System.Collections.Generic;
using System.Linq;

namespace EMU7800.Services.Dto
{
    public class Result
    {
        public bool IsOk { get; } = true;

        public bool IsFail => !IsOk;

        public bool IsRetryable { get; }

        public string ErrorMessage { get; } = string.Empty;

        public Result() { }

        public Result(string message, bool isRetryable)
        {
            IsOk = false;
            IsRetryable = isRetryable;
            ErrorMessage = message;
        }
    }

    public class Result<T> : Result where T : class, new()
    {
        public T Value { get; } = new T();

        public Result(string message, bool isRetryable) : base(message, isRetryable) { }

        public Result(T value) => Value = value;
    }

    public class Results<T> : Result where T : class, new()
    {
        public IEnumerable<T> Values { get; } = Array.Empty<T>();

        public Results(string message, bool isRetryable) : base(message, isRetryable) { }

        public Results(IEnumerable<T> values) => Values = values;
    }

    public static class ResultHelper
    {
        public static Result Ok()
            => new Result();
        public static Result<T> Ok<T>(T value) where T : class, new()
            => new Result<T>(value);

        public static Result Fail(string message, bool isRetryable = false)
            => new Result(message, isRetryable);
        public static Result<T> Fail<T>(string message, bool isRetryable = false) where T : class, new()
            => new Result<T>(message, isRetryable);

        public static Result Fail(Exception ex, bool isRetryable = false)
            => new Result(ToUnexpectedExceptionMessage(ex), isRetryable);
        public static Result<T> Fail<T>(Exception ex, bool isRetryable = false) where T : class, new()
            => new Result<T>(ToUnexpectedExceptionMessage(ex), isRetryable);

        public static Result Fail(AggregateException aex, bool isRetryable = false)
            => new Result(ToUnexpectedExceptionsMessage(aex), isRetryable);
        public static Result<T> Fail<T>(AggregateException aex, bool isRetryable = false) where T : class, new()
            => new Result<T>(ToUnexpectedExceptionsMessage(aex), isRetryable);

        public static string ToUnexpectedExceptionsMessage(AggregateException aex)
            => "Unexpected exception(s): " + ToExceptionMessages(aex);

        public static string ToUnexpectedExceptionMessage(Exception ex)
            => "Unexpected exception: " + ToExceptionMessage(ex);

        public static string ToExceptionMessages(AggregateException aex)
            => string.Join("; ", aex.Flatten()
                    .InnerExceptions
                    .Concat(new[] { aex.InnerException })
                        .Select(iex => iex != null ? ToExceptionMessage(iex) : string.Empty)
                        .Where(s => !string.IsNullOrWhiteSpace(s)));

        public static string ToExceptionMessage(Exception ex)
            => $"{ex.GetType().Name}: {ex.Message}";
    }

    public static class ResultsHelper
    {
        public static Results<T> Ok<T>(IEnumerable<T> values) where T : class, new()
            => new Results<T>(values);

        public static Results<T> Fail<T>(string message, bool isRetryable = false) where T : class, new()
            => new Results<T>(message, isRetryable);
        public static Results<T> Fail<T>(Exception ex, bool isRetryable = false) where T : class, new()
            => new Results<T>(ResultHelper.ToUnexpectedExceptionMessage(ex), isRetryable);

        public static Results<T> Fail<T>(AggregateException aex, bool isRetryable = false) where T : class, new()
            => new Results<T>(ResultHelper.ToUnexpectedExceptionsMessage(aex), isRetryable);
    }
}
