using HBYWCZP2024;
using HBYWCZP2024.Mediator.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System;
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

                var insertSql = $"INSERT INTO `{tableName}` (`equipNum`, `containerCode`, `identification`, `message`, `Time`) " +
                                "VALUES (@equipNum, @containerCode, @identification, @message, @time);";

                await using var connection = new MySqlConnection(MainStorage.Saves.ExDbConnectionString);
                await connection.OpenAsync(cancellationToken);
                await using var command = new MySqlCommand(insertSql, connection);
                command.Parameters.AddWithValue("@equipNum", request.EquipNum ?? string.Empty);
                command.Parameters.AddWithValue("@containerCode", request.ContainerCode ?? string.Empty);
                command.Parameters.AddWithValue("@identification", request.Identification ?? string.Empty);
                command.Parameters.AddWithValue("@message", request.Message ?? string.Empty);
                command.Parameters.AddWithValue("@time", request.Time ?? DateTime.Now);

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
