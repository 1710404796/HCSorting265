using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HBYWCZP2024.Data;
using HBYWCZP2024.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;


namespace HBYWCZP2024.ViewModel.CZPallet

{
    public partial class WCSLogViewModel : ObservableRecipient, IDisposable
    {
        private readonly LinxContext _context; // DI注入，由Dispose释放
        private bool _disposed;

        public WCSLogViewModel(LinxContext context)
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
            _context?.Dispose();
        }

        [ObservableProperty] private ObservableCollection<WcsLog> _wcsLogs = [];
        private IEnumerable<WcsLog> _logs = [];

        public DateTime StartTime { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndTime { get; set; } = DateTime.Today.AddDays(1);

        public string SelectName { get; set; } = "(全部)";

        public WcsLog SelectLog { get; set; }
        public string[] Apinames { get; } = ["(全部)", 
        //"getCellModelAsync",
        //"getCellGradeAsync",
        //"getShopNameAsync",
        //"UpMESCartonDataAsync",
        //"UpMESBoxtDataAsync",
        //"getIDCardCheckAsync",
        //"CheckCartonDataAsync",
        //"CheckMESBoxtDataAsync",
            "pre/sorting/query",
            "equipment/assembly/logistics/line",
            "assemble/dismantle/info/group/disk/iface",
            "formation/section/arrival",
            "in/station/process/currency",
    ];

        public void AddLog(WcsLog log)
        {
            _logs = _logs.Prepend(log);
            if (IsNowLog(log))
            {
                Dispatcher.CurrentDispatcher.Invoke(() => WcsLogs.Insert(0, log));
            }

        }

        public async Task RefreshContext(CancellationToken cancellationToken = default)
        {
            _logs = await _context.WcsLogs.AsNoTracking().OrderByDescending(s => s.Id).Take(100).ToListAsync(cancellationToken);
            Search(cancellationToken);



        }

        public bool IsNowLog(WcsLog log)
        {
            if (SelectName != "(全部)")
            {
                if (log.ApiName != SelectName) return false;
            }

            return log.RequestTime >= StartTime && log.ResponseTime <= EndTime;
        }

        [RelayCommand]
        public void Search() => Search(CancellationToken.None);

        private void Search(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var afterFilter = _context.WcsLogs.AsNoTracking().OrderByDescending(s => s.Id)
                .Where(s => s.RequestTime >= StartTime && s.RequestTime <= EndTime);
            if (SelectName != "(全部)")
            {
                afterFilter = afterFilter.Where(s => s.ApiName == SelectName);
            }
            WcsLogs = new(
                afterFilter.Take(100).ToList()
            );
        }
    }
}
