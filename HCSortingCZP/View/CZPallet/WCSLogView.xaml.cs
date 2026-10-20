using HCSortingCZP.Attritubes;
using HCSortingCZP.ViewModel.CZPallet;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;


namespace HCSortingCZP.View.CZPallet;
[PowerView(PowerEnum.WCS交互日志)]

public partial class WCSLogView : Page, IDisposable
{
    public WCSLogViewModel ViewModel { get; }
    private bool _disposed;
    private CancellationTokenSource? _loadCts;
    
    public WCSLogView()
    {
        DataContext = this;
        ViewModel = App.GetService<WCSLogViewModel>();
        InitializeComponent();
    } // log页面格式化？

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;
        ViewModel.Dispose();
    }

    private async void MesLogView_OnLoaded(object sender, RoutedEventArgs e)
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new CancellationTokenSource();

        try
        {
            await ViewModel.RefreshContext(_loadCts.Token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
