EMU7800 D2D README
April 2015

On Win32, every Visual Studio release increments C++ project tools version.
.SLNs that target Win32 and use C++ projects have per-VS level solution files.


Released Versions
=================
WinRT_8:      v1.1  (no longer serviced)
WinRT_8.1:    v1.3  (built using VS2013.2)
WinRT_8.1WP:  v1.9  (did not release, failed certification, need moga library update, will wait for .net native)

File Moga.Windows.Phone.dll has failed the AppContainerCheck check.
Apply the required linker options - SAFESEH, DYNAMICBASE, NXCOMPAT, and APPCONTAINER - when you link the app. See links below for more information:
http://go.microsoft.com/fwlink/?LinkID=324325

