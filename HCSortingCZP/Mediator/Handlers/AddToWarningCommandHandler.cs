using HCSortingCZP.Data;
using HCSortingCZP.Data.Models;
using HCSortingCZP.Mediator.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HCSortingCZP.Mediator.Handlers
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