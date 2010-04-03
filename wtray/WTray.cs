// WTray.cs - EeePC hardware management tray application
// Copyright (C) 2010 Matteo Panella <morpheus@level28.org>
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using awmi;

namespace wtray
{
    public class WTray : Form
    {
        private class DevEntry
        {
            public ToolStripMenuItem MenuItem { get; set; }

            private DeviceStatus status;
            public DeviceStatus Status
            {
                get
                {
                    return status;
                }
                set
                {
                    switch (value)
                    {
                        case DeviceStatus.Unknown:
                            MenuItem.Enabled = false;
                            MenuItem.Visible = false;
                            break;
                        case DeviceStatus.PowerOff:
                            MenuItem.Visible = true;
                            MenuItem.Checked = false;
                            break;
                        case DeviceStatus.PowerOn:
                            MenuItem.Visible = true;
                            MenuItem.Checked = true;
                            break;
                    }
                }
            }

            public DevEntry(string menuLabel, EventHandler evt)
            {
                MenuItem = new ToolStripMenuItem(menuLabel, null, evt);
                MenuItem.Visible = false;
                status = DeviceStatus.Unknown;
            }
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Check for Windows 7
            Version osVer = System.Environment.OSVersion.Version;
            if (osVer.Major < 6 || (osVer.Major == 6 && osVer.Minor < 1))
            {
                MessageBox.Show(Strings.WinSevenRequired, Strings.WTrayName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check for other instances
            string mutexGUID = "{6F5F238E-09BE-4649-B29B-67D326187AE8}";
            bool isSingleInstance;
            Mutex instanceMutex = new Mutex(true, mutexGUID, out isSingleInstance);
            if (!isSingleInstance)
            {
                MessageBox.Show(Strings.SingleInstanceOnly, Strings.WTrayName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // All clear, application
            Application.Run(new WTray());

            // Prevent garbage collection of application mutex
            GC.KeepAlive(instanceMutex);
        }

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenuStrip;

        private DevEntry Camera;
        private DevEntry CardReader;

        private AwmiInterface wmiObj;

        private System.Timers.Timer hwStatusRefreshTimer;

        public WTray()
        {
            try
            {
                wmiObj = new AwmiInterface();
            }
            catch (AwmiException)
            {
                MessageBox.Show(Strings.FatalAwmiError, Strings.WTrayName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            Camera = new DevEntry(Strings.Webcam, OnCameraToggle);
            CardReader = new DevEntry(Strings.CardReader, OnCardReaderToggle);

            trayMenuStrip = new ContextMenuStrip();
            ToolStripLabel lblDevices = new ToolStripLabel(Strings.Devices);
            lblDevices.Enabled = false;
            ToolStripMenuItem mnuAbout = new ToolStripMenuItem(Strings.About, null, OnAbout);
            ToolStripMenuItem mnuExit = new ToolStripMenuItem(Strings.Exit, null, OnExit);

            trayMenuStrip.Items.Add(lblDevices);
            trayMenuStrip.Items.Add(Camera.MenuItem);
            trayMenuStrip.Items.Add(CardReader.MenuItem);
            trayMenuStrip.Items.Add(new ToolStripSeparator());
            trayMenuStrip.Items.Add(mnuAbout);
            trayMenuStrip.Items.Add(mnuExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = Strings.WTrayName;
            trayIcon.Icon = new Icon(Icons.WTray, 40, 40);

            trayIcon.ContextMenuStrip = trayMenuStrip;
            trayIcon.Visible = true;
            RefreshDeviceStatus();
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            hwStatusRefreshTimer = new System.Timers.Timer(10000);
            hwStatusRefreshTimer.Enabled = false;
            hwStatusRefreshTimer.SynchronizingObject = this;
            hwStatusRefreshTimer.AutoReset = true;
            hwStatusRefreshTimer.Elapsed += new System.Timers.ElapsedEventHandler(hwStatusRefreshTimer_Elapsed);
            hwStatusRefreshTimer.Enabled = true;

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnAbout(object sender, EventArgs e)
        {
            WtrayAbout about = new WtrayAbout();
            about.Show();
        }

        private void OnCameraToggle(object sender, EventArgs e)
        {
            try
            {
                Camera.Status = wmiObj.ToggleWebcam();
            }
            catch (AwmiException)
            {
                MessageBox.Show(Strings.CameraError, Strings.WTrayName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnCardReaderToggle(object sender, EventArgs e)
        {
            try
            {
                CardReader.Status = wmiObj.ToggleCardReader();
            }
            catch (AwmiException)
            {
                MessageBox.Show(Strings.CardReaderError, Strings.WTrayName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void hwStatusRefreshTimer_Elapsed(object sender, EventArgs e)
        {
            RefreshDeviceStatus();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                hwStatusRefreshTimer.Dispose();
                trayIcon.Dispose();
            }

            base.Dispose(disposing);
        }

        private void RefreshDeviceStatus()
        {
            Camera.Status = wmiObj.GetDeviceStatus(DeviceIdentifier.Camera);
            CardReader.Status = wmiObj.GetDeviceStatus(DeviceIdentifier.CardReader);
        }
    }
}
