using HCSortingCZP.Mediator.Commands;
using HCSortingCZP.Mediator.ECS;
using HCSortingCZP.Data;
using HCSortingCZP.Data.Models;

//using HCSortingCZP.Mediator.Queries;
using HCSortingCZP.Models;


//using HCSortingCZP.Mediator.Queries;
using HCSortingCZP.Models.CZPallet;
using HCSortingCZP.Models.ECS;
using HCSortingCZP.Service;
using HCSortingCZP.Service.Models;
using HCSortingCZP.Utils;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Mediator.Handlers.ECS
{
    internal class GrabCommandHandler(IHBECSServiceFactory hBECSServiceFactory, IMediator mediator, IDbContextFactory<LinxContext> dbContextFactory, ILogger<GrabCommandHandler> logger) : IRequestHandler<GrabCommand, bool>
    {
        public async Task<bool> Handle(GrabCommand request, CancellationToken cancellationToken)
        {
            var hBECSService = hBECSServiceFactory.Create(request.Key);
            var newEcsService = hBECSService.NewECSService;
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
                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.UploadFMSChaiHuanpanEnable)
                {
                    var pickedInfos = request.GrabInfo.Infos
                        .Filter(s => !string.IsNullOrWhiteSpace(s.Code))
                        .ToArray();

                    foreach (var pickedInfo in pickedInfos)
                    {
                        var uploadRes = await mediator.Send(new YZScreenUploadCommand(
                            request.Key,
                            from.PalletCode,
                            request.GrabInfo.From.OutsideStationNum.ToString(),
                            [pickedInfo.FromChannel]),
                            cancellationToken);

                        if (!uploadRes)
                        {
                            logger.LogWarning(
                                "筛选结果按单电芯上传失败，托盘：{PalletCode}，电芯：{CellCode}，通道：{Channel}",
                                from.PalletCode,
                                pickedInfo.Code,
                                pickedInfo.FromChannel);
                            return false;
                        }
                    }
                }
                // 拆上报
                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.UploadMesDataEnable)
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
                    });
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
                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.UploadMesDataEnable)
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
                    });
                    var mesdata = mesRes.Match(
                        Succ: s => s,
                        Fail: f =>
                        {
                            mediator.Send(new WcsDataWarningBoxCommand(f.Message)).Wait();
                            throw f;
                        });
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
                        MainStorage.Saves.KeysSave[request.Key].DeviceCode,
                        bindings,
                        "OK",
                        Key: request.Key
                    ));
                }
            }

            if (request.GrabInfo.To.InsideNum == MainStorage.Saves.Stage.NgBelt.InsideNum)
            {
                var now = DateTime.Now;
                var logs = request.GrabInfo.Infos
                    .Filter(s => !string.IsNullOrWhiteSpace(s.Code))
                    .Map(s => new NgBeltCellLog
                    {
                        RecordTime = now,
                        Key = request.Key,
                        FromInsideNum = request.GrabInfo.From.InsideNum,
                        FromStationName = request.GrabInfo.From.Name,
                        ToInsideNum = request.GrabInfo.To.InsideNum,
                        ToStationName = request.GrabInfo.To.Name,
                        CellCode = s.Code,
                        FromChannel = s.FromChannel,
                        ToChannel = s.ToChannel,
                    })
                    .ToArray();

                if (logs.Length > 0)
                {
                    try
                    {
                        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                        context.NgBeltCellLogs.AddRange(logs);
                        await context.SaveChangesAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "写入NG拉带电芯记录失败");
                    }
                }
            }



            return true;
        }
    }
}
