using System;
using System.Collections.Generic;
using System.Linq;
using EMU7800.Assets;
using EMU7800.Core;
using EMU7800.D2D.Interop;
using EMU7800.Services;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell.Win32
{
   public sealed class Win32EntryPoint
    {
        static readonly Action<string> PrintFn = s => Console.WriteLine(s);

        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                FreeConsole();
                Start();
            }
            else
            {
                var option         = args[0].ToLower();
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
                                var len = GetBytes(romPath).Length;
                                cartType = RomBytesService.InferCartTypeFromSize(machineType, len);
                            }
                            else if (cartTypeStr.StartsWith("A78"))
                            {
                                PrintFn($"Bad CartType '{cartType}' for MachineType '{machineType}'");
                                Environment.Exit(-8);
                            }
                            lController = lController is Controller.None ? Controller.Joystick : lController;
                            rController = rController is Controller.None ? Controller.Joystick : rController;
                        }
                        else if (machineType is MachineType.A7800NTSC or MachineType.A7800PAL)
                        {
                            if (cartType is CartType.Unknown)
                            {
                                var len = RomBytesService.RemoveA78HeaderIfNecessary(GetBytes(romPath)).Length;
                                cartType = RomBytesService.InferCartTypeFromSize(machineType, len);
                            }
                            else if (!cartTypeStr.StartsWith("A78"))
                            {
                                PrintFn($"Bad CartType '{cartType}' for MachineType '{machineType}'");
                                Environment.Exit(-8);
                            }
                            lController = lController is Controller.None ? Controller.ProLineJoystick : lController;
                            rController = rController is Controller.None ? Controller.ProLineJoystick : rController;
                        }
                        else
                        {
                            PrintFn("Unknown MachineType: " + machineType);
                            Environment.Exit(-8);
                        }
                        PrintFn($"Starting {romPath} with {machineType} {cartType} {lController} {rController}...");

                        StartGameProgram(new GameProgramInfoViewItem
                        {
                            ImportedGameProgramInfo = new()
                            {
                                GameProgramInfo = new()
                                {
                                    CartType    = cartType,
                                    MachineType = machineType,
                                    LController = lController,
                                    RController = rController
                                },
                                StorageKeySet = new HashSet<string> { romPath }
                            }
                        });
                    }
                    else
                    {
                        StartGameProgram(romPath);
                    }
                }
                else if (romPath.Length > 0 && new[] { "-d", "/d" }.Any(s => option.StartsWith(s)))
                {
                    RomBytesService.DumpBin(romPath, PrintFn);
                    var gpiList = GetGameProgramInfos(romPath);
                    if (gpiList.Any())
                    {
                        PrintFn(@"
Found matching entries in ROMProperties.csv database:");
                    }
                    else
                    {
                        PrintFn(@"
No matching entries found in ROMProperties.csv database");
                    }

                    foreach (var gpi in gpiList)
                    {
                        PrintFn(@$"
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
                        PrintFn("Unknown option: " + option);
                    }
                    else if (romPath.ToLower() == "enums")
                    {
                        PrintFn(@$"
MachineType:
{string.Join("\n", GetMachineTypes())}

CartType:
{string.Join("\n", GetCartTypes())}

Controller:
{string.Join("\n", GetControllers())}");
                    }
                    else
                    {
                        PrintFn(@"
** EMU7800 **
Copyright (c) 2012-2020 Mike Murphy

Usage:
    EMU7800.exe [<option> <filename> [MachineType [CartType [LController [RController]]]]]

Options:
-r <filename>: Try launching Game Program (uses machine configuration if specified)
-d <filename>: Dump Game Program information
-? enums     : List valid MachineTypes, CartTypes, and Controllers
-?           : This help
             : Run Game Program selection menu (no option specified)");
                    }
                }
            }

            return 0;

            static IEnumerable<string> GetMachineTypes()
                => MachineTypeUtil.GetAllValues().Select(MachineTypeUtil.ToString);

            static IEnumerable<string> GetCartTypes()
                => CartTypeUtil.GetAllValues().Select(ct => $"{CartTypeUtil.ToString(ct), -7}: {CartTypeUtil.ToCartTypeWordString(ct)}");

            static IEnumerable<string> GetControllers()
                => ControllerUtil.GetAllValues().Select(ControllerUtil.ToString);
        }

        static void StartGameProgram(string romPath)
        {
            var gpiviList = GetGameProgramInfoViewItems(romPath);
            if (gpiviList.Any())
            {
                StartGameProgram(gpiviList.First());
            }
            else
            {
                PrintFn("No information in ROMProperties.csv database for: " + romPath);
                Environment.Exit(-8);
            }
        }

        static void StartGameProgram(GameProgramInfoViewItem gpivi)
        {
            using var win = new Win32Window();
            using var app = new Win32App(win, gpivi);
            app.Run();
        }

        static void Start()
        {
            using var win = new Win32Window();
            using var app = new Win32App(win);
            app.Run();
        }

        static IList<GameProgramInfoViewItem> GetGameProgramInfoViewItems(string romPath)
            => GetGameProgramInfos(romPath)
                .Select(gpi => new GameProgramInfoViewItem
                {
                    Title    = gpi.Title,
                    SubTitle = $"{gpi.Manufacturer} {gpi.Year}",
                    ImportedGameProgramInfo  = new()
                    {
                        GameProgramInfo      = gpi,
                        PersistedStateExists = false,
                        StorageKeySet        = new HashSet<string> { romPath }
                    }
                })
                .ToList();

        static IList<GameProgramInfo> GetGameProgramInfos(string romPath)
        {
            var bytes = DatastoreService.GetRomBytes(romPath);
            var md5key = RomBytesService.ToMD5Key(bytes);
            var romPropertiesCsv = AssetService.GetAssetByLines(Asset.ROMProperties);
            return RomPropertiesService.ToGameProgramInfo(romPropertiesCsv)
                .Where(gpi => gpi.MD5 == md5key)
                .ToList();
        }

        static byte[] GetBytes(string romPath)
        {
            try
            {
                return System.IO.File.ReadAllBytes(romPath);
            }
            catch (Exception)
            {
                return Array.Empty<byte>();
            }
        }

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern int FreeConsole();
    }
}
