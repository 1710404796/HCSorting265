using HBYWCZP2024.Attritubes;
using HBYWCZP2024.ViewModel.CZPallet;
using System;
using System.Windows;
using System.Windows.Controls;

namespace HBYWCZP2024.View.CZPallet;

public partial class NgBeltCellLogView : Page, IDisposable
{
    public NgBeltCellLogViewModel ViewModel { get; }
    private bool _disposed;

    public NgBeltCellLogView()
    {
        DataContext = this;
        ViewModel = App.GetService<NgBeltCellLogViewModel>();
        InitializeComponent();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        ViewModel.Dispose();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshContext();
    }
}
