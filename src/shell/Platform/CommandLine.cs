using EMU7800.Core;
using EMU7800.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EMU7800.Shell;

public sealed class CommandLine
{
    readonly IFileSystemAccessor _fileSystemAccessor;
    readonly ILogger _logger;

    readonly DatastoreService _datastoreSvc;

    public void Run(IWindowDriver driver, string[] args)
    {
        try
        {
            RunInternal(driver, args);
        }
        catch (Exception ex)
        {
            _datastoreSvc.DumpCrashReport(ex);
        }
    }

    void RunInternal(IWindowDriver driver, string[] args)
    {
        PrintBanner(args);

        if (GetHelpOption(args))
        {
            PrintHelp();
            Environment.Exit(0);
        }
        else if (TryGetRunGameOption(args, out var rompathtorun))
        {
            var window = RunGameProgram(rompathtorun, args);
            if (window is not null)
            {
                driver.Start(window, false);
            }
        }
        else if (TryGetDumpGameInfoOption(args, out var rompathtodump))
        {
            DumpRomInfo(rompathtodump);
        }
        else
        {
            driver.Start(new(_datastoreSvc, _logger), GetFullscreenOption(args));
        }

        Environment.Exit(0);
    }

    public void DumpRomInfo(string romPath)
    {
        var gameProgramLibrarySvc = new GameProgramLibraryService(_datastoreSvc);

        var romBytesSvc = new RomBytesService(_fileSystemAccessor, _logger);
        romBytesSvc.DumpBin(romPath);

        var gpiList = gameProgramLibrarySvc.GetGameProgramInfos(romPath);

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
                "{Environment.ProcessPath}" -r "{romPath}" {gpi.MachineType} {gpi.CartType} {gpi.LController} {gpi.RController}
            """);
        }
    }

    public void PrintHelp()
    {
        _logger.Log(1, $"""
               Usage:
                   {VersionInfo.ExecutableName} [<option> <filename|path> [MachineType [CartType [LController [RController]]]]]

               Options:
               -r <filename> : Try launching Game Program using specified machine configuration or .a78 header info
               -d <path>     : Dump Game Program information
               -c            : Open console window (Windows only)
               -f            : Run fullscreen
               -v <0-9>      : Logging verbosity level (0 = no logging, 9 = most verbose)
               (none)        : Run Game Program selection menu

               MachineTypes:
               {string.Join(Environment.NewLine, GetMachineTypes())}

               CartTypes:
               {string.Join(Environment.NewLine, GetCartTypes())}

               Controllers:
               {string.Join(Environment.NewLine, GetControllers())}
               """);
    }

    public void PrintBanner(string[] args)
    {
        _logger.Log(1, $"""

               ** {VersionInfo.EMU7800} {VersionInfo.AssemblyVersion} **
               {VersionInfo.Author}
               """);

        if (GetHelpOption(args))
        {
            _logger.Log(1, $"""

               """);
            return;
        }
        else
        {
            _logger.Log(1, $"""
               -? for help

               """);
        }
    }

    public Window? RunGameProgram(string romPath, string[] args)
    {
        if (string.IsNullOrWhiteSpace(romPath))
        {
            _logger.Log(1, "Rom path not specified.");
            Environment.Exit(-8);
        }

        var machineType = args.Select(MachineTypeUtil.From).FirstOrDefault(mt => mt != MachineType.Unknown);
        var cartType = args.Select(CartTypeUtil.From).FirstOrDefault(ct => ct != CartType.Unknown);
        var lController = args.Select(ControllerUtil.From).FirstOrDefault(co => co != Controller.None);
        var rController = args.Select(ControllerUtil.From).Where(co => co != Controller.None).Skip(1).FirstOrDefault();

        _logger.Log(3, "Importing ROMs...");

        var romImportSvc = new RomImportService(_datastoreSvc);
        var importedRoms = romImportSvc.Import();

        _logger.Log(3, "Importing ROMs completed.");

        if (machineType != MachineType.Unknown)
        {
            if (MachineTypeUtil.Is2600(machineType))
            {
                if (cartType is CartType.Unknown)
                {
                    var bytes = _datastoreSvc.GetRomBytes(romPath);
                    var len = bytes.Length;
                    cartType = RomBytesService.InferCartTypeFromSize(machineType, len);
                }
                else if (cartType.ToString().StartsWith("A78"))
                {
                    _logger.Log(1, $"Bad CartType '{cartType}' for MachineType '{machineType}'");
                    return null;
                }
                lController = lController is Controller.None ? Controller.Joystick : lController;
                rController = rController is Controller.None ? Controller.Joystick : rController;
            }
            else if (MachineTypeUtil.Is7800(machineType))
            {
                if (cartType is CartType.Unknown)
                {
                    var bytes = _datastoreSvc.GetRomBytes(romPath);
                    var len = RomBytesService.RemoveA78HeaderIfNecessary(bytes).Length;
                    cartType = RomBytesService.InferCartTypeFromSize(machineType, len);
                }
                else if (!cartType.ToString().StartsWith("A78"))
                {
                    _logger.Log(1, $"Bad CartType '{cartType}' for MachineType '{machineType}'");
                    return null;
                }
                lController = lController is Controller.None ? Controller.ProLineJoystick : lController;
                rController = rController is Controller.None ? Controller.ProLineJoystick : rController;
            }
            else
            {
                _logger.Log(1, "Unknown MachineType: " + machineType);
                return null;
            }

            return new(new(machineType, cartType, lController, rController, romPath), importedRoms.SpecialBinaries, _datastoreSvc, _logger);
        }
        else
        {
            var gameProgramLibrarySvc = new GameProgramLibraryService(_datastoreSvc);
            var gpiviList = gameProgramLibrarySvc.GetGameProgramInfoViewItems(romPath);
            if (gpiviList.Count > 0)
            {
                return new(gpiviList.First(), importedRoms.SpecialBinaries, _datastoreSvc, _logger);
            }
            else
            {
                var bytes = _datastoreSvc.GetRomBytes(romPath);
                if (RomBytesService.IsA78Format(bytes))
                {
                    var gpi = RomBytesService.ToGameProgramInfoFromA78Format(bytes);
                    return new(new(gpi, string.Empty, romPath), importedRoms.SpecialBinaries, _datastoreSvc, _logger);
                }
                else
                {
                    _logger.Log(1, "No information in ROMProperties.csv database for: " + romPath);
                    return null;
                }
            }
        }
    }

    public static int GetLoggingVerbosityOption(string[] args, int defaultLevel = 3)
      => TryGetIntOption(args, out var level, "v") ? level : defaultLevel;

    public static bool TryGetDumpGameInfoOption(string[] args, out string path)
      => TryGetStringOption(args, out path, "d");

    public static bool TryGetRunGameOption(string[] args, out string path)
      => TryGetStringOption(args, out path, "r");

    public static bool GetFullscreenOption(string[] args)
      => GetBooleanOptionFlag(args, "f");

    public static bool GetHelpOption(string[] args)
      => GetBooleanOptionFlag(args, "?", "h");

    public static bool GetOpenConsoleOption(string[] args)
      => GetBooleanOptionFlag(args, "c");

    #region Constructors

    public CommandLine(ILogger logger)
      : this(new FileSystemAccessor(logger), logger) {}

    public CommandLine(IFileSystemAccessor fileSystemAccessor, ILogger logger)
    {
        _logger = logger;
        _fileSystemAccessor = fileSystemAccessor;
        _datastoreSvc = new(_fileSystemAccessor, _logger);
    }

    #endregion

    static IEnumerable<string> GetMachineTypes()
        => MachineTypeUtil.GetAllValues().Select(mt => $"{mt,-13}: {MachineTypeUtil.ToMachineTypeWordString(mt)}");

    static IEnumerable<string> GetCartTypes()
        => CartTypeUtil.GetAllValues().Select(ct => $"{ct,-13}: {CartTypeUtil.ToCartTypeWordString(ct)}");

    static IEnumerable<string> GetControllers()
        => ControllerUtil.GetAllValues().Select(c => c.ToString());

    static bool TryGetIntOption(string[] args, out int val, params string[] options)
    {
        var i = FindOptionIndex(args, options);
        val = i >= 0 ? GetIntFromOptionIndex(args, i) : -1;
        return i >= 0;
    }

    static bool TryGetStringOption(string[] args, out string val, params string[] options)
    {
        var i = FindOptionIndex(args, options);
        val = i >= 0 ? GetStrFromOptionIndex(args, i) : string.Empty;
        return i >= 0;
    }

    static bool GetBooleanOptionFlag(string[] args, params string[] options)
      => FindOptionIndex(args, options) >= 0;

    static int GetIntFromOptionIndex(string[] args, int i)
      => i < 0 || i + 1 >= args.Length || !int.TryParse(args[i + 1], out var intval) ? -1 : intval;

    static string GetStrFromOptionIndex(string[] args, int i)
      => i < 0 || i + 1 >= args.Length ? string.Empty : args[i].Trim();

    static int FindOptionIndex(string[] args, params string[] options)
      => args.Select((a, i) => new { a, i})
             .Where(r => options.Any(o => CiEq(r.a, $"-{o}") || CiEq(r.a, $"/{o}")))
             .Select(r => r.i)
             .DefaultIfEmpty(-1)
             .FirstOrDefault();

    static bool CiEq(string a, string b)
        => string.Compare(a, b, StringComparison.OrdinalIgnoreCase) == 0;
}
