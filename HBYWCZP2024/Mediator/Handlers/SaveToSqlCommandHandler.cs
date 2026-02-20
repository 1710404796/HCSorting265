using HBYWCZP2024;
using HBYWCZP2024.Models;
using HBYWCZP2024.Mediator.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers
{
    internal class SaveToSqlCommandHandler(ILogger<SaveToSqlCommandHandler> logger) : IRequestHandler<SaveToSqlCommand, bool>
    {
        public async Task<bool> Handle(SaveToSqlCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var key = string.IsNullOrWhiteSpace(request.Key)
                    ? MainStorage.Saves.SelectStageName
                    : request.Key;

                if (!string.IsNullOrWhiteSpace(key)
                    && MainStorage.Saves.KeysSave.TryGetValue(key, out var mono)
                    && mono.ApiEnableTable is not null
                    && !mono.ApiEnableTable.UploadCupBindingToDbEnable)
                {
                    logger.LogInformation("外部数据库写入已禁用, Key={Key}", key);
                    return true;
                }

                if (string.IsNullOrWhiteSpace(MainStorage.Saves.ExDbConnectionString))
                {
                    logger.LogWarning("外部数据库连接信息缺失");
                    return false;
                }

                var tableName = string.IsNullOrWhiteSpace(MainStorage.Saves.ExDbTableName)
                    ? request.TableName
                    : MainStorage.Saves.ExDbTableName;

                if (string.IsNullOrWhiteSpace(tableName))
                {
                    logger.LogWarning("外部数据库表名缺失");
                    return false;
                }

                var items = request.CupBatteryModels?
                    .Where(s => s is not null)
                    .ToArray() ?? System.Array.Empty<CupBatteryModel>();

                if (items.Length == 0)
                {
                    logger.LogWarning("外部数据库写入数据缺失");
                    return false;
                }

                var values = new string[items.Length];
                for (var i = 0; i < items.Length; i++)
                {
                    values[i] = $"(@equipNum{i}, @containerCode{i}, @identification{i}, @message{i}, @time{i})";
                }

                var insertSql = $"INSERT INTO `{tableName}` (`equipNum`, `containerCode`, `identification`, `message`, `Time`) " +
                                $"VALUES {string.Join(", ", values)};";

                await using var connection = new MySqlConnection(MainStorage.Saves.ExDbConnectionString);
                await connection.OpenAsync(cancellationToken);
                await using var command = new MySqlCommand(insertSql, connection);

                for (var i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    command.Parameters.AddWithValue($"@equipNum{i}", request.EquipNum ?? string.Empty);
                    command.Parameters.AddWithValue($"@containerCode{i}", item.CupCode ?? string.Empty);
                    command.Parameters.AddWithValue($"@identification{i}", item.BatteryCode ?? string.Empty);
                    command.Parameters.AddWithValue($"@message{i}", request.Message ?? string.Empty);
                    command.Parameters.AddWithValue($"@time{i}", request.Time ?? DateTime.Now);
                }

                var rows = await command.ExecuteNonQueryAsync(cancellationToken);
                if (rows == 0)
                {
                    logger.LogWarning("外部数据库写入失败");
                }
                return rows > 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "外部数据库写入发生异常");
                return false;
            }
        }
    }
}
