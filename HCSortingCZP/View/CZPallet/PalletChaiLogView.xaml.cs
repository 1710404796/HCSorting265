using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HCSortingCZP.Attritubes;
using HCSortingCZP.Data.Models;
using HCSortingCZP.ViewModel;

namespace HCSortingCZP.View.CZPallet;
/// <summary>
/// PalletChaiLogView.xaml 的交互逻辑
/// </summary>
[PowerView(PowerEnum.拆盘日志)]
public partial class PalletChaiLogView : Page, IDisposable
{
    private bool _disposed;

    public PalletChaiLogViewModel ViewModel
    {
        get;
    }
    public PalletChaiLogView(PalletChaiLogViewModel viewModel)
    {
        DataContext = this;
        ViewModel = viewModel;

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

    /// <summary>
    /// 托盘内容按钮点击事件，打开托盘详情窗口
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PalletContents_Click(object sender, RoutedEventArgs e)
    {
        var palletLog = (sender as FrameworkElement)!.Tag as PalletLog;

        new PalletDetailWindow(palletLog!).ShowDialog();

    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshContext();
    }
}
