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
    public class GrabActionCommandHandler(IHBECSServiceFactory hBECSServiceFactory, IMediator mediator) : IRequestHandler<GrabActionCommand, bool>
    {
        public async Task<bool> Handle(GrabActionCommand request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);

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
            if (request.Start)
            {
                var data = await hBECSService.SendAsync<S104, E104>(new() { TrayNumber = pallet.PalletCode, TrayPosition = pallet.PalletId + 1, TrayType = pallet.OKNGType });

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
                // 上报数据

                // TODO:上报数据结果权限添加
                var data = await hBECSService.SendAsync<S105, E105>(new S105()
                {
                    TrayNumber = pallet.PalletCode,
                    TrayPosition = pallet.PalletId + 1,
                    MaterialId = MainStorage.Saves.KeysSave[request.Key].MaterialCode,
                    BatteryCount = pallet.ChannelNum,
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
                // 作业结束
                var e106 = await hBECSService.SendAsync<S106, E106>(new() { 
                    TrayNumber = pallet.PalletCode, 
                    TrayPosition = pallet.PalletId + 1,
                    TrayType = pallet.OKNGType,
                    PalletStatus = pallet.Channels.Take(pallet.ChannelNum).All(s => s.BatteryLevel != Models.BatteryLevel.None && s.BatteryLevel != Models.BatteryLevel.EMPTY) ? PalletStatus.满盘 : PalletStatus.不满
                });

                if (e106 == null)
                {
                    await mediator.Send(new WcsDataWarningBoxCommand("ECS通讯异常"));
                    return false;
                }
                if (e106.DeviceStatus != DeviceStatus2.OK)
                {
                    await mediator.Send(new WcsDataWarningBoxCommand($"错误码: {data.ErrorCode}"));
                    return false;
                }

            }
            //else
            //{
            //    throw new Exception("托盘类型错误");
            //}

            // TODO: 结束要用106命令




            return true;

        }
    }
}
