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
using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Data.Models;
using HBYWCZP2024.ViewModel;

namespace HBYWCZP2024.View;
/// <summary>
/// PalletChaiLogView.xaml 的交互逻辑
/// </summary>
[PowerView(PowerEnum.拆盘日志)]
public partial class PalletChaiLogView : Page
{
    public PalletChaiLogViewModel ViewModel
    {
        get;
    }
    public PalletChaiLogView()
    {
        DataContext = this;
        ViewModel = App.GetService<PalletChaiLogViewModel>();

        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var palletLog = (sender as FrameworkElement)!.Tag as PalletLog;

        new PalletDetailWindow(palletLog!).ShowDialog();

    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshContext();
    }
}
