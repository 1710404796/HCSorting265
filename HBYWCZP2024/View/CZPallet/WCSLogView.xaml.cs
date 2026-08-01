using HBYWCZP2024.Attritubes;
using HBYWCZP2024.ViewModel.CZPallet;
using System.Windows;
using System.Windows.Controls;


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
            Unloaded += (s, e) => ViewModel?.Dispose(); // 页面切走时释放DbContext，避免每次切换累积一个孤立实例
    } // log页面格式化？


    private async void MesLogView_OnLoaded(object sender, RoutedEventArgs e)
    {
       await  ViewModel.RefreshContext();
    }
}