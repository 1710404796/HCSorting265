using HCSortingCZP.Mediator.Commands;
using HCSortingCZP.Mediator.ECS;
using HCSortingCZP.Models;
using HCSortingCZP.Models.ECS;
using HCSortingCZP.Service;
using HCSortingCZP.Service.Models;
using HCSortingCZP.Utils;
using LanguageExt.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Mediator.Handlers.ECS
{
    internal class PalletArrivedCommandHandler
        (IHBECSServiceFactory hBECSServiceFactory,
        IMediator mediator) : IRequestHandler<PalletArrivedCommand, bool>
    {
        public async Task<bool> Handle(PalletArrivedCommand request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);
            var newEcsService = hBECSService.NewECSService;
            

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

            var service = hBECSServiceFactory.Create(request.Key);
            if (pallet.Type == Models.CZPallet.PalletType.组盘)
            {
                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.CheckNullTrayEnable)
                {
                    var result = await service.MesService.CheckNullTrayAsync(new CheckNullTrayRequest(request.Key)
                {
                    containerCode = pallet.PalletCode,
                });
                return await result.Match(async response =>
                {
                    if (response.Success) return true;
                    await mediator.Send(new WcsDataWarningBoxCommand($"工位：{request.Key}，站台：{request.InsideStationNum}，组盘托盘：{pallet.PalletCode}验证失败: {response.Message}"));
                    return false;
                },
                async ex =>
                {
                    await mediator.Send(new WcsDataWarningBoxCommand($"工位：{request.Key}，站台：{request.InsideStationNum}，组盘托盘：{pallet.PalletCode}验证异常: {ex.Message}"));
                    return false;
                });
                }
                
            }
            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.PalletArrivedEnable)
            {
                // if (pallet.Type == Models.CZPallet.PalletType.拆盘)
                // {
                //     if (MainStorage.Saves.ApiEnableTable.GRegionFXQueryEnable)
                //     {
                //         BatteryLevel[] batteryLevel = [BatteryLevel.A, BatteryLevel.B];
                //         for (int i = 0; i < 2; ++i)
                //         {
                //             await pallet.BindBattery(i, $"test{i:00}", true, batteryLevel[i]);

                //         }
                //     }
                // }


                    return true;
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
                    if (r.Result?.CellList != null)
                    {
                        foreach (var cell in r.Result.CellList)
                        {
                            var channelId = cell.Channel;
                            var type = cell.CellType switch
                            {
                                0 => Models.BatteryLevel.EMPTY,
                                1 => Models.BatteryLevel.OK,
                                2 => Models.BatteryLevel.NG,
                                3 => Models.BatteryLevel.Fake,
                                _ => Models.BatteryLevel.EMPTY // 这里需要 那个映射一下

                            };
                            await pallet.BindBattery(channelId - 1, cell.CellCode, true, type);
                        }
                    }

                    RuntimeStateStore.Save();

                    return true;    
                },
                Fail: async ex =>
                {
                    await mediator.Send(new WcsDataWarningBoxCommand($"工位：{request.Key},站台：{request.InsideStationNum}，托盘：{pallet.PalletCode}上报MES失败: {ex.Message}"));
                    return false;
                }
             );
            return a;
            //var data = await hBECSService.SendAsync<S102, E102>(new() { TrayNumber = pallet.PalletCode, TrayPosition = pallet.PalletId + 1, TrayType = pallet.OKNGType }, pallet.OutsideStationNum);
            //return data != null;// && data.ErrorCode == "0000";
        }
    }
}
