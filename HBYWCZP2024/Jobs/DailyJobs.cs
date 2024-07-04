using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
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
}
