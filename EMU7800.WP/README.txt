EMU7800.WP README
April 2015

Legacy Silverlight-based Windows Phone projects. Active maintenance has transitioned to new WinRT-based
Windows Phone runtime.

Decided that servicing existing Silverlight-based release best done with WP8 target for best
combination of compatibility and tooling support.

Release was not obsoleted as soon as WinRT replacement was completed because of failure of the Moga
controller .dll not passing the certification tests for WinRT:

File Moga.Windows.Phone.dll has failed the AppContainerCheck check.
Apply the required linker options - SAFESEH, DYNAMICBASE, NXCOMPAT, and APPCONTAINER - when you link the app. See links below for more information:
http://go.microsoft.com/fwlink/?LinkID=324325


Released Versions
=================
WP7: EMU7800.WP.VS2012.sln   v1.5   no longer serviced
WP8: EMU7800.WP8.sln         v1.9   latest release built using VS2013.2

