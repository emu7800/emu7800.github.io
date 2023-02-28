using EMU7800.Core;
using EMU7800.D2D.Shell.Win32;
using EMU7800.Services;
using EMU7800.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

using static System.Console;

if (args.Length == 0 || new[] { "-c", "/c" }.Any(OptEq))
{
    if (args.Length > 0)
    {
        AllocConsole();
    }
    Start(args.Length == 0);
    EnvironmentExit(0, args.Length > 0);
}

var romPath        = args.Length > 1 ? args[1] : string.Empty;
var machineTypeStr = args.Length > 2 ? args[2] : string.Empty;
var cartTypeStr    = args.Length > 3 ? args[3] : string.Empty;
var lControllerStr = args.Length > 4 ? args[4] : string.Empty;
var rControllerStr = args.Length > 5 ? args[5] : string.Empty;

if (new[] { "-r", "/r" }.Any(OptEq))
{
    if (machineTypeStr.Length > 0)
    {
        var machineType = MachineTypeUtil.From(machineTypeStr);
        var cartType = CartTypeUtil.From(cartTypeStr);
        var lController = ControllerUtil.From(lControllerStr);
        var rController = ControllerUtil.From(rControllerStr);
        if (machineType is MachineType.A2600NTSC or MachineType.A2600PAL)
        {
            if (cartType is CartType.Unknown)
            {
                var bytes = DatastoreService.GetRomBytes(romPath);
                var len = bytes.Length;
                cartType = RomBytesService.InferCartTypeFromSize(machineType, len);
            }
            else if (cartTypeStr.StartsWith("A78"))
            {
                AllocConsole();
                WriteLine($"Bad CartType '{cartType}' for MachineType '{machineType}'");
                EnvironmentExit(-8);
            }
            lController = lController is Controller.None ? Controller.Joystick : lController;
            rController = rController is Controller.None ? Controller.Joystick : rController;
        }
        else if (machineType is MachineType.A7800NTSC or MachineType.A7800PAL)
        {
            if (cartType is CartType.Unknown)
            {
                var bytes = DatastoreService.GetRomBytes(romPath);
                var len = RomBytesService.RemoveA78HeaderIfNecessary(bytes).Length;
                cartType = RomBytesService.InferCartTypeFromSize(machineType, len);
            }
            else if (!cartTypeStr.StartsWith("A78"))
            {
                AllocConsole();
                WriteLine($"Bad CartType '{cartType}' for MachineType '{machineType}'");
                EnvironmentExit(-8);
            }
            lController = lController is Controller.None ? Controller.ProLineJoystick : lController;
            rController = rController is Controller.None ? Controller.ProLineJoystick : rController;
        }
        else
        {
            AllocConsole();
            WriteLine("Unknown MachineType: " + machineType);
            EnvironmentExit(-8);
        }

        StartGameProgram(new(machineType, cartType, lController, rController, romPath));
    }
    else
    {
        var gpiviList = GameProgramLibraryService.GetGameProgramInfoViewItems(romPath);
        if (gpiviList.Any())
        {
            StartGameProgram(gpiviList.First());
        }
        else
        {
            var bytes = DatastoreService.GetRomBytes(romPath);
            if (RomBytesService.IsA78Format(bytes))
            {
                var gpi = RomBytesService.ToGameProgramInfoFromA78Format(bytes);
                StartGameProgram(new(gpi, string.Empty, romPath));
            }
            else
            {
                AllocConsole();
                WriteLine("No information in ROMProperties.csv database for: " + romPath);
                EnvironmentExit(-8);
            }
        }
    }
}
else if (romPath.Length > 0 && new[] { "-d", "/d" }.Any(OptEq))
{
    AllocConsole();
    RomBytesService.DumpBin(romPath, WriteLine);
    var gpiList = GameProgramLibraryService.GetGameProgramInfos(romPath);
    if (gpiList.Any())
    {
        WriteLine(@"
Found matching entries in ROMProperties.csv database:");
    }
    else
    {
        WriteLine(@"
No matching entries found in ROMProperties.csv database");
    }

    foreach (var gpi in gpiList)
    {
        WriteLine(@$"
    Title       : {gpi.Title}
    Manufacturer: {gpi.Manufacturer}
    Author      : {gpi.Author}
    Qualifier   : {gpi.Qualifier}
    Year        : {gpi.Year}
    ModelNo     : {gpi.ModelNo}
    Rarity      : {gpi.Rarity}
    CartType    : {gpi.CartType}
    MachineType : {gpi.MachineType}
    LController : {gpi.LController}
    RController : {gpi.RController}
    MD5         : {gpi.MD5}
    HelpUri     : {gpi.HelpUri}");
    }
    EnvironmentExit(0);
}
else
{
    if (args.Length >= 1 && !new[] { "-?", "/?", "--?", "-h", "/h", "--help" }.Any(OptEq))
    {
        AllocConsole();
        WriteLine("Unknown option: " + args[0]);
        EnvironmentExit(0);
    }
    else if (args.Length >= 2 && CiEq(args[1], "enums"))
    {
        AllocConsole();
        WriteLine(@$"
MachineType:
{string.Join("\n", GetMachineTypes())}

CartType:
{string.Join("\n", GetCartTypes())}

Controller:
{string.Join("\n", GetControllers())}");
        EnvironmentExit(0);
    }
    else
    {
        AllocConsole();
        WriteLine(@"
** EMU7800 **
Copyright (c) 2012-2023 Mike Murphy

Usage:
    EMU7800.exe [<option> <filename> [MachineType [CartType [LController [RController]]]]]

Options:
-r <filename>: Try launching Game Program (uses specified machine configuration or .a78 header info)
-d <filename>: Dump Game Program information
-? enums     : List valid MachineTypes, CartTypes, and Controllers
-?           : This help
(none)       : Run Game Program selection menu (specify -c to keep console)");
        EnvironmentExit(0);
    }
}

return 0;

static void EnvironmentExit(int exitCode, bool waitForAnyKey = true)
{
    if (waitForAnyKey)
    {
        WriteLine("");
        WriteLine("Hit any key to close");
        ReadKey();
    }
    Environment.Exit(exitCode);
}

static IEnumerable<string> GetMachineTypes()
    => MachineTypeUtil.GetAllValues().Select(MachineTypeUtil.ToString);

static IEnumerable<string> GetCartTypes()
    => CartTypeUtil.GetAllValues().Select(ct => $"{CartTypeUtil.ToString(ct), -7}: {CartTypeUtil.ToCartTypeWordString(ct)}");

static IEnumerable<string> GetControllers()
    => ControllerUtil.GetAllValues().Select(ControllerUtil.ToString);

bool OptEq(string opt)
    => CiEq(opt, args[0]);

static bool CiEq(string a, string b)
    => string.Compare(a, b, true) == 0;

static void StartGameProgram(GameProgramInfoViewItem gpivi)
{
    using var app = new Win32App(gpivi);
    app.Run();
}

static void Start(bool startMaximized = true)
{
    using var app = new Win32App();
    app.Run(startMaximized);
}

[System.Runtime.InteropServices.DllImport("Kernel32.dll")]
static extern void AllocConsole();
