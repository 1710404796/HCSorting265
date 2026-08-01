using HBYWCZP2024.Attritubes;
using HBYWCZP2024.ViewModel.CZPallet;
using System.Windows;
using System.Windows.Controls;

namespace HBYWCZP2024.View.CZPallet;

public partial class NgBeltCellLogView : Page
{
    public NgBeltCellLogViewModel ViewModel { get; }

    public NgBeltCellLogView()
    {
        DataContext = this;
        ViewModel = App.GetService<NgBeltCellLogViewModel>();
        InitializeComponent();
            Unloaded += (s, e) => ViewModel?.Dispose(); // 页面切走时释放DbContext，避免每次切换累积一个孤立实例
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshContext();
    }
}
