using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Service;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    internal class YZCallMaterialCommandHandler(IHBECSServiceFactory hBECSServiceFactory, IMediator mediator)
        : IRequestHandler<YZCallMaterialCommand, bool>
    {
        public async Task<bool> Handle(YZCallMaterialCommand request, CancellationToken cancellationToken)
        {
            var fmsService = hBECSServiceFactory.Create(request.Key).NewFMSService;
            var res = await fmsService.YZCallMaterialAsync(new YZCallMaterialRequest(request.Key));
            if (res.IsSuccess)
            {
                return true;
            }

            var err = res.Match(Succ: _ => string.Empty, Fail: f => f.Message);
            await mediator.Send(new WcsDataWarningBoxCommand($"FMS NG叫盘失败: {err}"), cancellationToken);
            return false;
        }
    }
}
