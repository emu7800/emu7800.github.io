#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:property LangVersion=14.0
#:property Nullable=enable
#:property UseArtifactsOutput=false

using System.Diagnostics;
using System.Runtime.InteropServices;

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    Console.WriteLine("This only runs on Windows.");
    return -1;
}

using var p = new Process();
p.StartInfo.UseShellExecute = true;
p.StartInfo.CreateNoWindow = true;
p.StartInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Inno Setup 6", "ISCC.exe");
p.StartInfo.Arguments = "setup.iss";
p.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
p.Start();
p.WaitForExit();

return 0;