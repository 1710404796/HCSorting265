using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace HCSortingCZP.Service
{
    public sealed class ProcessMemoryMonitorService : BackgroundService
    {
        private static readonly TimeSpan MonitorInterval = TimeSpan.FromMinutes(5);
        private readonly string _logPath = Path.Combine(AppContext.BaseDirectory, "logs", "HCSortingCZP.MemoryMonitor.csv");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await WriteSnapshotAsync("start", stoppingToken);

            using PeriodicTimer timer = new(MonitorInterval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await WriteSnapshotAsync("interval", stoppingToken);
            }
        }

        private async Task WriteSnapshotAsync(string reason, CancellationToken cancellationToken)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
                var needsHeader = !File.Exists(_logPath) || new FileInfo(_logPath).Length == 0;

                using var process = Process.GetCurrentProcess();
                process.Refresh();

                var gcInfo = GC.GetGCMemoryInfo();
                var line = string.Join(',',
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                    reason,
                    process.Id.ToString(CultureInfo.InvariantCulture),
                    Math.Round(TimeSpan.FromMilliseconds(Environment.TickCount64).TotalMinutes, 2).ToString(CultureInfo.InvariantCulture),
                    ToMb(process.PrivateMemorySize64),
                    ToMb(process.WorkingSet64),
                    ToMb(process.VirtualMemorySize64),
                    ToMb(process.PagedMemorySize64),
                    ToMb(GC.GetTotalMemory(false)),
                    ToMb(gcInfo.HeapSizeBytes),
                    ToMb(gcInfo.TotalCommittedBytes),
                    SafeThreadCount(process),
                    SafeHandleCount(process),
                    GC.CollectionCount(0).ToString(CultureInfo.InvariantCulture),
                    GC.CollectionCount(1).ToString(CultureInfo.InvariantCulture),
                    GC.CollectionCount(2).ToString(CultureInfo.InvariantCulture));

                await using FileStream stream = new(_logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                await using StreamWriter writer = new(stream, Encoding.UTF8);

                if (needsHeader)
                {
                    await writer.WriteLineAsync("time,reason,pid,uptimeMinutes,privateMb,workingSetMb,virtualMb,pagedMb,managedMb,gcHeapMb,gcCommittedMb,threadCount,handleCount,gcGen0,gcGen1,gcGen2");
                }

                await writer.WriteLineAsync(line);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private static string ToMb(long bytes)
        {
            return Math.Round(bytes / 1024d / 1024d, 2).ToString(CultureInfo.InvariantCulture);
        }

        private static string SafeThreadCount(Process process)
        {
            try
            {
                return process.Threads.Count.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string SafeHandleCount(Process process)
        {
            try
            {
                return process.HandleCount.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
