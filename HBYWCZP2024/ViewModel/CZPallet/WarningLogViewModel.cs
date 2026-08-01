using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HBYWCZP2024.Data;
using HBYWCZP2024.Data.Models;
using Microsoft.EntityFrameworkCore;


namespace HBYWCZP2024.ViewModel.CZPallet;

public partial class WarningLogViewModel : ObservableRecipient
{
    [ObservableProperty] private ObservableCollection<WarningLog> _mesLogs = [];
    private IEnumerable<WarningLog> _logs = [];
    
    public DateTime StartTime { get; set; } = DateTime.Today.AddDays(-30);
    public DateTime EndTime { get; set; } = DateTime.Today.AddDays(1);
    
    // 方法内创建短生命周期DbContext，用完即释放，避免Singleton持有常驻DbContext
    private static IEnumerable<WarningLog> GetWarnLogs()
    {
        using var ctx = App.GetService<LinxContext>(); // 局部using，函数返回前自动Dispose
        return ctx.WarningLogs.AsNoTracking().OrderByDescending(s => s.Id).Take(100).ToList();
    }

    public void RefreshContext()
    {
        _logs = GetWarnLogs();
        Search();
    }

    public bool IsNowLog(WarningLog log)
    {

        return log.WarningTime >= StartTime && log.WarningTime <= EndTime;
    }
    
    [RelayCommand]
    public void Search()
    {
        var afterFilter = _logs.Where(s => s.WarningTime >= StartTime && s.WarningTime <= EndTime);
        MesLogs = new (
            afterFilter
        );
    }
}