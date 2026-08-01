using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HBYWCZP2024.Data;
using HBYWCZP2024.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HBYWCZP2024.ViewModel.CZPallet;

public partial class NgBeltCellLogViewModel : ObservableRecipient, IDisposable
{
        private readonly LinxContext _context; // DI注入，由Dispose释放

    public NgBeltCellLogViewModel(LinxContext context)
    {
        _context = context;
    }

        // 页面切走时释放DbContext，防止内存泄漏
    public void Dispose()
    {
        _context?.Dispose();
    }

    [ObservableProperty] private ObservableCollection<NgBeltCellLog> _logs = [];

    public DateTime StartTime { get; set; } = DateTime.Today.AddDays(-30);
    public DateTime EndTime { get; set; } = DateTime.Today.AddDays(1);

    public string Code { get; set; } = string.Empty;

    public void RefreshContext()
    {
        Search();
    }

    [RelayCommand]
    public void Search()
    {
        IQueryable<NgBeltCellLog> query = _context.NgBeltCellLogs
            .AsNoTracking()
            .OrderByDescending(s => s.Id)
            .Where(s => s.RecordTime >= StartTime && s.RecordTime <= EndTime);

        if (!string.IsNullOrWhiteSpace(Code))
        {
            query = query.Where(s => s.CellCode.Contains(Code));
        }

        Logs = new ObservableCollection<NgBeltCellLog>(query.Take(200));
    }
}
