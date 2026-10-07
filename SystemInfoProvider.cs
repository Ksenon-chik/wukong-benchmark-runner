using System;
using System.Management;

namespace WukongBenchmarkRunner
{
    public record HardwareSpecs(
        string CpuName,
        int CpuCores,
        int CpuThreads,
        string GpuName,
        double GpuVramGb,
        double TotalRamGb,
        string OsVersion
    );

    public static class SystemInfoProvider
    {
        public static HardwareSpecs GetSpecs()
        {
            string cpu = GetWmiProperty("Win32_Processor", "Name");
            int cores = Convert.ToInt32(GetWmiProperty("Win32_Processor", "NumberOfCores") ?? "0");
            int threads = Convert.ToInt32(GetWmiProperty("Win32_Processor", "NumberOfLogicalProcessors") ?? "0");

            string gpu = GetWmiProperty("Win32_VideoController", "Name");
            ulong vramBytes = Convert.ToUInt64(GetWmiProperty("Win32_VideoController", "AdapterRAM") ?? "0");

            ulong ramBytes = Convert.ToUInt64(GetWmiProperty("Win32_ComputerSystem", "TotalPhysicalMemory") ?? "0");
            string os = GetWmiProperty("Win32_OperatingSystem", "Caption");

            return new HardwareSpecs(
                CpuName: cpu.Trim(),
                CpuCores: cores,
                CpuThreads: threads,
                GpuName: gpu.Trim(),
                GpuVramGb: Math.Round(vramBytes / (1024.0 * 1024 * 1024), 2),
                TotalRamGb: Math.Round(ramBytes / (1024.0 * 1024 * 1024), 2),
                OsVersion: os.Trim()
            );
        }

        private static string GetWmiProperty(string wmiClass, string propertyName)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {wmiClass}");
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj[propertyName] != null)
                        return obj[propertyName].ToString()!;
                }
            }
            catch
            {
                // Игнорируем ошибки доступа WMI
            }
            return "Unknown";
        }
    }
}