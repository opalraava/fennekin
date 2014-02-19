; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
AppName=WebDiver
AppVerName=Webdiver 0.2.10
AppPublisher=Bert van der Weerd
AppPublisherURL=http://code.google.com/p/iondrive/
AppSupportURL=http://code.google.com/p/iondrive/
AppUpdatesURL=http://code.google.com/p/iondrive/
DefaultDirName={pf}\WebDiver
DefaultGroupName=WebDiver
AllowNoIcons=yes
OutputDir=output
OutputBaseFilename=WebDiver-Setup-0.2.10
SetupIconFile=dist\BsodApp.ico
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "dist\WebDiver.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "dist\Universal.xml"; DestDir: "{userdocs}\My WebDiver Files"; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall confirmoverwrite
Source: "dist\Languages.xml"; DestDir: "{userdocs}\My WebDiver Files"; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall confirmoverwrite
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[INI]
Filename: "{app}\WebDiver.url"; Section: "InternetShortcut"; Key: "URL"; String: "http://code.google.com/p/iondrive/"

[Icons]
Name: "{group}\WebDiver"; Filename: "{app}\WebDiver.exe"
Name: "{group}\Examples\Universal topics"; Filename: "{app}\WebDiver.exe"; Parameters: """{userdocs}\My WebDiver Files\Universal.xml"""
Name: "{group}\Examples\Many languages example"; Filename: "{app}\WebDiver.exe"; Parameters: """{userdocs}\My WebDiver Files\Languages.xml"""
Name: "{group}\{cm:ProgramOnTheWeb,WebDiver}"; Filename: "{app}\WebDiver.url"
Name: "{group}\{cm:UninstallProgram,WebDiver}"; Filename: "{uninstallexe}"
Name: "{userdesktop}\WebDiver"; Filename: "{app}\WebDiver.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\WebDiver.exe"; Description: "{cm:LaunchProgram,WebDiver}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\WebDiver.url"

