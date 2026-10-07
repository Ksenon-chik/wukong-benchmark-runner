using System;
using System.IO;

namespace WukongBenchmarkRunner
{
    public enum PresetType { CpuBound, GpuBound }

    public static class BenchmarkConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"b1\Saved\Config\Windows\GameUserSettings.ini"
        );

        public static void ApplyPreset(PresetType preset)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);

            string content = preset switch
            {
                PresetType.CpuBound => GetCpuBoundIniContent(),
                PresetType.GpuBound => GetGpuBoundIniContent(),
                _ => throw new ArgumentOutOfRangeException(nameof(preset))
            };

            File.WriteAllText(ConfigPath, content);
        }

        private static string GetCpuBoundIniContent() => @"
[/Script/Engine.GameUserSettings]
ResolutionSizeX=1280
ResolutionSizeY=720
LastUserConfirmedResolutionSizeX=1280
LastUserConfirmedResolutionSizeY=720
FrameRateLimit=0.000000
bUseVSync=False

[ScalabilityGroups]
sg.ResolutionQuality=50.000000
sg.ViewDistanceQuality=0
sg.AntiAliasingQuality=0
sg.ShadowQuality=0
sg.PostProcessQuality=0
sg.TextureQuality=0
sg.EffectsQuality=0
sg.FoliageQuality=0
sg.ShadingQuality=0
sg.GlobalIlluminationQuality=0
sg.ReflectionQuality=0

[/Script/b1.B1GameUserSettings]
bRayTracingEnable=False
bFrameGenEnable=False
";

        private static string GetGpuBoundIniContent() => @"
[/Script/Engine.GameUserSettings]
ResolutionSizeX=3840
ResolutionSizeY=2160
LastUserConfirmedResolutionSizeX=3840
LastUserConfirmedResolutionSizeY=2160
FrameRateLimit=0.000000
bUseVSync=False

[ScalabilityGroups]
sg.ResolutionQuality=100.000000
sg.ViewDistanceQuality=4
sg.AntiAliasingQuality=4
sg.ShadowQuality=4
sg.PostProcessQuality=4
sg.TextureQuality=4
sg.EffectsQuality=4
sg.FoliageQuality=4
sg.ShadingQuality=4
sg.GlobalIlluminationQuality=4
sg.ReflectionQuality=4

[/Script/b1.B1GameUserSettings]
bRayTracingEnable=True
bFrameGenEnable=False
";

        public static string GetConfigPath() => ConfigPath;
    }
}