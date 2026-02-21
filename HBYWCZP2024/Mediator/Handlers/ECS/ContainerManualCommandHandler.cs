using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Service;
using HBYWCZP2024.Service.Models;
using HBYWCZP2024.Utils;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS;

internal class ContainerManualCommandHandler(IHBECSServiceFactory factory, IMediator mediator)
    : IRequestHandler<ContainerManualCommand, Arr<Channel>>
{
    public async Task<Arr<Channel>> Handle(ContainerManualCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ContainerCode))
        {
            await mediator.Send(new WcsDataWarningBoxCommand($"{request.Key}托盘条码不能为空"), cancellationToken);
            throw new ArgumentException("ContainerCode is required", nameof(request.ContainerCode));
        }

        var service = factory.Create(request.Key);
        var result = await service.MesService.ContainerManualAsync(request.ContainerCode);

        var response = result.Match(
            Succ: s => s,
            Fail: ex =>
            {
                mediator.Send(new WcsDataWarningBoxCommand(ex.Message), cancellationToken).Wait(cancellationToken);
                throw ex;
            }
        );

        var channels = response.Rows
            .Where(r => r?.CellDataList != null)
            .SelectMany(r => r.CellDataList)
            .Where(c =>
                c != null
                && !string.IsNullOrWhiteSpace(c.Identification)
                && int.TryParse(c.LocationBat, out var channelId)
                && channelId > 0)
            .Select(c =>
            {
                _ = int.TryParse(c.LocationBat, out var channelId);
                var level = CZPUtils.GetLevelFromMes(c.QualityStatus ?? "NG");
                return new Channel(channelId, Some(new BatteryWithLevel(c.Identification!, Some(level))));
            })
            .OrderBy(x => x.ChannelId)
            .ToArr();

        return channels;
    }
}
