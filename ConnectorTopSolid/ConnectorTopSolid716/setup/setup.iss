; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

;To be manually set
#define AppPublisher "TOPSOLID"
#define WebSite "https://speckle.systems/"
#define Project "Speckle"
#define AppName "ConnectorTopSolid"
#define Guid "{79A9EAFE-A578-4C98-8871-0D743CF83FDA}"
#define SrcFolder "C:\Sources\Topsolid 7.16\Release x64"
#define IconPath "\ContextMenu.Item.ico"
#define ServerRes "res://Services"
#define UpdateType "Services"

;Automatically computed
#define AppVersion GetStringFileInfo(SrcFolder+"\"+"Speckle.ConnectorTopSolid.AddIn.dll","ProductVersion")
#define TopSolidVersion RemoveFileExt(RemoveFileExt(AppVersion))
#define AppVerName AppName+" v"+AppVersion
#define AppVerName_ StringChange(AppVerName," ", "_");

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{#Guid}
AppName={#Project} {#AppVerName}
AppVersion={#AppVersion}
AppVerName={#Project} {#AppVerName}
AppPublisher={#AppPublisher}
AppPublisherURL={#WebSite}
AppSupportURL={#WebSite}
AppUpdatesURL={#WebSite}
CreateAppDir=no
OutputDir=../../../../Speckle.TopSolid/Binaries
OutputBaseFilename={#Project}_{#AppVerName_}_Setup
SetupIconFile={#SrcFolder}{#IconPath}
UninstallDisplayIcon={code:GetTopDir}\bin\topsolid.exe
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Files]

Source: "{#SrcFolder}\Speckle.ConnectorTopSolid.AddIn.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\Speckle.ConnectorTopSolid.AddIn.dll.config"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\Speckle.ConnectorTopSolid.AddIn.pdb"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Speckle.ConnectorTopSolid.UI.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\Speckle.ConnectorTopSolid.UI.dll.config"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\Speckle.ConnectorTopSolid.UI.pdb"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Speckle.ConnectorTopSolid.DB.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\Speckle.ConnectorTopSolid.DB.pdb"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\fr\Speckle.ConnectorTopSolid.AddIn.resources.dll"; DestDir: "{code:GetTopDir}\bin\fr"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\fr\Speckle.ConnectorTopSolid.UI.resources.dll"; DestDir: "{code:GetTopDir}\bin\fr"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;

Source: "{#SrcFolder}\Avalonia.Animation.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Base.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Controls.DataGrid.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Controls.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.DesignerSupport.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Desktop.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.DesktopRuntime.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Diagnostics.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Dialogs.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.FreeDesktop.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Input.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Interactivity.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Layout.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Markup.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Markup.Xaml.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.MicroCom.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Native.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.OpenGL.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.ReactiveUI.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Remote.Protocol.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Skia.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Styling.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Themes.Default.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Themes.Fluent.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Visuals.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.Win32.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Avalonia.X11.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\DesktopUI2.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\DesktopUI2.pdb"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\DynamicData.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\GraphQL.Client.Abstractions.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\GraphQL.Client.Abstractions.Websocket.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\GraphQL.Client.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\GraphQL.Primitives.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\HarfBuzzSharp.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\HarfBuzzSharp.pdb"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\JetBrains.Annotations.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\libHarfBuzzSharp.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\libHarfBuzzSharp.dylib"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\libSkiaSharp.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\libSkiaSharp.dylib"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Material.Avalonia.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Material.Colors.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Material.DataGrid.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Material.Dialog.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Material.Icons.Avalonia.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Material.Icons.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Material.Ripple.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Material.Styles.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Microsoft.Bcl.AsyncInterfaces.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Microsoft.CodeAnalysis.CSharp.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\Microsoft.CodeAnalysis.CSharp.pdb"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Microsoft.CodeAnalysis.CSharp.Scripting.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\Microsoft.CodeAnalysis.CSharp.Scripting.pdb"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Microsoft.CodeAnalysis.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\Microsoft.CodeAnalysis.pdb"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Microsoft.CodeAnalysis.Scripting.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\Microsoft.CodeAnalysis.Scripting.pdb"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Newtonsoft.Json.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\ReactiveUI.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Sentry.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\SkiaSharp.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\SkiaSharp.pdb"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Speckle.Newtonsoft.Json.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\Speckle.Newtonsoft.Json.pdb"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\SpeckleCore2.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
; Source: "{#SrcFolder}\SpeckleCore2.pdb"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\SpeckleCore2.xml"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Splat.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Stylet.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Buffers.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Collections.Immutable.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.ComponentModel.Annotations.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Data.SQLite.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Drawing.Common.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Memory.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Net.WebSockets.Client.Managed.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Numerics.Vectors.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Reactive.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Reflection.Metadata.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Security.Principal.Windows.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Text.Encoding.CodePages.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Text.Encodings.Web.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Text.Json.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.Threading.Tasks.Extensions.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\System.ValueTuple.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Tmds.DBus.dll"; DestDir: "{code:GetTopDir}\bin"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;

Source: "{#SrcFolder}\musl-x64\libHarfBuzzSharp.so"; DestDir: "{code:GetTopDir}\bin\musl-x64"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\musl-x64\libSkiaSharp.so"; DestDir: "{code:GetTopDir}\bin\musl-x64"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\Native\libAvalonia.Native.OSX.dylib"; DestDir: "{code:GetTopDir}\bin\Native"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\x64\libHarfBuzzSharp.dll"; DestDir: "{code:GetTopDir}\bin\x64"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\x64\libHarfBuzzSharp.so"; DestDir: "{code:GetTopDir}\bin\x64"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\x64\libSkiaSharp.dll"; DestDir: "{code:GetTopDir}\bin\x64"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\x64\libSkiaSharp.so"; DestDir: "{code:GetTopDir}\bin\x64"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;
Source: "{#SrcFolder}\x64\SQLite.Interop.dll"; DestDir: "{code:GetTopDir}\bin\x64"; Flags: ignoreversion; Check: IsTopSolidDesignInstalled;



[Registry]

;Application regs
Root: HKLM64; Subkey: "SOFTWARE\{#AppPublisher}\{#AppName}"; Flags: uninsdeletekeyifempty;
Root: HKLM64; Subkey: "SOFTWARE\{#AppPublisher}\{#AppName}\{#TopSolidVersion}"; ValueType: string; ValueName: "INSTALLDIR"; ValueData: "{code:GetTopDir}";
Root: HKLM64; Subkey: "SOFTWARE\{#AppPublisher}\{#AppName}\{#TopSolidVersion}"; ValueType: string; ValueName: "Version"; ValueData: "{#AppVersion}";

;Custom Updates regs
Root: HKLM64; Subkey: "SOFTWARE\{#AppPublisher}\TopSolid'Update\CustomUpdates"; Flags: uninsdeletekeyifempty;
Root: HKLM64; Subkey: "SOFTWARE\{#AppPublisher}\TopSolid'Update\CustomUpdates\{#ServerRes}"; Flags: uninsdeletekeyifempty;
Root: HKLM64; Subkey: "SOFTWARE\{#AppPublisher}\TopSolid'Update\CustomUpdates\{#ServerRes}\{#TopSolidVersion}\x64\{#AppName}"; ValueType: string; ValueName: "CommandLineArguments"; ValueData: "/VERYSILENT"
Root: HKLM64; Subkey: "SOFTWARE\{#AppPublisher}\TopSolid'Update\CustomUpdates\{#ServerRes}\{#TopSolidVersion}\x64\{#AppName}"; ValueType: string; ValueName: "Executable"; ValueData: "TopSolid.exe"
Root: HKLM64; Subkey: "SOFTWARE\{#AppPublisher}\TopSolid'Update\CustomUpdates\{#ServerRes}\{#TopSolidVersion}\x64\{#AppName}"; ValueType: string; ValueName: "UpdateType"; ValueData: "{#UpdateType}"

[Code]
var
  TopDir: String;
  UpdateDir : String;
  TopSolidDesignInstalled : Boolean;

function FuncTopDir(dummy: String): String;
begin
  RegQueryStringValue(HKLM64, 'Software\{#AppPublisher}\TopSolid''Design\7.16', 'INSTALLDIR', TopDir);
  RegQueryStringValue(HKLM64, 'Software\{#AppPublisher}\TopSolid''Update', 'INSTALLDIR', UpdateDir);
  TopSolidDesignInstalled := DirExists(TopDir);
  Result := '';
end;

function GetTopDir(dummy: String): String;
begin
  Result := TopDir;
end;

function GetUpdateDir(dummy: String): String;
begin
  Result := UpdateDir;
end;

function IsTopSolidDesignInstalled(): Boolean;
begin
  Result := TopSolidDesignInstalled;
end;

// EVENT : Setup initialization.
function InitializeSetup(): Boolean;
begin
    // Check TopSolid main path
    FuncTopDir('');
    if DirExists(UpdateDir) or DirExists(TopDir) = True then begin
      Result := True;
    end else begin
      MsgBox('TopSolid directory is not found', mbInformation, MB_OK);
      Result := False;
    end;
end;

[Setup]
;WizardImageFile=Setup.bmp
;WizardSmallImageFile=Program.bmp