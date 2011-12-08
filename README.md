WTray
=====

WTray lets EeePC owners manage the power status of some internal devices
in Windows 7 without the need to reboot the machine and go through the
BIOS setup screens.


Rationale
---------

With the adoption of [ACPI-WMI][], the old Asus tools to manage internal
EeePC devices like the camera and the card reader don't work anymore even
when running under XP Compatibility Mode.

This program partially addresses the problem by performing the necessary
calls to the ACPI through WMI, enabling the user to selectively toggle power
status of each recognized device.

### Supported Models

Due to limited hardware availability, only the EeePC 1000H is officially
supported. Due to the way the ACPI-WMI agent works, however, similar models
may as well be supported without user intervention.

If you are interested in contributing support for a newer model you should be
prepared to dump and disassemble the DSDT table of your EeePC model and look
for the ASUSManagement ACPI-WMI instance (there should be only one). If this
doesn't make any sense to you just [file a new issue][issue] stating your
EeePC model.

### Supported Devices

Although the ACPI-WMI component of WTray is capable of managing power state
for all devices (wlan, bluetooth, card reader, camera), the ASUS hotkey
service doesn't really behave well when other programs use ACPI-WMI to change
power status for wireless devices, so support is restricted to card reader and
camera only.

This won't likely be fixed unless ASUS is willing to document the way their
hotkey service can be notified of a power status change by an external
application.


Contributing
------------

Just fork, create a topic branch, hack away and submit a pull request :-)

Please please please use _only_ the "Express" SKU of Visual C# 2010 to edit
the project.


Installation
------------

If you just want to install the binary, head over to the [downloads
page][downloads]
and pick the latest binary installer.

If you want to compile the program for yourself, clone this repository, open
`awmi.sln` in Visual C# 2010 Express and hit F6.


Caveats
-------

WTray requires administrative privileges in order to access ACPI-WMI. If you
want to start it on logon, you need to either disable UAC (not recommended) or
create a privileged scheduled task. See the [wiki][] for more information.


License
-------

The program itself is licensed under the GNU General Public License version 2,
the icons are copyrighted by [Leerhuelle](http://leerhuelle.deviantart.com/)
and licensed under [Creative Commons Attribution-Noncommercial-Share Alike 3.0
License](http://creativecommons.org/licenses/by-nc-sa/3.0/).


[ACPI-WMI]: http://msdn.microsoft.com/en-us/windows/hardware/gg463463
[issue]: https://github.com/rfc1459/wtray/issues/new
[downloads]: https://github.com/rfc1459/wtray/downloads
