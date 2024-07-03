using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return Task.CompletedTask;
        }
    }
}
