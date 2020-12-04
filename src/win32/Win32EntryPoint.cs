using EMU7800.Core;
using EMU7800.D2D.Shell.Win32;
using EMU7800.Services;
using EMU7800.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

using static System.Console;

var option = args.Length > 0 ? args[0].ToLower() : string.Empty;

if (option.Length == 0 || new[] { "-c", "/c" }.Any(s => option.StartsWith(s)))
{
    if (option.Length == 0)
    {
        FreeConsole();
    }
    Start(option.Length == 0);
    return 0;
}

var romPath        = args.Length > 1 ? args[1] : string.Empty;
var machineTypeStr = args.Length > 2 ? args[2] : string.Empty;
var cartTypeStr    = args.Length > 3 ? args[3] : string.Empty;
var lControllerStr = args.Length > 4 ? args[4] : string.Empty;
var rControllerStr = args.Length > 5 ? args[5] : string.Empty;

if (new[] { "-r", "/r" }.Any(s => option.StartsWith(s)))
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
                WriteLine($"Bad CartType '{cartType}' for MachineType '{machineType}'");
                Environment.Exit(-8);
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
                WriteLine($"Bad CartType '{cartType}' for MachineType '{machineType}'");
                Environment.Exit(-8);
            }
            lController = lController is Controller.None ? Controller.ProLineJoystick : lController;
            rController = rController is Controller.None ? Controller.ProLineJoystick : rController;
        }
        else
        {
            WriteLine("Unknown MachineType: " + machineType);
            Environment.Exit(-8);
        }
        WriteLine($"Starting {romPath} with {machineType} {cartType} {lController} {rController}...");

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
            WriteLine("No information in ROMProperties.csv database for: " + romPath);
            Environment.Exit(-8);
        }
    }
}
else if (romPath.Length > 0 && new[] { "-d", "/d" }.Any(s => option.StartsWith(s)))
{
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
}
else
{
    if (!new[] { "-?", "/?", "--?", "-h", "/h", "--help" }.Any(s => option.StartsWith(s)))
    {
        WriteLine("Unknown option: " + option);
    }
    else if (romPath.ToLower() == "enums")
    {
        WriteLine(@$"
MachineType:
{string.Join("\n", GetMachineTypes())}

CartType:
{string.Join("\n", GetCartTypes())}

Controller:
{string.Join("\n", GetControllers())}");
    }
    else
    {
        WriteLine(@"
** EMU7800 **
Copyright (c) 2012-2020 Mike Murphy

Usage:
    EMU7800.exe [<option> <filename> [MachineType [CartType [LController [RController]]]]]

Options:
-r <filename>: Try launching Game Program (uses machine configuration if specified)
-d <filename>: Dump Game Program information
-? enums     : List valid MachineTypes, CartTypes, and Controllers
-?           : This help
             : Run Game Program selection menu (specify -c to keep console)");
    }
}

return 0;

static IEnumerable<string> GetMachineTypes()
    => MachineTypeUtil.GetAllValues().Select(MachineTypeUtil.ToString);

static IEnumerable<string> GetCartTypes()
    => CartTypeUtil.GetAllValues().Select(ct => $"{CartTypeUtil.ToString(ct), -7}: {CartTypeUtil.ToCartTypeWordString(ct)}");

static IEnumerable<string> GetControllers()
    => ControllerUtil.GetAllValues().Select(ControllerUtil.ToString);

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
static extern void FreeConsole();
