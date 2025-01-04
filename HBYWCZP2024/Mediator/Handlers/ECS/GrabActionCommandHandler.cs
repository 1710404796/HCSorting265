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
                // 拆组盘开始 作业开始
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
                
                // 拆组盘结束。步骤较多
                // 1. 向FMS上报结果，这里感觉也需要加一下处理
                // 2. 向MES上报结果
                // 3. 向FMS上报组盘完成
                


                // TODO:上报数据结果权限添加

                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.UploadFmsDataEnable)
                {

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
                }
                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.UploadMesDataEnable)
                {
                    var mesRes = await hBECSService.MesService.UploadPalletResultAsync(new UploadPalletResultRequest(request.Key)
                    {
                        cellDataVOList = [.. pallet.Channels.Take(pallet.ChannelNum)
                    .Select((x, i) =>
                        new Celldatavolist
                        {
                            identification = x.Code,
                            locationBat = (i + 1).ToString()
                        })],
                        containerCode = pallet.PalletCode

                    }
                );

                    var mesdata = mesRes.Match(
                        Succ: s => s,
                        Fail: f =>
                        {
                            mediator.Send(new WcsDataWarningBoxCommand(f.Message)).Wait();
                            throw f;
                        }
                        );

                }
                // 作业结束
                var e106 = await hBECSService.SendAsync<S106, E106>(new() { 
                    TrayNumber = pallet.PalletCode, 
                    TrayPosition = pallet.PalletId + 1,
                    TrayType = pallet.OKNGType,
                    OutSideStationNum = pallet.OutsideStationNum.ToString(),
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
