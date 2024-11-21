using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Models;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Service;
using HBYWCZP2024.Utils;
using MediatR;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    public class ApplyBatteryInfoCommandHandler(HBECSServiceFactory hBECSServiceFactory, IMediator mediator) : IRequestHandler<ApplyBatteryInfoCommand, BatteryLevel[]?>
    {

        public async Task<BatteryLevel[]?> Handle(ApplyBatteryInfoCommand request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);



            var pallet = CZPUtils.GetPalletByOutsideNum(request.Key, request.StationNum);

            if (pallet == null)
            {
                throw new Exception("托盘不存在");
            }

            var data = await hBECSService.SendAsync<S103, E103>(new S103() {  TrayNumber = pallet.PalletCode });

            if (data == null)
            {
                await mediator.Send(new WcsDataWarningBoxCommand("ECS通讯异常"));
                return null;
            }

            return data.GetBatteryLevels();

        }
    }
}
