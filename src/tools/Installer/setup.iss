; Inno Setup downloadable from https://jrsoftware.org/isinfo.php

#define MyAppName "EMU7800"
#define MyAppVersion "5.0.0"
#define MyAppPublisher "Mike Murphy"
#define MyAppURL "http://emu7800.net"
#define MyAppExeName "EMU7800.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{AD789DFB-1C97-4344-92A6-20AF95D86DFD}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={localappdata}\Programs\{#MyAppName}
DisableProgramGroupPage=yes
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=commandline
OutputDir=..\..\..\artifacts
OutputBaseFilename=EMU7800Setup-x64-5.0.0
SetupIconFile=..\..\shell\EMUIcon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\..\..\artifacts\EMU7800.bin\*.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\..\artifacts\EMU7800.bin\*.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\..\artifacts\EMU7800.bin\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\..\artifacts\EMU7800.bin\ROMS\*.zip"; DestDir: "{app}\ROMS"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

