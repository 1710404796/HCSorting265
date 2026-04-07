using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Utils;
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
            var pallet = CZPUtils.GetPalletByInsideNum(request.Key, request.InsideStationNum);
            if (pallet == null)
            {
                await mediator.Send(new WcsDataWarningBoxCommand($"{request.Key},{request.InsideStationNum}托盘不存在，无法调用FMS设备叫料接口"), cancellationToken);
                return false;
            }

            // if (!pallet.CallMaterialEnable)
            // {
            //     return true;
            // }

            var fmsService = hBECSServiceFactory.Create(request.Key).NewFMSService;
            var res = await fmsService.YZCallMaterialAsync(new YZCallMaterialRequest(request.Key)
            {
                desConveyorCode = pallet.OutsideStationNum.ToString()
            });
            if (res.IsSuccess)
            {
                return true;
            }

            var err = res.Match(Succ: _ => string.Empty, Fail: f => f.Message);
            await mediator.Send(new WcsDataWarningBoxCommand($"FMS设备叫料失败: {err}"), cancellationToken);
            return false;
        }
    }
}
