using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinxUniverse.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HBYWCZP2024.Data;
using HBYWCZP2024.Data.Models;
using HBYWCZP2024.Models.CZPallet;

namespace HBYWCZP2024.ViewModel
{
    public class PalletLogExt : PalletLog
    {
        public PalletLogExt(PalletLog log)
        {
            BatteryInfo = log.BatteryInfo;
            PalletCode = log.PalletCode;
            ZuPanTime = log.ZuPanTime;
            Id = log.Id;
            Column = log.Column;
        }
        public bool IsSelect { get; set; }
    }
    public partial class PalletChaiLogViewModel(LinxContext context): ObservableRecipient
    {
        //public ObservableCollection<PalletLogViewModel> Log { get; set; } = [];
        [ObservableProperty] private ObservableCollection<PalletLogExt> _palletLogs = [];
        public DateTime StartTime { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndTime { get; set; } = DateTime.Today;

        public string Code { get; set; } = string.Empty;


        public void RefreshContext()
        {
            Search();
        }

        public bool IsNowLog(WarningLog log)
        {

            return log.WarningTime >= StartTime && log.WarningTime <= EndTime;
        }

        [RelayCommand]
        public void Export()
        {
            StringBuilder sb = new(1000);
            string[] titles = ["托盘编号", "组盘时间", "电池数量", "电池条码"];

            sb.AppendLine(string.Join(",", titles));

            foreach (var log in PalletLogs.Where(s => s.IsSelect))
            {
                var batteryCodes = PalletCsvExportHelper.FormatCodes(log.BatteryInfo.Select(s => s.BatteryCode));
                sb.AppendLine(
                    $"{PalletCsvExportHelper.Escape(log.PalletCode)}," +
                    $"{log.ZuPanTime:yyyy-MM-dd HH:mm:ss}," +
                    $"{log.ChannelCount}," +
                    $"{PalletCsvExportHelper.Escape(batteryCodes)}");

            }

            string fileName = $"RemoveLog/export-{FilenameHelper.FileName}.csv";
            System.IO.File.WriteAllBytes(fileName, Encoding.GetEncoding("gb2312").GetBytes(sb.ToString()));
            MessageBox.Show($"export to {fileName}");

        }
        [RelayCommand]
        public void Search()
        {
            var start = StartTime.Date;
            var endExclusive = EndTime.Date.AddDays(1);

            lock (context)
            {
                IQueryable<PalletLog> query = context.PalletLogs
                    .AsNoTracking()
                    .Where(s => s.PalletType == PalletType.拆盘 || s.PalletType == PalletType.筛选);

                query = query.Where(s => s.ZuPanTime >= start && s.ZuPanTime < endExclusive);

                if (!string.IsNullOrWhiteSpace(Code))
                {
                    query = query.Where(s => s.PalletCode.Contains(Code));
                }

                PalletLogs = new(query
                    .OrderByDescending(s => s.Id)
                    // .Take(3000)   暂时屏蔽显示条数
                    .AsEnumerable()
                    .Select(s => new PalletLogExt(s)));
            }
        }
    }
}
