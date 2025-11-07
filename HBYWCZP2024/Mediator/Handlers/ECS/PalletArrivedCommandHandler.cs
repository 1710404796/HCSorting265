using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Service;
using HBYWCZP2024.Utils;
using LanguageExt.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    internal class PalletArrivedCommandHandler
        (IHBECSServiceFactory hBECSServiceFactory,
        IMediator mediator) : IRequestHandler<PalletArrivedCommand, bool>
    {
        public async Task<bool> Handle(PalletArrivedCommand request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);
            var newEcsService = hBECSService.NewECSService;
            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.PalletArrivedEnable)
            {
                return true;
            }

            var pallet = CZPUtils.GetPalletByInsideNum(request.Key, request.InsideStationNum);
            if (pallet == null)
            {
                throw new Exception($"{request.Key},{request.InsideStationNum}托盘不存在");
            }
            if (string.IsNullOrWhiteSpace(pallet.PalletCode))
            {
                await mediator.Send(new WcsDataWarningBoxCommand($"{request.Key},{request.InsideStationNum}进站托盘条码为空"));
                return false;
            }

            Result<EquipEnterResponse> response = await newEcsService.EquipEnterAsync(
                new EquipEnterRequest(request.Key)
                {
                    TrayCode = pallet.PalletCode,
                    TrayType = pallet.OKNGType.GetTrayTypeStr(),
                    Location = pallet.OutsideStationNum.ToString() // 可能+1
                });
            // 不知道要不要上报
            //var data = await mediator.Send(new EquipStatusReportCommand(request.Key, pallet.InsideNum, EquipStatus.T));
            ////PalletStatus = request.isSpecial ? PalletStatus.不满 : PalletStatus.满盘,

            //if (!data)
            //{
            //    await mediator.Send(new WcsDataWarningBoxCommand("ECS上报状态失败"));
            //    return false;
            //}


            var a = await response.Match(
                Succ: async r =>
                {
                    r.Result?.CellList?.ToList().ForEach(async cell =>
                    {
                        var channelId = cell.Channel;
                        var type = cell.CellType switch
                        {
                            0 => Models.BatteryLevel.EMPTY,
                            1 => Models.BatteryLevel.OK,
                            2 => Models.BatteryLevel.NG,
                            3 => Models.BatteryLevel.Fake,
                            _ => Models.BatteryLevel.EMPTY

                        };
                        await pallet.BindBattery(channelId - 1, cell.CellCode, true, type);
                    });

                    return true;    
                },
                Fail: async ex =>
                {
                    await mediator.Send(new WcsDataWarningBoxCommand($"托盘{pallet.PalletCode}上报MES失败: {ex.Message}"));
                    return false;
                }
             );
            return a;
            //var data = await hBECSService.SendAsync<S102, E102>(new() { TrayNumber = pallet.PalletCode, TrayPosition = pallet.PalletId + 1, TrayType = pallet.OKNGType }, pallet.OutsideStationNum);
            //return data != null;// && data.ErrorCode == "0000";
        }
    }
}
