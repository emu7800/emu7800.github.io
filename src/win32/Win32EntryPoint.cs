﻿using EMU7800.Core;
using EMU7800.D2D.Shell.Win32;
using EMU7800.Services;
using EMU7800.Services.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static System.Console;

if (IsArg0("-rc", "/rc", "-r", "/r"))
{
    if (IsArg0("-rc", "/rc"))
    {
        AllocConsole();
    }

    var romPath = args.Length > 1 ? args[1] : string.Empty;
    if (string.IsNullOrWhiteSpace(romPath) || !File.Exists(romPath))
    {
        WriteLine($"Rom path not specified or not found: '{romPath}'");
        EnvironmentExit(-8);
    }

    var machineType = args.Select(arg => MachineTypeUtil.From(arg)).FirstOrDefault(mt => mt != MachineType.Unknown);
    var cartType    = args.Select(arg => CartTypeUtil.From(arg)).FirstOrDefault(ct => ct != CartType.Unknown);
    var lController = args.Select(arg => ControllerUtil.From(arg)).FirstOrDefault(co => co != Controller.None);
    var rController = args.Select(arg => ControllerUtil.From(arg)).Where(co => co != Controller.None).Skip(1).FirstOrDefault();

    if (machineType != MachineType.Unknown)
    {
        RomImportService.Import();
        if (MachineTypeUtil.Is2600(machineType))
        {
            if (cartType is CartType.Unknown)
            {
                var bytes = DatastoreService.GetRomBytes(romPath);
                var len = bytes.Length;
                cartType = RomBytesService.InferCartTypeFromSize(machineType, len);
            }
            else if (cartType.ToString().StartsWith("A78"))
            {
                WriteLine($"Bad CartType '{cartType}' for MachineType '{machineType}'");
                EnvironmentExit(-8);
            }
            lController = lController is Controller.None ? Controller.Joystick : lController;
            rController = rController is Controller.None ? Controller.Joystick : rController;
        }
        else if (MachineTypeUtil.Is7800(machineType))
        {
            if (cartType is CartType.Unknown)
            {
                var bytes = DatastoreService.GetRomBytes(romPath);
                var len = RomBytesService.RemoveA78HeaderIfNecessary(bytes).Length;
                cartType = RomBytesService.InferCartTypeFromSize(machineType, len);
            }
            else if (!cartType.ToString().StartsWith("A78"))
            {
                WriteLine($"Bad CartType '{cartType}' for MachineType '{machineType}'");
                EnvironmentExit(-8);
            }
            lController = lController is Controller.None ? Controller.ProLineJoystick : lController;
            rController = rController is Controller.None ? Controller.ProLineJoystick : rController;
        }
        else
        {
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
                WriteLine("No information in ROMProperties.csv database for: " + romPath);
                EnvironmentExit(-8);
            }
        }
    }
    EnvironmentExit(0);
}

if (IsArg0("-d", "/d"))
{
    AllocConsole();
    var romPath = args.Length > 1 ? args[1] : string.Empty;
    var romPaths = string.IsNullOrEmpty(romPath)
        ? []
        : Directory.Exists(romPath)
            ? new DirectoryInfo(romPath).GetFiles().Where(IsBinOrA78File).Select(fi => fi.FullName).ToList()
            : [romPath];

    foreach (var path in romPaths)
    {
        RomBytesService.DumpBin(path, WriteLine);
        var gpiList = GameProgramLibraryService.GetGameProgramInfos(path);
        WriteLine(gpiList.Any()
            ? """

              Found matching entries in ROMProperties.csv database:
              """
            : """

              No matching entries found in ROMProperties.csv database
              """);

        foreach (var gpi in gpiList)
        {
            WriteLine($"""

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
                           HelpUri     : {gpi.HelpUri}

                           Launch by copying and pasting to Start > Run:
                           "{Environment.ProcessPath}" -r "{Path.GetFullPath(path)}" {gpi.MachineType} {gpi.CartType} {gpi.LController} {gpi.RController}
                       """);
        }
    }

    EnvironmentExit(0);
}

if (IsArg0("-?", "/?", "-h", "/h", "--help"))
{
    AllocConsole();
    WriteLine($"""

               ** EMU7800 **
               Copyright (c) 2012-2024 Mike Murphy

               Usage:
                   EMU7800.exe [<option> <filename|path> [MachineType [CartType [LController [RController]]]]]

               Options:
               -r <filename> : Try launching Game Program using specified machine configuration or .a78 header info
               -rc <filename>: Like -r, but opens console window
               -d <path>     : Dump Game Program information
               -c            : Run Game Program selection menu with console window
               (none)        : Run Game Program selection menu without console window
               -?            : This help

               MachineTypes:
               {string.Join(Environment.NewLine, GetMachineTypes())}

               CartTypes:
               {string.Join(Environment.NewLine, GetCartTypes())}

               Controllers:
               {string.Join(Environment.NewLine, GetControllers())}
               """);
    EnvironmentExit(0);
}

if (IsArg0("-c", "/c"))
{
    AllocConsole();
    Start(false);
    EnvironmentExit(0);
}

if (args.Length == 0)
{
    Start();
    EnvironmentExit(0, false);
}

AllocConsole();
WriteLine("Unknown option: " + args[0]);
EnvironmentExit(-8);

static void EnvironmentExit(int exitCode, bool waitForAnyKey = true)
{
    if (waitForAnyKey)
    {
        WriteLine("""

                  Hit any key to close
                  """);
        ReadKey();
    }
    Environment.Exit(exitCode);
}

static bool IsBinOrA78File(FileInfo fi)
    => CiEq(fi.Extension, ".a78")
    || CiEq(fi.Extension, ".bin");

static IEnumerable<string> GetMachineTypes()
    => MachineTypeUtil.GetAllValues().Select(mt => $"{MachineTypeUtil.ToString(mt), -13}: {MachineTypeUtil.ToMachineTypeWordString(mt)}");

static IEnumerable<string> GetCartTypes()
    => CartTypeUtil.GetAllValues().Select(ct => $"{CartTypeUtil.ToString(ct), -13}: {CartTypeUtil.ToCartTypeWordString(ct)}");

static IEnumerable<string> GetControllers()
    => ControllerUtil.GetAllValues().Select(ControllerUtil.ToString);

bool IsArg0(params string[] options)
    => args.Length >= 1 && options.Any(opt => CiEq(opt, args[0]));

static bool CiEq(string a, string b)
    => string.Compare(a, b, StringComparison.OrdinalIgnoreCase) == 0;

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