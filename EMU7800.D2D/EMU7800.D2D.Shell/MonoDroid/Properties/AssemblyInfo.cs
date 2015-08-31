using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("EMU7800")]
[assembly: AssemblyDescription("An Atari 2600/7800 .NET-based Emulator for MonoDroid")]

[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]

[assembly: AssemblyProduct("EMU7800 for MonoDroid")]
[assembly: AssemblyCompany("Mike Murphy")]
[assembly: AssemblyCopyright("Copyright © 2012-2015 Mike Murphy")]
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]

#if DEBUG
[assembly: Android.App.Application(Debuggable=true)]
#else
[assembly: Android.App.Application(Debuggable=false)]
#endif
