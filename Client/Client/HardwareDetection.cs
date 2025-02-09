using OllamaInstaller;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Vanara.PInvoke;
using static Vanara.PInvoke.SetupAPI;

namespace HardwareDetection
{
    public static class DisplayAdapterDetector
    {

        private static readonly Guid GUID_DEVCLASS_DISPLAY = new Guid("4d36e968-e325-11ce-bfc1-08002be10318");

        public static bool DetectHardware()
        {
            bool hasGrpahicCard = false;
            ConsoleHelper.WriteYellow("GPUs:");
            foreach (var (deviceName, driverProvider) in DisplayAdapterDetector.GetDisplayAdapterDriverInfo())
            {
                hasGrpahicCard = true;
                ConsoleHelper.WriteGreen($"- Device: {deviceName}");
                ConsoleHelper.WriteGreen($"  Manufacturer: {driverProvider}");
            }
            return hasGrpahicCard;
        }

        public static IEnumerable<(string DeviceName, string DriverProvider)> GetDisplayAdapterDriverInfo()
        {
            // Get device info set handle
            using var hDevInfo = SetupAPI.SetupDiGetClassDevs(GUID_DEVCLASS_DISPLAY, null, IntPtr.Zero, SetupAPI.DIGCF.DIGCF_PRESENT);
            if (hDevInfo.IsInvalid)
                yield break;

            uint index = 0;
            var devInfoData = new SetupAPI.SP_DEVINFO_DATA();
            devInfoData.cbSize = (uint)Marshal.SizeOf(typeof(SetupAPI.SP_DEVINFO_DATA));

            // Enumerate devices
            while (SetupAPI.SetupDiEnumDeviceInfo(hDevInfo, index, ref devInfoData))
            {
                string deviceName = GetDevicePropertyString(hDevInfo, ref devInfoData, SetupAPI.SPDRP.SPDRP_DEVICEDESC);
                string driverProvider = GetDevicePropertyString(hDevInfo, ref devInfoData, SetupAPI.SPDRP.SPDRP_MFG);

                yield return (deviceName, driverProvider);
                index++;
            }
        }

        private static string GetDevicePropertyString(SafeHDEVINFO hDevInfo, ref SetupAPI.SP_DEVINFO_DATA devInfoData, SetupAPI.SPDRP property)
        {
            uint requiredSize = 0;
            Vanara.PInvoke.REG_VALUE_TYPE regType;

            // Allocate unmanaged memory for the property data (1024 bytes should be sufficient for most strings)
            IntPtr buffer = Marshal.AllocHGlobal(1024);

            try
            {
                bool success = SetupAPI.SetupDiGetDeviceRegistryProperty(
                    hDevInfo,
                    ref devInfoData,
                    property,
                    out regType,
                    buffer,
                    1024,
                    out requiredSize);

                if (success)
                {
                    // Convert unmanaged memory to a managed string
                    string result = Marshal.PtrToStringUni(buffer, (int)(requiredSize / 2)); // Convert to Unicode string
                    return result?.TrimEnd('\0') ?? string.Empty;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer); // Free allocated memory
            }

            return string.Empty;
        }
    }
}
