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
            var pallet = CZPUtils.GetPalletByOutsideNum(request.Key, request.StationNum);
           
            if (pallet == null)
            {
                throw new Exception("托盘不存在");

            }

            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.GrabActionInsertEnable)
            {
                if (pallet.Type == Models.CZPallet.PalletType.组盘)
                {
                    return true;
                }

            }
            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.GrabActionRemoveEnable)
            {
                if (pallet.Type == Models.CZPallet.PalletType.拆盘 || pallet.Type == Models.CZPallet.PalletType.筛选)
                {
                    return true;
                }

            }
            if (pallet.Type == Models.CZPallet.PalletType.组盘)
            {
                var data = await hBECSService.SendAsync<S104, E104>(new S104() { TrayNumber = pallet.PalletCode, TrayPosition = pallet.PalletId + 1 });

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
            }
            else if (pallet.Type == Models.CZPallet.PalletType.拆盘 || pallet.Type == Models.CZPallet.PalletType.筛选)
            {
                var data = await hBECSService.SendAsync<S106, E106>(new S106() 
                { 
                    TrayNumber = pallet.PalletCode, 
                    TrayPosition = pallet.PalletId + 1 ,
                    BatteryBarcodes = pallet.Channels.Take(pallet.ChannelNum).Select(x => x.Code).ToList()
                });

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
            }
            else
            {
                throw new Exception("托盘类型错误");
            }

            // TODO: 结束要用106命令
            

            

            return true;

        }
    }
}
