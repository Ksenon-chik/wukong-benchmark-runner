using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace WukongBenchmarkRunner
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("======================================================================");
            Console.WriteLine("   AUTOMATED BENCHMARK RUNNER - BLACK MYTH: WUKONG BENCHMARK TOOL    ");
            Console.WriteLine("======================================================================\n");

            // 1. Сбор характеристик ПК
            Console.WriteLine("[1/4] Сбор характеристик системы...");
            var specs = SystemInfoProvider.GetSpecs();
            PrintSpecsTable(specs);

            // 2. Выполнение CPU-теста
            Console.WriteLine("\n[2/4] Запуск CPU-теста (CPU-bound профиль)...");
            BenchmarkConfigManager.ApplyPreset(PresetType.CpuBound);
            Console.WriteLine("      Конфигурация приведена к: 720p, RenderScale 50%, Low Quality, No RT.");
            var cpuResults = await BenchmarkRunnerAndParser.RunAndParseAsync();

            // 3. Выполнение GPU-теста
            Console.WriteLine("\n[3/4] Запуск GPU-теста (GPU-bound профиль)...");
            BenchmarkConfigManager.ApplyPreset(PresetType.GpuBound);
            Console.WriteLine("      Конфигурация приведена к: 4K/1440p Native, Cinematic Quality, Full RT.");
            var gpuResults = await BenchmarkRunnerAndParser.RunAndParseAsync();

            // 4. Формирование итогового отчёта
            Console.WriteLine("\n[4/4] Формирование итогового отчёта...");
            PrintFinalReport(specs, cpuResults, gpuResults);
        }

        private static void PrintSpecsTable(HardwareSpecs s)
        {
            Console.WriteLine($"┌──────────────────────┬──────────────────────────────────────────┐");
            Console.WriteLine($"│ Процессор (CPU)      │ {s.CpuName,-40} │");
            Console.WriteLine($"│ Ядра / Потоки        │ {s.CpuCores} cores / {s.CpuThreads} threads{"",-23} │");
            Console.WriteLine($"│ Видеокарта (GPU)     │ {s.GpuName,-40} │");
            Console.WriteLine($"│ Видеопамять (VRAM)   │ {s.GpuVramGb} GB{"",-35} │");
            Console.WriteLine($"│ Оперативная память   │ {s.TotalRamGb} GB RAM{"",-32} │");
            Console.WriteLine($"│ ОС                   │ {s.OsVersion,-40} │");
            Console.WriteLine($"└──────────────────────┴──────────────────────────────────────────┘");
        }

        private static void PrintFinalReport(HardwareSpecs specs, BenchmarkMetrics cpu, BenchmarkMetrics gpu)
        {
            Console.WriteLine("\n======================================================================");
            Console.WriteLine("                       ИТОГОВЫЕ РЕЗУЛЬТАТЫ                            ");
            Console.WriteLine("======================================================================\n");

            Console.WriteLine("--- [ МЕТРИКИ CPU-ТЕСТА (720p Low) ] ---");
            Console.WriteLine($"Средний FPS (Average):   {cpu.AvgFps:F1}");
            Console.WriteLine($"Минимальный FPS (Min):    {cpu.MinFps:F1}");
            Console.WriteLine($"95% Low FPS:              {cpu.Percentile95Fps:F1}");

            Console.WriteLine("\n--- [ МЕТРИКИ GPU-ТЕСТА (4K/1440p Cinematic + RT) ] ---");
            Console.WriteLine($"Средний FPS (Average):   {gpu.AvgFps:F1}");
            Console.WriteLine($"Минимальный FPS (Min):    {gpu.MinFps:F1}");
            Console.WriteLine($"95% Low FPS:              {gpu.Percentile95Fps:F1}");

            Console.WriteLine("\n--- [ НАСТРОЙКИ ТЕСТОВ ] ---");
            Console.WriteLine("CPU Test: Res=1280x720, Scale=50%, Presets=Low (0), RT=Off, FrameGen=Off");
            Console.WriteLine("GPU Test: Res=3840x2160, Scale=100%, Presets=Cinematic (4), RT=On, FrameGen=Off");
        }
    }
}