using System;
using System.Runtime.Serialization;

namespace EMU7800.Core.Extensions;

public static class ArgumentExceptionExtensions
{
    extension(ArgumentException ex)
    {
        public static void ThrowIf(bool cond, string? message)
        {
            if (cond) throw new ArgumentException(message);
        }
        public static void ThrowIf(bool cond, string? message, string? paramName = null)
        {
            if (cond) throw new ArgumentException(message, paramName);
        }
    }
}

public static class SerializationExceptionExtensions
{
    extension(SerializationException ex)
    {
        public static void ThrowIf(bool cond, string? message)
        {
            if (cond) throw new SerializationException(message);
        }
    }
}
