using System;
using System.Collections.Generic;
using System.Linq;
using EMU7800.Assets;
using EMU7800.D2D.Interop;
using EMU7800.Services;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell.Win32
{
   public sealed class Win32EntryPoint
    {
        [STAThread]
        public static int Main(string[] args)
        {
            if (args.Length > 0)
            {
                var option = args[0].ToLower();
                var romPath = args.Length > 1 ? args[1] : string.Empty;

                if (romPath.Length > 0 && new[] { "-r", "/r" }.Any(s => option.StartsWith(s)))
                {
                     StartGameProgram(romPath);
                }
                else if (romPath.Length > 0 && new[] { "-d", "/d" }.Any(s => option.StartsWith(s)))
                {
                    RomBytesService.DumpBin(romPath, s => Console.WriteLine(s));

                    var (_, bytes) = DatastoreService.GetRomBytes(romPath);
                    var md5key = RomBytesService.ToMD5Key(bytes);
                    var romPropertiesCsv = AssetService.GetAssetByLines(Asset.ROMProperties);
                    var gpiList = RomPropertiesService.ToGameProgramInfo(romPropertiesCsv).Where(gpi => gpi.MD5 == md5key).ToList();

                    if (gpiList.Any())
                    {
                        Console.WriteLine(@"
Found matching entries in ROMProperties.csv database:");
                    }
                    else
                    {
                        Console.WriteLine(@"
No matching entries found in ROMProperties.csv database");
                    }

                    foreach (var gpi in gpiList)
                    {
                        Console.WriteLine(@$"
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
                else if (romPath.Length > 0 && new[] { "-s", "/s" }.Any(s => option.StartsWith(s)))
                {
                    Console.WriteLine("** not implemented yet **");
                }
                else
                {
                    if (!new[] { "-?", "/?", "--?", "-h", "/h", "--help" }.Any(s => option.StartsWith(s)))
                    {
                        Console.WriteLine("Unknown option: " + args[0]);
                    }

                    Console.WriteLine(@"
** EMU7800 **
Copyright (c) 2012-2020 Mike Murphy

Usage:
    EMU7800.exe [<option> <filespec>]

Options:
-r <filename> : Try launching ROM
-d <filename> : Dump ROM information
-s <inputtape>: Run POKEY sound emulator
-?            : This help
              : Run shell (no option specified)");
                }

            }
            else
            {
                Start();
            }

            return 0;
        }

        public static void Start()
        {
            using var win = new Win32Window();
            using var app = new Win32App(win);
            app.Run();
        }

        public static void StartGameProgram(string romPath)
        {
            var gpivi = ToGameProgramInfoViewItem(romPath);
            if (gpivi == null)
                Environment.Exit(-8);
            StartGameProgram(gpivi);
        }

        public static void StartGameProgram(GameProgramInfoViewItem gpivi)
        {
            using var win = new Win32Window();
            using var app = new Win32App(win, gpivi);
            app.Run();
        }

        public static GameProgramInfoViewItem? ToGameProgramInfoViewItem(string romPath)
        {
            var (_, bytes) = DatastoreService.GetRomBytes(romPath);
            var md5key = RomBytesService.ToMD5Key(bytes);

            var romPropertiesCsv = AssetService.GetAssetByLines(Asset.ROMProperties);
            var gpiList = RomPropertiesService.ToGameProgramInfo(romPropertiesCsv);

            return gpiList
                .Where(gpi => gpi.MD5 == md5key)
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
                .FirstOrDefault();
        }
    }
}
