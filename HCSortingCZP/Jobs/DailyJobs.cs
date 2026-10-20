using HCSortingCZP.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HCSortingCZP.Jobs
{
    class DailyJobs() : IJob
    {
        /// <summary>
        /// 每日执行任务
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            MainStorage.SaveManager.BackUp();

            var fileRetentionDays = MainStorage.Saves.LogDeleteDay;
            if (fileRetentionDays <= 0)
            {
                return;
            }

            var fileCutoff = DateTime.Now.AddDays(-fileRetentionDays);
            CleanFileLogs(fileCutoff);
        }

        private static void CleanFileLogs(DateTime cutoff)
        {
            var logDirectory = new DirectoryInfo("logs");
            if (!logDirectory.Exists)
            {
                return;
            }

            foreach (var file in logDirectory.GetFiles())
            {
                if (file.CreationTime < cutoff)
                {
                    file.Delete();
                }
            }
        }
    }

    [DisallowConcurrentExecution]
    class DatabaseCleanupJobs(
        IDbContextFactory<LinxContext> dbContextFactory,
        ILogger<DatabaseCleanupJobs> logger) : IJob
    {
        private const int DeleteBatchSize = 1000;
        private const int DataFileGrowthMb = 256;
        private const int DataFileGrowthStepMb = 128;

        public async Task Execute(IJobExecutionContext context)
        {
            var retentionDays = MainStorage.Saves.DatabaseLogDeleteDay;
            await using var db = await dbContextFactory.CreateDbContextAsync(context.CancellationToken);
            var connection = db.Database.GetDbConnection();

            logger.LogInformation(
                "数据库维护开始：服务器={Server}，数据库={Database}，日志保留天数={RetentionDays}",
                connection.DataSource,
                connection.Database,
                retentionDays);

            try
            {
                await db.Database.OpenConnectionAsync(context.CancellationToken);

                if (retentionDays > 0)
                {
                    var cutoff = DateTime.Now.AddDays(-retentionDays);
                    try
                    {
                        await CleanDatabaseLogsAsync(db, cutoff, context.CancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "数据库日志清理失败，截止时间={Cutoff}", cutoff);
                    }
                }

                try
                {
                    await EnsureDataFileCapacityAsync(connection, context.CancellationToken);
                }
                catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 1827)
                {
                    logger.LogWarning(
                        ex,
                        "数据库已达到 SQL Server Express/LocalDB 的 10GB 单库上限，无法继续扩容；请依靠日志清理释放可复用空间，或升级 SQL Server 版本");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "数据库数据文件扩容失败");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "数据库维护失败");
            }
        }

        private async Task EnsureDataFileCapacityAsync(
            DbConnection connection,
            CancellationToken cancellationToken)
        {
            var files = new List<DataFileInfo>();
            await using (var command = connection.CreateCommand())
            {
                command.CommandText = """
                    SELECT name, size, max_size, growth, is_percent_growth,
                           ISNULL(FILEPROPERTY(name, 'SpaceUsed'), 0)
                    FROM sys.database_files
                    WHERE type_desc = 'ROWS';
                    """;

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    files.Add(new DataFileInfo(
                        reader.GetString(0),
                            Convert.ToInt64(reader.GetValue(1)),
                            Convert.ToBoolean(reader.GetValue(4)),
                            Convert.ToInt64(reader.GetValue(5))));
                }
            }

            foreach (var file in files)
            {
                var freePages = file.SizePages - file.UsedPages;
                var shouldGrow = freePages <= 128 * 128 || file.IsPercentGrowth;
                if (!shouldGrow)
                {
                    continue;
                }

                var currentSizeMb = Math.Max(1, (int)Math.Ceiling(file.SizePages / 128d));
                var targetSizeMb = currentSizeMb + DataFileGrowthMb;
                var fileName = file.Name.Replace("'", "''");

                await using var command = connection.CreateCommand();
                command.CommandText =
                    $"ALTER DATABASE CURRENT MODIFY FILE " +
                    $"(NAME = N'{fileName}', SIZE = {targetSizeMb}MB, " +
                    $"FILEGROWTH = {DataFileGrowthStepMb}MB, MAXSIZE = UNLIMITED);";
                await command.ExecuteNonQueryAsync(cancellationToken);

                logger.LogWarning(
                    "数据库数据文件空间不足，已扩容：文件={FileName}，原大小={OldSizeMb}MB，新大小={NewSizeMb}MB",
                    file.Name,
                    currentSizeMb,
                    targetSizeMb);
            }
        }

        private async Task CleanDatabaseLogsAsync(
            LinxContext db,
            DateTime cutoff,
            CancellationToken cancellationToken)
        {
            var wcsLogs = await DeleteInBatchesAsync(
                db.WcsLogs.Where(log => log.RequestTime < cutoff),
                cancellationToken);

            var palletLogs = await DeleteInBatchesAsync(
                db.PalletLogs.Where(log => log.ZuPanTime < cutoff),
                cancellationToken);

            var warningLogs = await DeleteInBatchesAsync(
                db.WarningLogs.Where(log => log.WarningTime < cutoff),
                cancellationToken);

            var ngBeltCellLogs = await DeleteInBatchesAsync(
                db.NgBeltCellLogs.Where(log => log.RecordTime < cutoff),
                cancellationToken);

            var total = wcsLogs + palletLogs + warningLogs + ngBeltCellLogs;
            logger.LogInformation(
                "数据库日志清理完成：删除总数={Total}，WcsLogs={WcsLogs}，PalletLogs={PalletLogs}，WarningLogs={WarningLogs}，NgBeltCellLogs={NgBeltCellLogs}，截止时间={Cutoff}",
                total,
                wcsLogs,
                palletLogs,
                warningLogs,
                ngBeltCellLogs,
                cutoff);
        }

        private static async Task<int> DeleteInBatchesAsync<TEntity>(
            IQueryable<TEntity> query,
            CancellationToken cancellationToken)
            where TEntity : class
        {
            var deleted = 0;
            while (true)
            {
                var currentBatch = await query
                    .Take(DeleteBatchSize)
                    .ExecuteDeleteAsync(cancellationToken);

                deleted += currentBatch;
                if (currentBatch < DeleteBatchSize)
                {
                    return deleted;
                }
            }
        }

        private sealed record DataFileInfo(
            string Name,
            long SizePages,
            bool IsPercentGrowth,
            long UsedPages);
    }

    [DisallowConcurrentExecution]
    class SaveJobs(ILogger<SaveJobs> logger) : IJob
    {
        /// <summary>
        /// 每日执行任务
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                MainStorage.SaveManager.Save();
            }
            catch (OutOfMemoryException ex)
            {
                logger.LogCritical(ex, "配置保存时内存不足，本次保存已跳过");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "配置保存失败");
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 每分钟上报一次ECS
    /// </summary>
    //internal class StatusJobs(PLCTaskService<HBYWPLCContext> hBYWPLCContext, EquipStatusReportCommand request, NewECSService newECSService) : IJob
    //{

    //    [JsonIgnore]
    //    static Lazy<ILogger> _lazyLogger = new Lazy<ILogger>(App.GetService<ILogger<MainViewModel>>);
    //    [JsonIgnore]
    //    static ILogger _logger => _lazyLogger.Value;

    //    public async Task Execute(IJobExecutionContext context)
    //    {
    //        try
    //        {
    //            var res = await newECSService.EquipStatusReportAsync([new EquipStatusRequest(request.Key)
    //        {

    //            StatusEnum = (EquipStatus)hBYWPLCContext.Context.DB500.Mode,
    //        }]);
    //            //MainStorage.WcsOnline = res != null;
    //            _logger.LogInformation($"调度设备状态响应: {res}");
    //        }
    //        catch (Exception)
    //        {
    //            //MainStorage.WcsOnline = false;
    //            _logger.LogError("调度设备状态异常");

    //        }
    //    }
    //}
}
