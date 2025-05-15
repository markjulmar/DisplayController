using System.Runtime.InteropServices;
using DisplayController.Models;

namespace DisplayController.Services
{
    /// <summary>
    /// Provides services for monitor display configuration using Windows API
    /// </summary>
    public class DisplayService
    {
        #region P/Invoke structures and functions

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [Flags]
        public enum DisplayDeviceStateFlags : int
        {
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            PrimaryDevice = 0x4,
            MirroringDriver = 0x8,
            VGACompatible = 0x10,
            Removable = 0x20,
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        public enum DISP_CHANGE : int
        {
            Successful = 0,
            Restart = 1,
            Failed = -1,
            BadMode = -2,
            NotUpdated = -3,
            BadFlags = -4,
            BadParam = -5,
            BadDualView = -6
        }

        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int ENUM_REGISTRY_SETTINGS = -2;
        public const int DM_PELSWIDTH = 0x80000;
        public const int DM_PELSHEIGHT = 0x100000;
        public const int DM_DISPLAYORIENTATION = 0x80;
        public const int DM_POSITION = 0x20;
        public const int DM_DISPLAYFIXEDOUTPUT = 0x200000;
        public const int CDS_UPDATEREGISTRY = 0x01;
        public const int CDS_TEST = 0x02;
        public const int CDS_FULLSCREEN = 0x04;
        public const int CDS_GLOBAL = 0x08;
        public const int CDS_SET_PRIMARY = 0x10;
        public const int CDS_NORESET = 0x10000000;

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        [DllImport("user32.dll")]
        public static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, uint dwflags, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, IntPtr lpDevMode, IntPtr hwnd, uint dwflags, IntPtr lParam);

        #endregion

        /// <summary>
        /// Gets the current configuration of all connected monitors
        /// </summary>
        public MonitorConfigCollection GetCurrentConfig()
        {
            var config = new MonitorConfigCollection();
            var device = new DISPLAY_DEVICE();
            var deviceMode = new DEVMODE();

            device.cb = Marshal.SizeOf(device);
            uint deviceIndex = 0;
            while (EnumDisplayDevices(null, deviceIndex, ref device, 0))
            {
                deviceIndex++;

                // Only include devices that are attached to the desktop
                if ((device.StateFlags & (int)DisplayDeviceStateFlags.AttachedToDesktop) == 0)
                    continue;

                deviceMode.dmSize = (short)Marshal.SizeOf(deviceMode);
                if (!EnumDisplaySettings(device.DeviceName, ENUM_CURRENT_SETTINGS, ref deviceMode))
                    continue;

                var monitorConfig = new MonitorConfig
                {
                    DeviceName = device.DeviceName,
                    ID = device.DeviceID,
                    IsPrimary = (device.StateFlags & (int)DisplayDeviceStateFlags.PrimaryDevice) != 0,
                    Width = deviceMode.dmPelsWidth,
                    Height = deviceMode.dmPelsHeight,
                    Orientation = (DisplayOrientation)deviceMode.dmDisplayOrientation,
                    X = deviceMode.dmPositionX,
                    Y = deviceMode.dmPositionY
                };

                config.Monitors.Add(monitorConfig);
            }

            return config;
        }

        /// <summary>
        /// Applies a monitor configuration
        /// </summary>
        public bool ApplyConfig(MonitorConfigCollection config)
        {
            return config.Monitors.Aggregate(true, (current, monitor) => current & ApplyMonitorConfig(monitor));
        }

        /// <summary>
        /// Applies a single monitor configuration
        /// </summary>
        private bool ApplyMonitorConfig(MonitorConfig config)
        {
            var deviceMode = new DEVMODE();
            deviceMode.dmSize = (short)Marshal.SizeOf(deviceMode);

            if (!EnumDisplaySettings(config.DeviceName, ENUM_CURRENT_SETTINGS, ref deviceMode))
                return false;

            deviceMode.dmPelsWidth = config.Width;
            deviceMode.dmPelsHeight = config.Height;
            deviceMode.dmPositionX = config.X;
            deviceMode.dmPositionY = config.Y;
            deviceMode.dmDisplayOrientation = (int)config.Orientation;

            deviceMode.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_POSITION | DM_DISPLAYORIENTATION;

            uint flags = CDS_UPDATEREGISTRY;
            if (config.IsPrimary)
                flags |= CDS_SET_PRIMARY;

            var result = ChangeDisplaySettingsEx(config.DeviceName, ref deviceMode, IntPtr.Zero, flags, IntPtr.Zero);
            return result == DISP_CHANGE.Successful || result == DISP_CHANGE.Restart;
        }

        /// <summary>
        /// Sets the primary monitor by its device ID
        /// </summary>
        public bool SetPrimaryMonitor(string id)
        {
            var config = GetCurrentConfig();
            var monitor = config.Monitors.Find(m => m.ID == id);

            if (monitor == null)
                return false;

            // Set this monitor as primary
            monitor.IsPrimary = true;

            // Set all other monitors as non-primary
            foreach (var otherMonitor in config.Monitors.Where(otherMonitor => otherMonitor.ID != id))
            {
                otherMonitor.IsPrimary = false;
            }

            return ApplyConfig(config);
        }

        /// <summary>
        /// Changes the resolution of a monitor by its device ID
        /// </summary>
        public bool ChangeResolution(string id, int width, int height)
        {
            var config = GetCurrentConfig();
            var monitor = config.Monitors.Find(m => m.ID == id);

            if (monitor == null)
                return false;

            monitor.Width = width;
            monitor.Height = height;

            return ApplyMonitorConfig(monitor);
        }

        /// <summary>
        /// Configures display mode (extended or mirrored)
        /// </summary>
        public bool SetDisplayMode(bool mirrored)
        {
            var config = GetCurrentConfig();

            if (mirrored)
            {
                // For mirrored mode, set all monitors to the same position
                var primary = config.Monitors.Find(m => m.IsPrimary);
                if (primary == null && config.Monitors.Count > 0)
                    primary = config.Monitors[0];

                // Only proceed if we found a primary monitor
                if (primary != null)
                {
                    foreach (var monitor in config.Monitors.Where(monitor => monitor != primary))
                    {
                        monitor.X = primary.X;
                        monitor.Y = primary.Y;
                    }
                }
            }
            else
            {
                // For extended mode, we'd need to position monitors side by side
                // This is a simple implementation; real-world usage might need more complex positioning
                int currentX = 0;

                foreach (var monitor in config.Monitors)
                {
                    monitor.X = currentX;
                    monitor.Y = 0;
                    currentX += monitor.Width;
                }
            }

            return ApplyConfig(config);
        }
    }
}