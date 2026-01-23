using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.PLC;
using HBYWCZP2024.Service;
using HBYWCZP2024.ViewModel;
using LinxUniverse.OPCUA;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HBYWCZP2024.Jobs
{
    class DailyJobs() : IJob
    {
        /// <summary>
        /// 每日执行任务
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Execute(IJobExecutionContext context)
        {
            MainStorage.SaveManager.BackUp();

            var files = new DirectoryInfo("logs").GetFiles();

            foreach (var file in files)
            {
                if (DateTime.Now - file.CreationTime > TimeSpan.FromDays(MainStorage.Saves.LogDeleteDay))
                {
                    File.Delete(file.FullName);
                }
            }
            return Task.CompletedTask;
        }
    }

    class SaveJobs() : IJob
    {
        /// <summary>
        /// 每日执行任务
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Execute(IJobExecutionContext context)
        {
            MainStorage.SaveManager.Save();
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 每分钟上报一次ECS
    /// </summary>
    //internal class StatusJobs(PLCTaskService<HBYWPLCContext> hBYWPLCContext, EquipStatusReportCommand request, NewECSService newECSService) : IJob
    //{

    //    [JsonIgnore]
    //    static Lazy<ILogger> _lazyLogger = new Lazy<ILogger>(App.GetService<ILogger<MainViewModel>>);
    //    [JsonIgnore]
    //    static ILogger _logger => _lazyLogger.Value;

    //    public async Task Execute(IJobExecutionContext context)
    //    {
    //        try
    //        {
    //            var res = await newECSService.EquipStatusReportAsync([new EquipStatusRequest(request.Key)
    //        {

    //            StatusEnum = (EquipStatus)hBYWPLCContext.Context.DB500.Mode,
    //        }]);
    //            //MainStorage.WcsOnline = res != null;
    //            _logger.LogInformation($"调度设备状态响应: {res}");
    //        }
    //        catch (Exception)
    //        {
    //            //MainStorage.WcsOnline = false;
    //            _logger.LogError("调度设备状态异常");

    //        }
    //    }
    //}
}
