; Inno Setup Script for Universal Local Signing Bridge
#define MyAppName "Universal Local Signing Bridge"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Universal PKI Architecture Project"
#define MyAppURL "http://127.0.0.1:8080"
#define MyAppExeName "Bridge.Desktop.exe"

[Setup]
AppId={{C8E192B4-9F81-4C10-9118-E27F4A423219}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\Universal Local Signing Bridge
DisableProgramGroupPage=yes
LicenseFile=..\README.md
PrivilegesRequired=lowest
OutputDir=bin
OutputBaseFilename=Universal-Local-Signing-Bridge-Setup-{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "autostart"; Description: "Automatically start Universal Signing Bridge when Windows logs in"; GroupDescription: "Startup options:"

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: autostart

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "UniversalSigningBridge"; ValueData: """{app}\{#MyAppExeName}"""; Tasks: autostart; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
