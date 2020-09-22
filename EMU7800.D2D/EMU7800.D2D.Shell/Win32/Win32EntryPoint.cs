using System;
using System.Collections.Generic;
using System.Linq;
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
            if (args.Length > 0 && System.IO.File.Exists(args[0]))
            {
                StartGameProgram(args[0]);
            }
            else
            {
                Start();
            }

            return 0;
        }

        public static void Start()
        {
            using (var win = new Win32Window())
            using (var app = new Win32App(win))
            {
                app.Run();
            }
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
            using (var win = new Win32Window())
            using (var app = new Win32App(win, gpivi))
            {
                app.Run();
            }
        }

        public static GameProgramInfoViewItem ToGameProgramInfoViewItem(string romPath)
        {
            var datastoreService = new DatastoreService();
            var romBytesService = new RomBytesService();

            var (getBytesResult, bytes) = DatastoreService.GetRomBytes(romPath);
            var md5key = romBytesService.ToMD5Key(bytes);

            var (getContentResult, csvFileContent) = DatastoreService.GetGameProgramInfoFromReferenceRepository();
            var gameProgramInfoSet = RomPropertiesService.ToGameProgramInfo(csvFileContent);

            return gameProgramInfoSet
                .Where(gpi => gpi.MD5 == md5key)
                .Select(gpi => new GameProgramInfoViewItem
                {
                    Title    = gpi.Title,
                    SubTitle = $"{gpi.Manufacturer} {gpi.Year}",
                    ImportedGameProgramInfo  = new ImportedGameProgramInfo
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
