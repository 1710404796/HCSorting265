using HBYWCZP2024.Attritubes;
using System.Windows;
using System.Windows.Controls;

using XYLCZP2024.ViewModel;

namespace HBYWCZP2024.View.CZPallet;
[PowerView(PowerEnum.WCS交互日志)]

public partial class WCSLogView : Page
{
    public WCSLogViewModel ViewModel { get; }
    
    public WCSLogView()
    {
        DataContext = this;
        ViewModel = App.GetService<WCSLogViewModel>();
        InitializeComponent();
    } // log页面格式化？


    private async void MesLogView_OnLoaded(object sender, RoutedEventArgs e)
    {
       await  ViewModel.RefreshContext();
    }
}