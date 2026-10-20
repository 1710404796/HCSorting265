using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HCSortingCZP.Data;
using HCSortingCZP.Data.Models;
using Microsoft.EntityFrameworkCore;


namespace HCSortingCZP.ViewModel.CZPallet;

public partial class WarningLogViewModel : ObservableRecipient
{
    private readonly IDbContextFactory<LinxContext> _dbContextFactory;
    [ObservableProperty] private ObservableCollection<WarningLog> _mesLogs = [];
    private IEnumerable<WarningLog> _logs = [];
    
    public DateTime StartTime { get; set; } = DateTime.Today.AddDays(-30);
    public DateTime EndTime { get; set; } = DateTime.Today.AddDays(1);

    public WarningLogViewModel(IDbContextFactory<LinxContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    
    private IEnumerable<WarningLog> GetWarnLogs()
    {
        using var ctx = _dbContextFactory.CreateDbContext();
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
