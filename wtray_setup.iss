[Setup]
OutputBaseFilename=wtray_setup
VersionInfoVersion=1.0
VersionInfoCompany=Matteo Panella
VersionInfoCopyright=Copyright (c) 2010 Matteo Panella
VersionInfoProductName=WTray
VersionInfoProductVersion=1.0
MinVersion=6.1,6.1
AppCopyright=Copyright © 2010 Matteo Panella
AppName=WTray
AppVerName=WTray 1.0
AppMutex={{6F5F238E-09BE-4649-B29B-67D326187AE8}
AppPublisher=Matteo Panella
AppPublisherURL=https://github.com/rfc1459/wtray/
AppSupportURL=https://github.com/rfc1459/wtray/
AppVersion=1.0
AppID={{3FD903F3-43A0-4F74-9E6E-8092B6E5F7BC}
UninstallDisplayName=WTray
DefaultDirName={pf}\WTray
AllowNoIcons=true
DefaultGroupName=WTray
VersionInfoDescription=WTray Installer
LicenseFile=LICENSE.txt
AllowUNCPath=false
UninstallDisplayIcon={app}\wtray.exe
[Files]
Source: wtray\bin\Release\awmi.dll; DestDir: {app}
Source: wtray\bin\Release\wtray.exe; DestDir: {app}
Source: wtray\bin\Release\it-IT\wtray.resources.dll; DestDir: {app}\it-IT
Source: AUTHORS.txt; DestDir: {app}
Source: LICENSE.txt; DestDir: {app}
Source: LICENSE-icons.txt; DestDir: {app}
[Dirs]
Name: {app}\it-IT; Flags: deleteafterinstall
[Icons]
Name: {group}\WTray; Filename: {app}\wtray.exe; WorkingDir: {app}; IconFilename: {app}\wtray.exe; IconIndex: 0
[Languages]
Name: English; MessagesFile: compiler:Default.isl; LicenseFile: LICENSE.txt
Name: Italiano; MessagesFile: compiler:Languages\Italian.isl; LicenseFile: LICENSE.txt
