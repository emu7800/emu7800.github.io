// © Mike Murphy

using EMU7800.Core;
using System;
using System.Runtime.InteropServices;

namespace EMU7800.Win32.Interop;

public sealed partial class ConsoleLogger : ILogger
{
    bool _hasAllocated;

    public int Level { get; set; }

    public void Log(int level, string message)
    {
        if (!_hasAllocated)
        {
            AttachConsole(-1);
            _hasAllocated = true;
        }

        if (level <= Level)
        {
            Console.WriteLine(message);
        }
    }

    [LibraryImport("Kernel32.dll")]
    internal static partial void AttachConsole(int dwProcessId);
}
