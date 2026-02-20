using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;

//using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Models;


//using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Models.CZPallet;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Service;
using HBYWCZP2024.Service.Models;
using HBYWCZP2024.Utils;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    internal class GrabCommandHandler(IHBECSServiceFactory hBECSServiceFactory, IMediator mediator) : IRequestHandler<GrabCommand, bool>
    {
        public async Task<bool> Handle(GrabCommand request, CancellationToken cancellationToken)
        {
            var hBECSService = hBECSServiceFactory.Create(request.Key);
            var newEcsService = hBECSService.NewECSService;
            var newFMSService = hBECSService.NewFMSService;
            if (request.GrabInfo.Infos.Length <= 0) return true;

            if (!string.IsNullOrWhiteSpace(request.GrabInfo.From.ServerIp) && request.GrabInfo.From.GetFromServer)
            {
                await mediator.Send(new FetchStationCommand(request.GrabInfo.From.OutsideStationNum));
            }
            if (!string.IsNullOrWhiteSpace(request.GrabInfo.To.ServerIp) && request.GrabInfo.To.GetFromServer)
            {
                await mediator.Send(new FetchStationCommand(request.GrabInfo.To.OutsideStationNum));
            }

            if (request.GrabInfo.From is Pallet from)
            {
                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.UploadFMSChaiHuanpanEnable
                    )
                {
                    var codes = request.GrabInfo.Infos
                        .Map(s => (s.FromChannel, s.Code))
                        .ToArray();
                    //var data = await hBECSService.SendAsync<S104, E104>(
                    //    new S104() { 
                    //        TrayNumber = from.PalletCode, 
                    //        TrayPosition = from.PalletId + 1, 
                    //        TrayType = from.OKNGType,
                    //        BatteryCount = codes.Length,
                    //        Battery = codes
                    //    }
                    //    , from.OutsideStationNum
                    //    );

                    var data = await newFMSService.MpSplitAsync(new MpSplitRequest(request.Key)
                    {
                        trayCode = from.PalletCode,
                        Cells = [..
                            request.GrabInfo.Infos
                            .Map(s => new CellInfo1{ CellCode = s.Code, Channel = s.FromChannel})
                            .Filter(s => !string.IsNullOrWhiteSpace(s.CellCode))]
                    });

                    if (data == null)
                    {
                        //await mediator.Send(new WcsDataWarningBoxCommand("ECS通讯异常"));
                        return false;
                    }
                    //if (data.DeviceStatus != DeviceStatus2.OK)
                    //{
                    //    await mediator.Send(new WcsDataWarningBoxCommand($"错误码: {data.ErrorCode}"));
                    //    return false;
                    //}
                }
                // 拆上报
                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.UploadMesDataEnable
                    )
                {
                    var mesRes = await hBECSService.MesService.UploadPalletResultAsync(new UploadPalletResultRequest(request.Key)
                    {
                        cellDataVOList = [..
                            request.GrabInfo.Infos
                            .Map(s => new Celldatavolist{ identification = s.Code, locationBat = s.FromChannel.ToString()})
                            .Filter(s => !string.IsNullOrWhiteSpace(s.identification))],
                        containerCode = from.PalletCode,
                        actionType =
                            from.Type switch
                            {
                                PalletType.组盘 => "OUT",
                                PalletType.拆盘 => "OUT",
                                PalletType.筛选 => "OUT",
                                _ => "OUT"
                            }

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
            }
            if (request.GrabInfo.To is Pallet to)
            {
                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.UploadMesDataEnable
                   )
                {
                    var mesRes = await hBECSService.MesService.UploadPalletResultAsync(new UploadPalletResultRequest(request.Key)
                    {
                        cellDataVOList = [..
                            request.GrabInfo.Infos
                            .Map(s => new Celldatavolist{ identification = s.Code, locationBat = s.ToChannel.ToString()})
                            .Filter(s => !string.IsNullOrWhiteSpace(s.identification))],
                        containerCode = to.PalletCode,
                        actionType =
                   to.Type switch
                   {
                       PalletType.组盘 => "IN",
                       PalletType.拆盘 => "IN",
                       PalletType.筛选 => "IN",
                       _ => "IN"
                   }

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
                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.HighTempEnable)
                {
                    //if (MainStorage.Saves.Stage.IncomingBelt.IsEnable && to.OKNGType == TrayType.Engineering)
                    if (MainStorage.Saves.Stage.IncomingBelt.IsEnable && to.OKNGType == TrayType.空托盘)
                    {

                        var service = hBECSService;
                        // 验证器
                        // 高温静置入站
                        // 可能非ng盘上报 现在不管他
                        var rss = request.GrabInfo.Infos
                            .Map(c => c.Code)

                                    .AsParallel()
                                    .Map(s =>
                                    {
                                        return service.MesService.HighTempOnlineAsync(new HighTempOnlineRequest(request.Key)
                                        {
                                            identification = s
                                        }).Result;
                                    });

                        //bool CheckAll()

                        if (!rss.All(s => s.IsSuccess))
                        {
                            var message = string.Join("\n",
                                rss
                                .Filter(s => !s.IsSuccess)
                                .Map(s => s.Match(Succ: su => "", Fail: fa => fa.Message)));
                            await mediator.Send(new WcsDataWarningBoxCommand("高温进站Mes返回错误信息: " + message));
                            throw new Exception(message);
                        }
                    }
                }

            }

            if (request.GrabInfo.To is TrayCupStation toTrayCup)
            {
                var trayCupInfos = request.GrabInfo.Infos
                    .Filter(s => !string.IsNullOrWhiteSpace(s.TrayCupCode))
                    .Filter(s => !string.IsNullOrWhiteSpace(s.Code))
                    .ToArray();

                var bindings = trayCupInfos
                    .Select(info => new CupBatteryModel(info.TrayCupCode, info.Code, info.ToChannel))
                    .ToArray();

                if (bindings.Length > 0)
                {
                    await mediator.Send(new SaveToSqlCommand(
                        MainStorage.Saves.ExDbTableName,
                        MainStorage.Saves.KeysSave[request.Key].EquipNum,
                        bindings,
                        "OK",
                        Key: request.Key
                    ));
                }
            }



            return true;
        }
    }
}
