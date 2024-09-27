using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Service;
using HBYWCZP2024.Utils;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    public class GrabActionCommandHandler(HBECSService hBECSService, IMediator mediator) : IRequestHandler<GrabActionCommand, bool>
    {
        public async Task<bool> Handle(GrabActionCommand request, CancellationToken cancellationToken)
        {
            var pallet = CZPUtils.GetPalletByOutsideNum(request.StationNum);

            if (pallet == null)
            {
                throw new Exception("托盘不存在");

            }

            var data = await hBECSService.SendAsync<S104, E104>(new S104() {  TrayNumber =  pallet.PalletCode, TrayPosition= pallet.PalletId + 1 });

            if (data == null)
            {
                await mediator.Send(new WcsDataWarningBoxCommand("ECS通讯异常"));
                return false;
            }

            if (data.DeviceStatus != DeviceStatus2.OK)
            {
                await mediator.Send(new WcsDataWarningBoxCommand($"错误码: {data.ErrorCode}"));
                return false;
            }

            return true;

        }
    }
}
