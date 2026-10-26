using HCSortingCZP.Mediator.ECS;
using HCSortingCZP.Models;
using HCSortingCZP.Models.CZPallet;
using HCSortingCZP.Service;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HCSortingCZP.Jobs
{
    /// <summary>
    /// 每小时上报所有工位运行状态
    /// </summary>
    //internal class EquipStatusReportJobs(IMediator mediator, ILogger<EquipStatusReportJobs> logger) : IJob
    //{
    //    public async Task Execute(IJobExecutionContext context)
    //    {
    //        if (MainStorage.Saves.KeysSave == null || MainStorage.Saves.KeysSave.Count == 0)
    //        {
    //            logger.LogWarning("未找到可用产线配置，跳过工位状态上报。");
    //            return;
    //        }

    //        foreach (var (key, stageMono) in MainStorage.Saves.KeysSave)
    //        {
    //            var stage = stageMono?.Stage;
    //            if (stage == null)
    //            {
    //                continue;
    //            }

    //            foreach (var station in stage.Stations.Filter(s => s is Pallet))
    //            {
    //                try
    //                {
    //                    if (!station.IsEnable)
    //                    {
    //                        continue;
    //                    }

    //                    var status = EquipStatus.I;
    //                    if (station.TempData.TryGetValue("EquipStatus", out var value) && value is EquipStatus equipStatus)
    //                    {
    //                        status = equipStatus;
    //                    }

    //                    await mediator.Send(new EquipStatusReportCommand(key, station.InsideNum, status));
    //                }
    //                catch (Exception ex)
    //                {
    //                    logger.LogError(ex, "工位状态上报失败: {Key}, InsideNum={InsideNum}", key, station.InsideNum);
    //                }
    //            }
    //        }
    //    }
    //}
}
