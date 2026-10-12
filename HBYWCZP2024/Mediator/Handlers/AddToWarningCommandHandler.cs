using HBYWCZP2024.Data;
using HBYWCZP2024.Data.Models;
using HBYWCZP2024.Mediator.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HBYWCZP2024.Mediator.Handlers
{
    public class AddToWarningCommandHandler(IDbContextFactory<LinxContext> dbContextFactory, ILogger<AddToWarningCommandHandler> logger) : IRequestHandler<AddToWarningCommand>
    {
        public async Task Handle(AddToWarningCommand request, CancellationToken cancellationToken)
        {
            await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            context.WarningLogs.Add(new WarningLog
            {
                Message = request.Message
            });
            try
            {
                var rows = await context.SaveChangesAsync(cancellationToken);
                if (rows == 0)
                {
                    logger.LogWarning("写入报警日志失败");
                }
            }
            catch (Exception ex)
            {
                logger.LogError("写入报警发生错误 ex: {ex}", ex.Message);
            }

        }
    }
}