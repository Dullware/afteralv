; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "AfterALV"
#define MyAppVerName "AfterALV 1.0b"
#define MyAppPublisher "Dullware Inc."
#define MyAppURL "http://www.dullware.nl"
#define MyAppExeName "AfterALV.exe"

[Setup]
AppName={#MyAppName}
AppID={{2780DD42-21C5-41E7-8D60-1172A5B801D4}
AppVerName={#MyAppVerName}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\Dullware AfterALV
DisableDirPage=true
DefaultGroupName=Dullware AfterALV
DisableProgramGroupPage=true
LicenseFile=
OutputBaseFilename={#MyAppVerName} Setup
Compression=lzma
SolidCompression=true
WizardImageFile=compiler:WIZMODERNIMAGE-IS.BMP
WizardSmallImageFile=compiler:WIZMODERNSMALLIMAGE-IS.BMP
AppMutex=DullwareAfterALVerawlluD
VersionInfoVersion=1.0.0.0
VersionInfoCompany=Dullware
VersionInfoDescription=AfterALV Installer
VersionInfoCopyright=Dullware
ChangesAssociations=true
ChangesEnvironment=True

[Languages]
Name: english; MessagesFile: compiler:Default.isl

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}
Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked
Name: aapassociation; Description: Associate .aap files with AfterALV; GroupDescription: File associations:

[Files]
Source: ..\AfterALV 1.0 Branch\bin\Release\AfterALV.exe; DestDir: {app}; Flags: ignoreversion
Source: ..\AfterALV 1.0 Branch\bin\Release\DullPlot.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\AfterALV 1.0 Branch\bin\Release\DullForm.dll; DestDir: {app}; Flags: ignoreversion
Source: ..\AfterALV 1.0 Branch\bin\Release\CarlosAg.ExcelXmlWriter.dll; DestDir: {app}; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: ..\AfterALV 1.0 Branch\bin\Release\Worksheet.dll; DestDir: {app}; Flags: ignoreversion
Source: \\THUISWATER\shared\src\dls\afteralv_RB_1_0\Continpk.exe; DestDir: {app}; Flags: ignoreversion
Source: \\THUISWATER\shared\src\dls\afteralv_RB_1_0\AfterALV.ico; DestDir: {app}; Flags: ignoreversion
Source: \\THUISWATER\shared\src\dls\afteralv_RB_1_0\AfterALV Project File.ico; DestDir: {app}; Flags: ignoreversion

[Icons]
Name: {group}\{#MyAppName}; Filename: {app}\{#MyAppExeName}
Name: {userdesktop}\{#MyAppName}; Filename: {app}\{#MyAppExeName}; Tasks: desktopicon
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}; Filename: {app}\{#MyAppExeName}; Tasks: quicklaunchicon

[Dirs]

[Components]

[Registry]
Root: HKCR; SubKey: .aap; ValueType: string; ValueData: AfterALV Project; Flags: uninsdeletekey; Tasks: aapassociation
Root: HKCR; SubKey: AfterALV Project; ValueType: string; ValueData: AfterALV Project File; Flags: uninsdeletekey; Tasks: aapassociation
Root: HKCR; SubKey: AfterALV Project\Shell\Open\Command; ValueType: string; ValueData: """{app}\AfterALV.exe"" ""%1"""; Flags: uninsdeletevalue; Tasks: aapassociation
Root: HKCR; Subkey: AfterALV Project\DefaultIcon; ValueType: string; ValueData: {app}\AfterALV Project File.ico,0; Flags: uninsdeletevalue; Tasks: aapassociation
[Code]
function DotNETInstalled(): Boolean;
var
    success: boolean;
    install: cardinal;
begin
    //success := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v1.1.4322', 'Install', install);
    success := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v2.0.50727', 'Install', install);
    Result := success and (install = 1);
end;

procedure SplitCmdLine(const CmdLine: String;
  var Filename, Params: String);
var
  InQuote: Boolean;
  I: Integer;
begin
  Filename := '';
  Params := '';
  InQuote := False;
  I := 1;
  // Slurp Filename characters
  while I <= Length(CmdLine) do begin
    if (CmdLine[I] <= ' ') and not InQuote then
      Break;
    if CmdLine[I] = '"' then
      InQuote := not InQuote
    else
      Filename := Filename + CmdLine[I];
    I := I + 1;
  end;
  // Skip past any whitespace between the filename and parameters
  while I <= Length(CmdLine) do begin
    if CmdLine[I] > ' ' then
      Break;
    I := I + 1;
  end;
  // Get the parameters
  Params := Copy(CmdLine, I, 2147483647);
end;

function InitializeSetup(): Boolean;
var
	sUninstallString, Filename, Params: String;
	iResultCode, iErrorCode, iResult: integer;
begin
	if not DotNETInstalled then
	begin
		MsgBox('The .NET Framework version 2.0 is not installed. Please install, and try again.', mbInformation, MB_OK );
		Result := false;
	end
	else if RegKeyExists(HKLM,'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{B09D820B-7A0D-4753-96B7-03DC339B2081}') then
	begin
		iResult := MsgBox('A previous version was detected.'#13
		+ 'This version is first uninstalled.'#13#13
		+ 'Agreed?', mbConfirmation, MB_YESNOCANCEL );
		Result:= iResult <> IDCANCEL;
		if iResult = IDYES then
		begin
			sUninstallString := '';
			if RegQueryStringValue( HKLM,
'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{B09D820B-7A0D-4753-96B7-03DC339B2081}','UninstallString',
sUninstallString) then
			begin
				SplitCmdLine(sUninstallString, Filename, Params);
				if not Exec(Filename, '/PASSIVE /X{B09D820B-7A0D-4753-96B7-03DC339B2081}', '', SW_SHOW, ewWaitUntilTerminated, iResultCode) then
				begin
					MsgBox( SysErrorMessage(iErrorCode),mbInformation, MB_OK);
					Result := false;
					exit;
				end
			end
		end
	end
	else Result := true;

	//MsgBox(ExpandConstant('AppID'),mbInformation,MB_OK);
end;
