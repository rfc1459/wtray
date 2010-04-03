using System;
using System.Management;

namespace awmi
{
    /// <summary>
    /// Base exception class for awmi.
    /// </summary>
    [global::System.Serializable]
    public class AwmiException : Exception
    {
        public AwmiException() { }
        public AwmiException(string message) : base(message) { }
        public AwmiException(string message, Exception inner) : base(message, inner) { }
        protected AwmiException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// Identifiers for embedded EeePC devices.
    /// </summary>
    /// <remarks>
    /// Identifiers are known to be valid for at least the 1000H model.
    /// If you have another model, please dump its DSDT and send it to the author for
    /// further inspection.
    /// </remarks>
    public static class DeviceIdentifier
    {
        private const uint wlan_id = 0x00010011;
        private const uint bt_id = 0x00010013;
        private const uint cam_id = 0x00060013;
        private const uint cr_id = 0x00080013;

        public static uint Wireless
        {
            get
            {
                return wlan_id;
            }
        }
        public static uint Bluetooth
        {
            get
            {
                return bt_id;
            }
        }
        public static uint Camera
        {
            get
            {
                return cam_id;
            }
        }
        public static uint CardReader
        {
            get
            {
                return cr_id;
            }
        }

        public static string DeviceName(uint device_id)
        {
            switch (device_id)
            {
                case wlan_id:
                    return "Wireless LAN";
                case bt_id:
                    return "Bluetooth HCI";
                case cam_id:
                    return "Camera";
                case cr_id:
                    return "Card Reader";
                default:
                    throw new AwmiException("Invalid device_id");
            }
        }
    }

    /// <summary>
    /// Power status codes.
    /// </summary>
    public enum DeviceStatus : short
    {
        Unknown = -1,
        PowerOff,
        PowerOn
    }

    /// <summary>
    /// Public interface to EeePC ACPI-WMI object.
    /// </summary>
    public class AwmiInterface
    {
        private ManagementClass asusCls;
        private ManagementObject asusWMI;

        private const uint DEV_PRESENT_MASK = 0x00030000;

        /// <summary>
        /// Connect to WMI service and get an ASUSManagement instance.
        /// </summary>
        /// <remarks>Requires administrative privileges.</remarks>
        public AwmiInterface()
        {
            try
            {
                // Connect with full privileges
                ConnectionOptions opts = new ConnectionOptions();
                opts.Impersonation = ImpersonationLevel.Impersonate;
                opts.EnablePrivileges = true;
                opts.Authentication = AuthenticationLevel.Default;

                ManagementScope wmiScope = new ManagementScope(@"\\.\root\wmi", opts);
                wmiScope.Connect();

                // Get ASUSManagement class handle
                ManagementPath p = new ManagementPath("ASUSManagement");
                asusCls = new ManagementClass(wmiScope, p, null);
                asusCls.Options.UseAmendedQualifiers = true;

                // Retrieve ASUSManagement instance (crude hack, but hey...)
                ManagementObjectCollection moc = asusCls.GetInstances();
                if (moc.Count > 1)
                {
                    throw new AwmiException("Too many ASUSManagement object instances!");
                }
                foreach (ManagementObject obj in moc)
                {
                    asusWMI = obj;
                    break;
                }
            }
            catch (AwmiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AwmiException("Connection to ASUSManagement object failed", e);
            }
        }

        /// <summary>
        /// Get power status for a given device.
        /// </summary>
        /// <param name="device_id">Device identifier</param>
        /// <returns>Power status for given device or Unknown if device is not present</returns>
        /// <seealso cref="DeviceStatus"/>
        public DeviceStatus GetDeviceStatus(uint device_id)
        {
            try
            {
                // Build input params
                string meth = "device_status";
                ManagementBaseObject inParams = asusCls.GetMethodParameters(meth);
                inParams["device_id"] = device_id;

                // Call object
                ManagementBaseObject outParams = asusWMI.InvokeMethod(meth, inParams, null);

                // Get result
                uint ctrl_param = (uint)outParams["ctrl_param"];
                if ((ctrl_param & DEV_PRESENT_MASK) == 0)
                    return DeviceStatus.Unknown;
                else
                    return ((ctrl_param & 1) == 1 ? DeviceStatus.PowerOn : DeviceStatus.PowerOff);
            }
            catch (Exception e)
            {
                throw new AwmiException("GetDeviceStatus failure", e);
            }
        }

        /// <summary>
        /// Toggle power status of embedded camera.
        /// </summary>
        public DeviceStatus ToggleWebcam()
        {
            return ToggleDevice(DeviceIdentifier.Camera);
        }

        /// <summary>
        /// Toggle power status of embedded card reader.
        /// </summary>
        public DeviceStatus ToggleCardReader()
        {
            return ToggleDevice(DeviceIdentifier.CardReader);
        }

        private DeviceStatus ToggleDevice(uint devid)
        {
            try
            {
                DeviceStatus wcamst = GetDeviceStatus(devid);
                if (wcamst == DeviceStatus.Unknown)
                    throw new AwmiException("System has no " + DeviceIdentifier.DeviceName(devid));

                if (wcamst == DeviceStatus.PowerOff)
                {
                    SetDeviceStatus(devid, true);
                    return DeviceStatus.PowerOn;
                }
                else
                {
                    SetDeviceStatus(devid, false);
                    return DeviceStatus.PowerOff;
                }
            }
            catch (AwmiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AwmiException("ToggleWebcam failure", e);
            }
        }

        private void SetDeviceStatus(uint device_id, bool power_status)
        {
            try
            {
                // Same mumbo-jumbo: get parameters, bind them and invoke the method
                string meth = "device_ctrl";
                ManagementBaseObject inParams = asusCls.GetMethodParameters(meth);

                inParams["device_id"] = device_id;
                inParams["ctrl_param"] = power_status ? 1 : 0;

                asusWMI.InvokeMethod(meth, inParams, null);
            }
            catch (AwmiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new AwmiException("SetDeviceStatus failure", e);
            }
        }
    }
}
