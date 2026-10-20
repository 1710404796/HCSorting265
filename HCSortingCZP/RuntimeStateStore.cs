using HCSortingCZP.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace HCSortingCZP
{
    public static class RuntimeStateStore
    {
        private const int Version = 1;
        private const int BackupRetentionDays = 7;
        private static readonly object SyncRoot = new();
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        private static string RootDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RuntimeState");
        private static string BackupDirectory => Path.Combine(RootDirectory, "Backup");
        private static string CurrentPath => Path.Combine(RootDirectory, "RuntimeState.json");

        public static void Restore()
        {
            lock (SyncRoot)
            {
                EnsureDirectories();
                BackupCurrentForToday();
                DeleteExpiredBackups();

                var snapshot = File.Exists(CurrentPath) ? TryReadSnapshot(CurrentPath) : null;
                snapshot ??= TryReadLatestBackup();
                if (snapshot?.Stages == null)
                {
                    return;
                }

                foreach (var stageState in snapshot.Stages)
                {
                    if (string.IsNullOrWhiteSpace(stageState.Name)
                        || !MainStorage.Saves.KeysSave.TryGetValue(stageState.Name, out var stageMono))
                    {
                        continue;
                    }

                    foreach (var stationState in stageState.Stations)
                    {
                        var station = stageMono.Stage.Stations.FirstOrDefault(s => s.InsideNum == stationState.InsideNum);
                        if (station == null)
                        {
                            continue;
                        }

                        ApplyStation(station, stationState);
                    }
                }
            }
        }

        public static void Save()
        {
            lock (SyncRoot)
            {
                EnsureDirectories();
                BackupCurrentForToday();
                DeleteExpiredBackups();

                var snapshot = CreateSnapshot();
                var json = JsonSerializer.Serialize(snapshot, JsonOptions);
                var tempPath = Path.Combine(RootDirectory, $"RuntimeState.{Guid.NewGuid():N}.tmp");

                File.WriteAllText(tempPath, json, Encoding.UTF8);
                File.Move(tempPath, CurrentPath, true);

                var todayBackupPath = GetBackupPath(DateTime.Today);
                if (!File.Exists(todayBackupPath))
                {
                    File.Copy(CurrentPath, todayBackupPath, false);
                }
            }
        }

        private static RuntimeStateSnapshot? TryReadSnapshot(string path)
        {
            try
            {
                var json = File.ReadAllText(path, Encoding.UTF8);
                return JsonSerializer.Deserialize<RuntimeStateSnapshot>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取运行状态失败: {path}, {ex.Message}");
                return null;
            }
        }

        private static RuntimeStateSnapshot? TryReadLatestBackup()
        {
            return new DirectoryInfo(BackupDirectory)
                .EnumerateFiles("*.json")
                .OrderByDescending(file => TryGetBackupDate(file, out var backupDate) ? backupDate : file.LastWriteTime.Date)
                .Select(file => TryReadSnapshot(file.FullName))
                .FirstOrDefault(snapshot => snapshot?.Stages != null);
        }

        private static RuntimeStateSnapshot CreateSnapshot()
        {
            return new RuntimeStateSnapshot
            {
                Version = Version,
                SavedAt = DateTime.Now,
                Stages = MainStorage.Saves.KeysSave
                    .Select(pair => new RuntimeStageState
                    {
                        Name = pair.Key,
                        Stations = pair.Value.Stage.Stations.Select(CreateStationState).ToList(),
                    })
                    .ToList(),
            };
        }

        private static RuntimeStationState CreateStationState(XYLStation station)
        {
            return new RuntimeStationState
            {
                InsideNum = station.InsideNum,
                OutsideStationNum = station.OutsideStationNum,
                Name = station.Name,
                PalletCode = station.PalletCode,
                Channels = station.Channels
                    .Select((channel, index) => new RuntimeChannelState
                    {
                        Index = index,
                        Code = channel.Code,
                        BatteryLevel = channel.BatteryLevel,
                        ContainerFrom = channel.ContainerFrom,
                        StationFrom = channel.StationFrom,
                        ChannelFrom = channel.ChannelFrom,
                    })
                    .Where(ShouldSaveChannel)
                    .ToList(),
            };
        }

        private static bool ShouldSaveChannel(RuntimeChannelState channel)
        {
            return channel.Code != StationChannel.DefaultCode
                || channel.BatteryLevel is not BatteryLevel.EMPTY and not BatteryLevel.None
                || !string.IsNullOrWhiteSpace(channel.ContainerFrom)
                || channel.StationFrom != 0
                || channel.ChannelFrom != 0;
        }

        private static void ApplyStation(XYLStation station, RuntimeStationState stationState)
        {
            station.PalletCode = stationState.PalletCode ?? string.Empty;

            foreach (var channel in station.Channels)
            {
                channel.Reset(station.IsFake);
            }

            foreach (var channelState in stationState.Channels)
            {
                if (channelState.Index < 0 || channelState.Index >= station.Channels.Count)
                {
                    continue;
                }

                var channel = station.Channels[channelState.Index];
                channel.Code = channelState.Code ?? StationChannel.DefaultCode;
                channel.BatteryLevel = channelState.BatteryLevel;
                channel.ContainerFrom = channelState.ContainerFrom ?? string.Empty;
                channel.StationFrom = channelState.StationFrom;
                channel.ChannelFrom = channelState.ChannelFrom;
            }
        }

        private static void EnsureDirectories()
        {
            Directory.CreateDirectory(RootDirectory);
            Directory.CreateDirectory(BackupDirectory);
        }

        private static void BackupCurrentForToday()
        {
            if (!File.Exists(CurrentPath))
            {
                return;
            }

            var todayBackupPath = GetBackupPath(DateTime.Today);
            if (!File.Exists(todayBackupPath))
            {
                File.Copy(CurrentPath, todayBackupPath, false);
            }
        }

        private static void DeleteExpiredBackups()
        {
            var cutoff = DateTime.Today.AddDays(-(BackupRetentionDays - 1));
            foreach (var file in new DirectoryInfo(BackupDirectory).EnumerateFiles("*.json"))
            {
                if (TryGetBackupDate(file, out var backupDate))
                {
                    if (backupDate < cutoff)
                    {
                        file.Delete();
                    }
                }
                else if (file.LastWriteTime.Date < cutoff)
                {
                    file.Delete();
                }
            }
        }

        private static bool TryGetBackupDate(FileInfo file, out DateTime date)
        {
            return DateTime.TryParseExact(
                Path.GetFileNameWithoutExtension(file.Name),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        private static string GetBackupPath(DateTime date)
        {
            return Path.Combine(BackupDirectory, $"{date:yyyy-MM-dd}.json");
        }

        private sealed class RuntimeStateSnapshot
        {
            public int Version { get; set; }
            public DateTime SavedAt { get; set; }
            public List<RuntimeStageState> Stages { get; set; } = new();
        }

        private sealed class RuntimeStageState
        {
            public string Name { get; set; } = string.Empty;
            public List<RuntimeStationState> Stations { get; set; } = new();
        }

        private sealed class RuntimeStationState
        {
            public int InsideNum { get; set; }
            public int OutsideStationNum { get; set; }
            public string Name { get; set; } = string.Empty;
            public string PalletCode { get; set; } = string.Empty;
            public List<RuntimeChannelState> Channels { get; set; } = new();
        }

        private sealed class RuntimeChannelState
        {
            public int Index { get; set; }
            public string Code { get; set; } = StationChannel.DefaultCode;
            public BatteryLevel BatteryLevel { get; set; } = BatteryLevel.EMPTY;
            public string ContainerFrom { get; set; } = string.Empty;
            public int StationFrom { get; set; }
            public int ChannelFrom { get; set; }
        }
    }
}
