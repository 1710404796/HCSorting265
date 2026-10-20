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
using HCSortingCZP.Data;
using HCSortingCZP.Data.Models;
using HCSortingCZP.Models.CZPallet;

namespace HCSortingCZP.ViewModel
{
    public partial class PalletLogViewModel : ObservableRecipient, IDisposable
    {
        private const int PageSize = 200;
        private readonly LinxContext _context; // DI注入，由Dispose释放
        private bool _disposed;

        public PalletLogViewModel(LinxContext context)
        {
            _context = context;
        }

        // 页面切走时释放DbContext，防止内存泄漏
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            PalletLogs = [];
            PageIndex = 1;
            TotalCount = 0;
            _context?.Dispose();
        }

        //public ObservableCollection<PalletLogViewModel> Log { get; set; } = [];
        [ObservableProperty] private ObservableCollection<PalletLogExt> _palletLogs = [];
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
        [NotifyPropertyChangedFor(nameof(HasNextPage))]
        [NotifyPropertyChangedFor(nameof(PageSummary))]
        private int _pageIndex = 1;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNextPage))]
        [NotifyPropertyChangedFor(nameof(PageSummary))]
        private int _totalCount;
        [ObservableProperty] private string _pageInput = "1";
        public DateTime StartTime { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndTime { get; set; } = DateTime.Today;

        public string Code { get; set; } = string.Empty;

        public bool HasLoaded { get; private set; }

        public bool HasPreviousPage => PageIndex > 1;

        public bool HasNextPage => PageIndex * PageSize < TotalCount;

        public string PageSummary =>
            $"第 {PageIndex}/{Math.Max(1, (TotalCount + PageSize - 1) / PageSize)} 页，共 {TotalCount} 条";

        public void RefreshContext()
        {
            if (HasLoaded)
            {
                return;
            }

            HasLoaded = true;
            SearchPage(PageIndex);
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

            string fileName = $"InsertLog/export-{FilenameHelper.FileName}.csv";
            System.IO.File.WriteAllBytes(fileName, Encoding.GetEncoding("gb2312").GetBytes(sb.ToString()));
            MessageBox.Show($"export to {fileName}");

        }
        [RelayCommand]
        public void NextPage() => SearchPage(PageIndex + 1);

        [RelayCommand]
        public void PreviousPage() => SearchPage(PageIndex - 1);

        [RelayCommand]
        public void Jump()
        {
            if (int.TryParse(PageInput, out var page) && page > 0)
            {
                SearchPage(page);
            }
        }

        [RelayCommand]
        public void Search() => SearchPage(1);

        private void SearchPage(int page)
        {
            if (_disposed)
            {
                return;
            }

            var start = StartTime.Date;
            var endExclusive = EndTime.Date.AddDays(1);

            lock (_context)
            {
                IQueryable<PalletLog> query = _context.PalletLogs
                    .AsNoTracking()
                    .Where(s => s.PalletType == PalletType.组盘);

                query = query.Where(s => s.ZuPanTime >= start && s.ZuPanTime < endExclusive);

                if (!string.IsNullOrWhiteSpace(Code))
                {
                    query = query.Where(s => s.PalletCode.Contains(Code));
                }

                TotalCount = query.Count();
                var totalPages = Math.Max(1, (TotalCount + PageSize - 1) / PageSize);
                if (page < 1)
                {
                    page = 1;
                }

                if (page > totalPages)
                {
                    page = totalPages;
                }

                PalletLogs = new(query
                    .OrderByDescending(s => s.Id)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .AsEnumerable()
                    .Select(s => new PalletLogExt(s)));

                PageIndex = page;
                PageInput = page.ToString();
            }
        }
    }
}
