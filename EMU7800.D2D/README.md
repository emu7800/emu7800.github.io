# EMU7800 D2D README
June 2015

#### Versions

- **WinRT_8 v1.1** - no longer serviced
- **WinRT_8.1 v1.3** - built using VS2013.2
- **WinRT_8.1WP v1.9** - did not release, failed certification, need Moga library update, will wait for .NET Native
- **Win32** - no longer upgrading C++ project tools version for backward compatibility, left at v120 (VS2013)
- **MonoDroid** - actively serviced Android/Xamarin version
- **WinUWP** - actively serviced

File ``Moga.Windows.Phone.dll`` has failed the AppContainerCheck check.
Apply the required linker options - ``SAFESEH``, ``DYNAMICBASE``, ``NXCOMPAT``, and ``APPCONTAINER`` - when you link the app.
See links below for more information:
[Windows security features test: BinScope Binary Analyzer tests](http://go.microsoft.com/fwlink/?LinkID=324325)
