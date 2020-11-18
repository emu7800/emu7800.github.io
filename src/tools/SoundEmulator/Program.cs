using System;
using System.IO;

namespace EMU7800.SoundEmulator
{
    static class Program
    {
        static readonly SoundEmulator SoundEmulator = new();

        [STAThread]
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            Console.CancelKeyPress += Console_CancelKeyPress;

            var helpRequested = false;
            var inputTapeFileName = string.Empty;
            var palRegionRequested = false;
            var buffers = 8;

            foreach (var arg in args)
            {
                if (StartsWith(arg, "/h"))
                {
                    helpRequested = true;
                }
                else if (StartsWith(arg, "/f"))
                {
                    inputTapeFileName = GetStrArg(arg, string.Empty);
                }
                else if (StartsWith(arg, "/r"))
                {
                    var regionVal = GetStrArg(arg, "ntsc");
                    palRegionRequested = regionVal.StartsWith("p", StringComparison.OrdinalIgnoreCase);
                }
                else if (StartsWith(arg, "/b"))
                {
                    buffers = GetIntArg(arg, 8);
                    if (buffers < 1 || buffers > 64)
                    {
                        Console.WriteLine("Buffers must be between 1 and 64.");
                        return 1;
                    }
                }
            }

            Console.WriteLine(@"
EMU7800 SoundEmulator
Copyright (c) 2012 Mike Murphy
");

            if (helpRequested)
            {
                Console.WriteLine(@"
Usage:
    /h                    Show usage information
    /f:<filename>         Input tape (required)
    /r:{region}           Region select: NTSC or PAL (default:NTSC)
    /b:{#}                Number of buffers in sound queue (default:8)
");
                return 0;
            }

            if (string.IsNullOrWhiteSpace(inputTapeFileName))
            {
                Console.WriteLine("Input tape not specified. /h for help.");
                return 0;
            }
            if (!File.Exists(inputTapeFileName))
            {
                Console.WriteLine("Specified input tape filename not found.");
                return 0;
            }

            Console.WriteLine("Sound buffer queue size: {0}", buffers);
            Console.WriteLine("Using {0} region 7800 machine configuration for playback.", palRegionRequested ? "PAL" : "NTSC");

            Console.WriteLine("Loading input tape: {0}", inputTapeFileName);

            var inputTapeReader = new InputTapeReader();
            var enqueuedCount = inputTapeReader.Load(inputTapeFileName);

            Console.WriteLine("Tape loaded, enqueued count: {0}", enqueuedCount);

            Console.WriteLine("Starting playback; CTRL-C terminates.");

            var player = new InputTapePlayer(inputTapeReader) { EndOfTapeReached = () => Console.WriteLine("End of tape reached.") };

            SoundEmulator.Buffers = buffers;
            SoundEmulator.GetRegisterSettingsForNextFrame = player.GetRegisterSettingsForNextFrame;

            if (palRegionRequested)
                SoundEmulator.StartPAL();
            else
                SoundEmulator.StartNTSC();

            SoundEmulator.WaitUntilStopped();

            return 0;
        }

        static bool StartsWith(string arg, string text)
        {
            return !string.IsNullOrWhiteSpace(arg) && arg.StartsWith(text, StringComparison.OrdinalIgnoreCase);
        }

        static int GetIntArg(string curArg, int defaultValue)
        {
            var startPos = curArg.IndexOf(":", StringComparison.OrdinalIgnoreCase);
            if (startPos < 0)
                return defaultValue;
            return int.TryParse(curArg[(startPos + 1)..], out var num) ? num : defaultValue;
        }

        static string GetStrArg(string curArg, string defaultValue)
        {
            var startPos = curArg.IndexOf(":", StringComparison.OrdinalIgnoreCase);
            if (startPos < 0)
                return defaultValue;
            return curArg[(startPos + 1)..];
        }

        static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Environment.Exit(1);
        }

        static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("*** Ctrl-C Acknowledgement: Requesting Termination ***");
            SoundEmulator.Stop();
        }
    }
}
