using EMU7800.Core;
using EMU7800.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EMU7800.Shell;

public sealed class CommandLine
{
    readonly ILogger _logger;

    static CommandLine()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    public void Run(ICommandLineDriver driver, string[] args)
    {
        if (IsArg0(args, ["-r", "/r"]))
        {
            var romPath = args.Length > 1 ? args[1] : string.Empty;
            if (string.IsNullOrWhiteSpace(romPath) || !File.Exists(romPath))
            {
                _logger.Log(1, $"Rom path not specified or not found: '{romPath}'");
                Environment.Exit(-8);
            }

            var machineType = args.Select(MachineTypeUtil.From).FirstOrDefault(mt => mt != MachineType.Unknown);
            var cartType    = args.Select(CartTypeUtil.From).FirstOrDefault(ct => ct != CartType.Unknown);
            var lController = args.Select(ControllerUtil.From).FirstOrDefault(co => co != Controller.None);
            var rController = args.Select(ControllerUtil.From).Where(co => co != Controller.None).Skip(1).FirstOrDefault();

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
                        _logger.Log(1, $"Bad CartType '{cartType}' for MachineType '{machineType}'");
                        Environment.Exit(-8);
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
                        _logger.Log(1, $"Bad CartType '{cartType}' for MachineType '{machineType}'");
                        Environment.Exit(-8);
                    }
                    lController = lController is Controller.None ? Controller.ProLineJoystick : lController;
                    rController = rController is Controller.None ? Controller.ProLineJoystick : rController;
                }
                else
                {
                    _logger.Log(1, "Unknown MachineType: " + machineType);
                    Environment.Exit(-8);
                }
                driver.StartGameProgram(new(machineType, cartType, lController, rController, romPath), false);
            }
            else
            {
                var gpiviList = GameProgramLibraryService.GetGameProgramInfoViewItems(romPath);
                if (gpiviList.Count > 0)
                {
                    driver.StartGameProgram(gpiviList.First(), false);
                }
                else
                {
                    var bytes = DatastoreService.GetRomBytes(romPath);
                    if (RomBytesService.IsA78Format(bytes))
                    {
                        var gpi = RomBytesService.ToGameProgramInfoFromA78Format(bytes);
                        driver.StartGameProgram(new(gpi, string.Empty, romPath), false);
                    }
                    else
                    {
                        _logger.Log(1, "No information in ROMProperties.csv database for: " + romPath);
                        Environment.Exit(-8);
                    }
                }
            }

            Environment.Exit(0);
        }

        if (IsArg0(args, ["-d", "/d"]))
        {
            var romPath = args.Length > 1 ? args[1] : string.Empty;
            var romPaths = string.IsNullOrEmpty(romPath)
                ? []
                : Directory.Exists(romPath)
                    ? new DirectoryInfo(romPath).GetFiles().Where(IsBinOrA78File).Select(fi => fi.FullName).ToList()
                    : [romPath];

            foreach (var path in romPaths)
            {
                RomBytesService.DumpBin(path, m => _logger.Log(1, m));
                var gpiList = GameProgramLibraryService.GetGameProgramInfos(path);
                _logger.Log(1, gpiList.Count > 0
                          ? """

                    Found matching entries in ROMProperties.csv database:
                    """
                          : """

                    No matching entries found in ROMProperties.csv database
                    """);

                foreach (var gpi in gpiList)
                {
                    _logger.Log(1, $"""

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

                           Launch by using the following command line:
                           "{Environment.ProcessPath}" -r "{Path.GetFullPath(path)}" {gpi.MachineType} {gpi.CartType} {gpi.LController} {gpi.RController}
                       """);
                }
            }

            Environment.Exit(0);
        }

        if (IsArg0(args, ["-?", "/?", "-h", "/h", "--help"]))
        {
            _logger.Log(1, $"""

               ** {VersionInfo.EMU7800} {VersionInfo.AssemblyVersion} **
               {VersionInfo.Author}

               Usage:
                   {VersionInfo.ExecutableName} [<option> <filename|path> [MachineType [CartType [LController [RController]]]]]

               Options:
               -r <filename> : Try launching Game Program using specified machine configuration or .a78 header info
               -d <path>     : Dump Game Program information
               -c            : Open console window (Windows only)
               (none)        : Run Game Program selection menu
               -?            : This help

               MachineTypes:
               {string.Join(Environment.NewLine, GetMachineTypes())}

               CartTypes:
               {string.Join(Environment.NewLine, GetCartTypes())}

               Controllers:
               {string.Join(Environment.NewLine, GetControllers())}
               """);

            Environment.Exit(0);
        }

        driver.Start(false);

        Environment.Exit(0);
    }

    #region Constructors

    #pragma warning disable IDE0290

    public CommandLine(ILogger logger)
      => _logger = logger;

    #endregion

    static bool IsBinOrA78File(FileInfo fi)
        => CiEq(fi.Extension, ".a78")
        || CiEq(fi.Extension, ".bin");

    static IEnumerable<string> GetMachineTypes()
        => MachineTypeUtil.GetAllValues().Select(mt => $"{mt,-13}: {MachineTypeUtil.ToMachineTypeWordString(mt)}");

    static IEnumerable<string> GetCartTypes()
        => CartTypeUtil.GetAllValues().Select(ct => $"{ct,-13}: {CartTypeUtil.ToCartTypeWordString(ct)}");

    static IEnumerable<string> GetControllers()
        => ControllerUtil.GetAllValues().Select(c => c.ToString());

    static bool IsArg0(string[] args, string[] options)
        => args.Length >= 1 && options.Any(opt => CiEq(opt, args[0]));

    static bool CiEq(string a, string b)
        => string.Compare(a, b, StringComparison.OrdinalIgnoreCase) == 0;

    static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            DatastoreService.DumpCrashReport(ex);
        }
    }
}
