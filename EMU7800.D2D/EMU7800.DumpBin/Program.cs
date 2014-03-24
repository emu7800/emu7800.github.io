using System;
using System.IO;
using EMU7800.Services;

namespace EMU7800.DumpBin
{
    class Program
    {
        static readonly RomBytesService _romBytesService = new RomBytesService();
        static readonly Md5HashService _md5HashService = new Md5HashService();

        [STAThread]
        static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            Console.WriteLine(@"
EMU7800 DumpBin
Copyright (c) 2012 Mike Murphy");
            if (args == null || args.Length == 0)
            {
                Console.WriteLine(@"
Usage:
    EMU7800.DumpBin.exe <file1> [file2] [...]");
                return 1;
            }

            foreach (var arg in args)
            {
                DumpBin(arg);
            }

            return 0;
        }

        static void DumpBin(string path)
        {
            Console.WriteLine();
            Console.WriteLine("File: {0}", path);
            var bytes = GetBytes(path);
            if (bytes == null)
                return;

            var isA78Format = _romBytesService.IsA78Format(bytes);
            if (isA78Format)
            {
                var gpi = _romBytesService.ToGameProgramInfoFromA78Format(bytes);
                Console.WriteLine(@"A78 : Title           : {0}
      MachineType     : {1}
      CartType        : {2}
      Left Controller : {3}
      Right Controller: {4}", gpi.Title, gpi.MachineType, gpi.CartType, gpi.LController, gpi.RController);
            }

            var rawBytes = _romBytesService.RemoveA78HeaderIfNecessary(bytes);
            var md5 = _md5HashService.ComputeHash(rawBytes);

            Console.WriteLine("MD5 : {0}", md5);
            Console.WriteLine("Size: {0} {1}", rawBytes.Length, isA78Format ? "(excluding a78 header)" : string.Empty);
        }

        static byte[] GetBytes(string path)
        {
            try
            {
                return File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Environment.Exit(1);
        }
    }
}
