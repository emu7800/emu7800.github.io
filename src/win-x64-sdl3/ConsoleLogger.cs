// © Mike Murphy

using EMU7800.Core;
using System;
using System.Runtime.InteropServices;

namespace EMU7800.Win32.Interop;

public sealed partial class ConsoleLogger : ILogger
{
    public int Level { get; set; }

    public void Log(int level, string message)
    {
        if (level <= Level)
        {
            Console.WriteLine(message);
        }
    }

    public ConsoleLogger(bool openConsoleWindow)
    {
        if (openConsoleWindow)
        {
            AllocConsole();
        }
        else
        {
            AttachConsole(-1);
        }
    }

    [LibraryImport("Kernel32.dll")]
    internal static partial void AllocConsole();

    [LibraryImport("Kernel32.dll")]
    internal static partial void AttachConsole(int dwProcessId);
}
