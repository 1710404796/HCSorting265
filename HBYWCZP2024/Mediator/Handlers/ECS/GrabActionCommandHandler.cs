using HBYWCZP2024.Data;
using HBYWCZP2024.Data.Models;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Models.CZPallet;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Service;
using HBYWCZP2024.Service.Models;
using HBYWCZP2024.Utils;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    public class GrabActionCommandHandler(IHBECSServiceFactory hBECSServiceFactory, 
        IMediator mediator, LinxContext context, ILogger<GrabActionCommandHandler> logger) : IRequestHandler<GrabActionCommand, bool>
    {
        public async Task<bool> Handle(GrabActionCommand request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);
            var pallet = CZPUtils.GetPalletByOutsideNum(request.Key, request.StationNum);
            bool isCodeRuleNG(string s) => s.StartsWith("noread", StringComparison.CurrentCultureIgnoreCase) || string.IsNullOrWhiteSpace(s);
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
            //if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.GrabActionRemoveEnable)
            //{
            //    if (pallet.Type == Models.CZPallet.PalletType.拆盘 || pallet.Type == Models.CZPallet.PalletType.筛选)
            //    {
            //        return true;
            //    }

            //}
            if (request.Start)
            {
                // 拆组盘开始 作业开始
                var data = await hBECSService.SendAsync<S104, E104>(new() { TrayNumber = pallet.PalletCode, TrayPosition = pallet.PalletId + 1, TrayType = pallet.OKNGType }, pallet.OutsideStationNum);

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
                        TrayType = pallet.OKNGType,
                        MaterialId = MainStorage.Saves.KeysSave[request.Key].MaterialCode,
                        BatteryCount = pallet.ChannelNum,
                        BatteryBarcodes = pallet.Channels.Take(pallet.ChannelNum)
                        .Select(x => isCodeRuleNG(x.Code) ? "" : x.Code)
                        //.Select(x => x.Code)
                        .ToList()
                    }, pallet.OutsideStationNum);

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
                //if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.UploadMesDataEnable 

                //    //&& 
                //    //(
                //    //MainStorage.Saves.KeysSave[request.Key].Stage.IncomingBelt.IsEnable
                //    //|| MainStorage.Saves.KeysSave[request.Key].Stage.StrippingBelt.IsEnable
                //    //)
                //    )
                //{
                //    var mesRes = await hBECSService.MesService.UploadPalletResultAsync(new UploadPalletResultRequest(request.Key)
                //    {
                //        cellDataVOList = [.. 
                //            pallet.Channels.Take(pallet.ChannelNum)
                //            .Select((x, i) =>
                //                new Celldatavolist
                //                {
                //                    identification = x.Code,
                //                    locationBat = (i + 1).ToString()
                //                })
                //            .Filter(s => !string.IsNullOrWhiteSpace(s.identification))],
                //        containerCode = pallet.PalletCode,
                //        actionType =
                //            pallet.Type switch
                //            {
                //                Models.CZPallet.PalletType.组盘 => "IN",
                //                Models.CZPallet.PalletType.拆盘 => "OUT",
                //                Models.CZPallet.PalletType.筛选 => "OUT",
                //                _ => "IN"
                //            }

                //    }
                //);

                //    var mesdata = mesRes.Match(
                //        Succ: s => s,
                //        Fail: f =>
                //        {
                //            mediator.Send(new WcsDataWarningBoxCommand(f.Message)).Wait();
                //            throw f;
                //        }
                //        );

                //}
                // 作业结束
                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.GrabActionRemoveEnable)
                {
                    var e106 = await hBECSService.SendAsync<S106, E106>(new()
                    {
                        TrayNumber = pallet.PalletCode,
                        TrayPosition = pallet.PalletId + 1,
                        TrayType = pallet.OKNGType,
                        OutSideStationNum = pallet.OutsideStationNum.ToString(),
                        PalletStatus = request.isSpecial ? PalletStatus.不满 : PalletStatus.满盘,
                        //PalletStatus = pallet.Channels.Take(pallet.ChannelNum).All(s => s.BatteryLevel != Models.BatteryLevel.None && s.BatteryLevel != Models.BatteryLevel.EMPTY) ? PalletStatus.满盘 : PalletStatus.不满
                    }, pallet.OutsideStationNum);
                    if (e106 == null)
                    {
                        await mediator.Send(new WcsDataWarningBoxCommand("ECS通讯异常"));
                        return false;
                    }
                    if (e106.DeviceStatus != DeviceStatus2.OK)
                    {
                        await mediator.Send(new WcsDataWarningBoxCommand($"错误码: {e106.ErrorCode}"));
                        return false;
                    }
                }


            }
        //else
        //{
        //    throw new Exception("托盘类型错误");
        //}

        // TODO: 结束要用106命令


        SaveLog:

            if (!request.Start && pallet.Type == PalletType.组盘)
            {
                try
                {

                
                // 存一下托盘日志
                var palletlog = new PalletLog
                {
                    PalletCode = pallet.PalletCode,
                    BatteryInfo = pallet.Channels.Select(s => new BatteryInfo
                    {
                        BatteryCode = s.Code,
                        BatteryLevel = s.BatteryLevel,
                    }).Take(pallet.ChannelNum).ToList(),
                    Column = pallet.Column,
                    PalletType = pallet.Type

                };

                context.Add(palletlog);

                await context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {

                    logger.LogError(ex, "组盘日志发生错误");
                }
            }

            return true;

        }
    }
}
