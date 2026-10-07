using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WukongBenchmarkRunner
{
    public record BenchmarkMetrics(
        double AvgFps,
        double MinFps,
        double MaxFps,
        double Percentile95Fps,
        string RawSummary
    );

    public static class BenchmarkRunnerAndParser
    {
        private const string SteamAppId = "3132990"; // Black Myth: Wukong Benchmark Tool
        private static readonly string SavedDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"b1\Saved\"
        );

        public static async Task<BenchmarkMetrics> RunAndParseAsync(CancellationToken ct = default)
        {
            DateTime startTime = DateTime.Now;

            // Запуск бенчмарка через протокол Steam
            Process.Start(new ProcessStartInfo
            {
                FileName = $"steam://rungameid/{SteamAppId}",
                UseShellExecute = true
            });

            Console.WriteLine("    [+] Ожидание появления процесса b1 / b1-Win64-Shipping...");
            Process? gameProcess = null;
            while (gameProcess == null && !ct.IsCancellationRequested)
            {
                gameProcess = Process.GetProcessesByName("b1-Win64-Shipping")
                    .Concat(Process.GetProcessesByName("b1"))
                    .FirstOrDefault();
                await Task.Delay(1000, ct);
            }

            if (gameProcess != null)
            {
                Console.WriteLine($"    [+] Бенчмарк запущен (PID: {gameProcess.Id}). Ожидание завершения работы...");
                await gameProcess.WaitForExitAsync(ct);
                Console.WriteLine("    [+] Процесс бенчмарка успешно завершился.");
            }

            // Небольшая пауза для сохранения отчёта на диск
            await Task.Delay(3000, ct);

            return ParseLatestResults(startTime);
        }

        private static BenchmarkMetrics ParseLatestResults(DateTime runStartTime)
        {
            // Поиск последнего созданного лога или отчёта в директории b1\Saved
            var logDir = new DirectoryInfo(Path.Combine(SavedDataDir, "Logs"));
            var saveDir = new DirectoryInfo(Path.Combine(SavedDataDir, "SaveGames"));

            FileInfo? latestFile = null;

            if (logDir.Exists)
            {
                latestFile = logDir.GetFiles("*.log")
                    .Where(f => f.LastWriteTime >= runStartTime.AddMinutes(-1))
                    .OrderByDescending(f => f.LastWriteTime)
                    .FirstOrDefault();
            }

            if (latestFile == null && saveDir.Exists)
            {
                latestFile = saveDir.GetFiles("*.*", SearchOption.AllDirectories)
                    .Where(f => f.LastWriteTime >= runStartTime.AddMinutes(-1))
                    .OrderByDescending(f => f.LastWriteTime)
                    .FirstOrDefault();
            }

            // Заглушка парсинга для демонстрации (считывает ключевые строки типа Avg FPS / Min FPS)
            if (latestFile != null && File.Exists(latestFile.FullName))
            {
                string text = File.ReadAllText(latestFile.FullName);
                return ExtractMetricsFromText(text);
            }

            // Дефолтный ответ, если структурированный отчёт не найден
            return new BenchmarkMetrics(0.0, 0.0, 0.0, 0.0, "Отчёт бенчмарка записан во внутренние сохранения игры.");
        }

        private static BenchmarkMetrics ExtractMetricsFromText(string rawText)
        {
            // Регулярные выражения для парсинга значений FPS из логов
            double avg = 0, min = 0, max = 0, p95 = 0;

            foreach (var line in rawText.Split('\n'))
            {
                if (line.Contains("Avg FPS", StringComparison.OrdinalIgnoreCase))
                    double.TryParse(line.Split(':').LastOrDefault(), out avg);
                if (line.Contains("Min FPS", StringComparison.OrdinalIgnoreCase))
                    double.TryParse(line.Split(':').LastOrDefault(), out min);
                if (line.Contains("Max FPS", StringComparison.OrdinalIgnoreCase))
                    double.TryParse(line.Split(':').LastOrDefault(), out max);
                if (line.Contains("95% Low", StringComparison.OrdinalIgnoreCase))
                    double.TryParse(line.Split(':').LastOrDefault(), out p95);
            }

            return new BenchmarkMetrics(avg, min, max, p95, rawText.Length > 200 ? rawText[..200] + "..." : rawText);
        }
    }
}