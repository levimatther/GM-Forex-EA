#define AppName                 "GM Forex EA"
#define InstallerName           "GM-Forex-EA"
#define AppVersion              "0.9.0"
#define Publisher               "Neo Eureka"
#define MT4DataFolderPath       "MetaQuotes\Terminal\1640F6577B1C4EC659BF41EA9F6C38ED"

[Setup]
AppId={{799BB897-D48F-4215-9CAE-1B01CF79735B}}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#Publisher}
DefaultDirName={autopf}\{#AppName}
OutputBaseFilename={#InstallerName}
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "Files\Bin\*"; DestDir: "{app}\Bin"; Flags: ignoreversion recursesubdirs
Source: "Files\MQL\*"; DestDir: "{code:GetDir|0}\MQL5\Experts\Advisors"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Backtest Generator"; Filename: "{app}\Bin\TestDataGenerator.exe"; WorkingDir: "{app}"
Name: "{commondesktop}\Backtest Generator"; Filename: "{app}\Bin\TestDataGenerator.exe"; WorkingDir: "{app}"

[Run]
Filename: "{app}\Bin\MarketDataProvider.exe"; Parameters: "uninstall"; WorkingDir: "{app}\Bin"; Flags: runhidden
Filename: "{app}\Bin\MarketDataProvider.exe"; Parameters: "install --manual"; WorkingDir: "{app}\Bin"; Flags: runhidden
Filename: "{app}\Bin\MarketDataProvider.exe"; Parameters: "start"; WorkingDir: "{app}\Bin"; Flags: runhidden

[UninstallRun]
Filename: "{app}\Bin\MarketDataProvider.exe"; Parameters: "uninstall"; WorkingDir: "{app}\Bin"; Flags: runhidden

[Code]
var
    DirPage: TInputDirWizardPage;

function GetDir(Param: String): String;
begin
    Result := DirPage.Values[StrToInt(Param)];
end;

procedure InitializeWizard;
begin
    DirPage := CreateInputDirPage(wpSelectDir, 'Select folders', 'Please select installation folders.', '', False, '');
    DirPage.Add('Please select MetaTrader4 data folder');
    DirPage.Values[0] := GetPreviousData('MT4DataDirectory', ExpandConstant('{userappdata}') + '\{#MT4DataFolderPath}');
end;

procedure RegisterPreviousData(PreviousDataKey: Integer);
begin
    SetPreviousData(PreviousDataKey, 'ProjectDirectory', DirPage.Values[0]);
end;
