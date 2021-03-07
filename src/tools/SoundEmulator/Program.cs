using EMU7800.SoundEmulator;
using System;
using System.IO;
using static System.Console;

AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

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
            WriteLine("Buffers must be between 1 and 64.");
            return 1;
        }
    }
}

WriteLine(@"
EMU7800 SoundEmulator
Copyright (c) 2012 Mike Murphy
");

if (helpRequested)
{
    WriteLine(@"
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
    WriteLine("Input tape not specified. /h for help.");
    return 0;
}
if (!File.Exists(inputTapeFileName))
{
    WriteLine("Specified input tape filename not found.");
    return 0;
}

WriteLine($@"Sound buffer queue size: {buffers}
Using {(palRegionRequested ? "PAL" : "NTSC")} region 7800 machine configuration for playback.
Loading input tape: {inputTapeFileName}");

var inputTapeReader = new InputTapeReader();
var enqueuedCount = inputTapeReader.Load(inputTapeFileName);

WriteLine($@"Tape loaded, enqueued count: {enqueuedCount}
Starting playback; CTRL-C terminates.");

var player = new InputTapePlayer(inputTapeReader) { EndOfTapeReached = () => WriteLine("End of tape reached.") };

var soundEmulator = new SoundEmulator();

CancelKeyPress += (o, e) => Console_CancelKeyPress(soundEmulator);

soundEmulator.Buffers = buffers;
soundEmulator.GetRegisterSettingsForNextFrame = player.GetRegisterSettingsForNextFrame;

if (palRegionRequested)
{
    soundEmulator.StartPAL();
}
else
{
    soundEmulator.StartNTSC();
}

soundEmulator.WaitUntilStopped();

return 0;

static bool StartsWith(string arg, string text)
    => !string.IsNullOrWhiteSpace(arg) && arg.StartsWith(text, StringComparison.OrdinalIgnoreCase);

static int GetIntArg(string curArg, int defaultValue)
{
    var startPos = curArg.IndexOf(":", StringComparison.OrdinalIgnoreCase);
    return startPos >= 0 ? int.TryParse(curArg[(startPos + 1)..], out var num) ? num : defaultValue : defaultValue;
}

static string GetStrArg(string curArg, string defaultValue)
{
    var startPos = curArg.IndexOf(":", StringComparison.OrdinalIgnoreCase);
    return startPos >= 0 ? curArg[(startPos + 1)..] : defaultValue;
}

static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    WriteLine(e.ExceptionObject.ToString());
    Environment.Exit(1);
}

static void Console_CancelKeyPress(object? sender)
{
    WriteLine("*** Ctrl-C Acknowledgement: Requesting Termination ***");
    if (sender is SoundEmulator soundEmulator)
    {
        soundEmulator.Stop();
    }
}
