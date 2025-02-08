using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Models.CZPallet;
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
    internal class PalletOutCommandHandler(IHBECSServiceFactory hBECSServiceFactory, IMediator mediator) : IRequestHandler<PalletOutCommand, bool>
    {
        public async Task<bool> Handle(PalletOutCommand request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);
            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.PalletOutEnable)
            {
                return true;
            }

            var pallet = CZPUtils.GetPalletByInsideNum(request.Key, request.InsideStationNum);
            if (pallet == null)
            {
                throw new Exception("托盘不存在");
            }
            TrayType Get(Pallet pallet)
            {
                if (pallet.Type == PalletType.筛选)
                {
                    if (pallet.Channels.Take(pallet.ChannelNum).All(s => string.IsNullOrWhiteSpace(s.Code)))
                    {
                        return TrayType.空筛选类型;
                    }
                }
                return pallet.OKNGType;
            }
            var type = Get(pallet);
            var data = await hBECSService.SendAsync<S107, E107>(new () { TrayNumber = pallet.PalletCode, TrayPosition = pallet.PalletId + 1, TrayType = type });
            return data != null;// && data.ErrorCode == "0";
        }
    }
}
