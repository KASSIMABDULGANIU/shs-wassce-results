; ============================================================
;  SHS WASSCE Results Management — Installer Script
;  Build with Inno Setup (https://jrsoftware.org/isinfo.php)
;
;  HOW TO USE:
;  1. Install Inno Setup (free)
;  2. First publish your app:
;       dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
;  3. Open this file in Inno Setup, click "Compile"
;  4. Output: Output\SHSWassceResultsMgt-Setup.exe
; ============================================================

#define MyAppName "SHS WASSCE Results Management"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "HASCO"
#define MyAppExeName "SHSWassceResultsMgt.exe"

; Path to the published output folder (adjust if your project folder name differs)
#define PublishDir "bin\Release\net9.0-windows\win-x64\publish"

[Setup]
AppId={{B6E2A6B0-9F3C-4D7A-9E2D-SHSWASSCEAPP1}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=SHSWassceResultsMgt-Setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Files]
; Copy everything from the publish folder
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Optional: keep the database when uninstalling so user data isn't lost.
; Comment this out if you DO want to delete the DB on uninstall.
; Type: files; Name: "{app}\shs_wassce.db"
