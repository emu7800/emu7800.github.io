using EMU7800.Core;
using EMU7800.Services;
using EMU7800.Services.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace EMU7800.Shell;

public interface ICommandLineDriver
{
    void AttachConsole(bool allocNewConsole = false);
    void Start(bool startMaximized = true);
    void StartGameProgram(GameProgramInfoViewItem gpivi);
}

public sealed class EmptyCommandLineDriver : ICommandLineDriver
{
    public static readonly EmptyCommandLineDriver Default = new();
    EmptyCommandLineDriver() {}

    #region ICommandLineDriver Members
    public void AttachConsole(bool allocNewConsole = false) {}
    public void Start(bool startMaximized = true) {}
    public void StartGameProgram(GameProgramInfoViewItem gpivi) {}

    #endregion
}

public static class CommandLine
{
    static CommandLine()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    public static Func<ICommandLineDriver> DriverFactory { get; set; } = () => EmptyCommandLineDriver.Default;

    static ICommandLineDriver _driver = EmptyCommandLineDriver.Default;

    public static void Run(string[] args)
    {
        _driver = DriverFactory();

        if (IsArg0(args, ["-r", "/r"]))
        {
            _driver.AttachConsole();

            var romPath = args.Length > 1 ? args[1] : string.Empty;
            if (string.IsNullOrWhiteSpace(romPath) || !File.Exists(romPath))
            {
                Console.WriteLine($"Rom path not specified or not found: '{romPath}'");
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
                        Console.WriteLine($"Bad CartType '{cartType}' for MachineType '{machineType}'");
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
                        Console.WriteLine($"Bad CartType '{cartType}' for MachineType '{machineType}'");
                        Environment.Exit(-8);
                    }
                    lController = lController is Controller.None ? Controller.ProLineJoystick : lController;
                    rController = rController is Controller.None ? Controller.ProLineJoystick : rController;
                }
                else
                {
                    Console.WriteLine("Unknown MachineType: " + machineType);
                    Environment.Exit(-8);
                }
                _driver.StartGameProgram(new(machineType, cartType, lController, rController, romPath));
            }
            else
            {
                var gpiviList = GameProgramLibraryService.GetGameProgramInfoViewItems(romPath);
                if (gpiviList.Count > 0)
                {
                    _driver.StartGameProgram(gpiviList.First());
                }
                else
                {
                    var bytes = DatastoreService.GetRomBytes(romPath);
                    if (RomBytesService.IsA78Format(bytes))
                    {
                        var gpi = RomBytesService.ToGameProgramInfoFromA78Format(bytes);
                        _driver.StartGameProgram(new(gpi, string.Empty, romPath));
                    }
                    else
                    {
                        Console.WriteLine("No information in ROMProperties.csv database for: " + romPath);
                        Environment.Exit(-8);
                    }
                }
            }

            Environment.Exit(0);
        }

        if (IsArg0(args, ["-d", "/d"]))
        {
            _driver.AttachConsole();

            var romPath = args.Length > 1 ? args[1] : string.Empty;
            var romPaths = string.IsNullOrEmpty(romPath)
                ? []
                : Directory.Exists(romPath)
                    ? new DirectoryInfo(romPath).GetFiles().Where(IsBinOrA78File).Select(fi => fi.FullName).ToList()
                    : [romPath];

            foreach (var path in romPaths)
            {
                RomBytesService.DumpBin(path, Console.WriteLine);
                var gpiList = GameProgramLibraryService.GetGameProgramInfos(path);
                Console.WriteLine(gpiList.Count > 0
                          ? """

                    Found matching entries in ROMProperties.csv database:
                    """
                          : """

                    No matching entries found in ROMProperties.csv database
                    """);

                foreach (var gpi in gpiList)
                {
                    Console.WriteLine($"""

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
            _driver.AttachConsole();

            Console.WriteLine($"""

               ** {VersionInfo.EMU7800} {VersionInfo.AssemblyVersion} **
               {VersionInfo.Author}

               Usage:
                   {VersionInfo.ExecutableName} [<option> <filename|path> [MachineType [CartType [LController [RController]]]]]

               Options:
               -r <filename> : Try launching Game Program using specified machine configuration or .a78 header info
               -d <path>     : Dump Game Program information
               -c            : Run Game Program selection menu with new console window (Windows only)
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

        if (IsArg0(args, ["-c", "/c"]))
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            _driver.AttachConsole(isWindows);

            _driver.Start(false);

            if (isWindows)
            {
                Console.WriteLine("""

                    Hit RETURN to close
                    """);
                Console.ReadLine();
            }

            Environment.Exit(0);
        }

        if (args.Length == 0)
        {
            _driver.AttachConsole();

            _driver.Start();

            Environment.Exit(0);
        }

        _driver.AttachConsole();

        Console.WriteLine($"Unknown option: {args[0]}");

        Environment.Exit(-8);
    }

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
