using HBYWCZP2024.Attritubes;
using HBYWCZP2024.ViewModel.CZPallet;
using System.Windows;
using System.Windows.Controls;


namespace HBYWCZP2024.View.CZPallet;
[PowerView(PowerEnum.报警日志)]
public partial class WarningLogView : Page
{
    public WarningLogViewModel ViewModel { get; }
    public WarningLogView()
    {
        ViewModel = App.GetService<WarningLogViewModel>();
        DataContext = this;
        InitializeComponent();
    }

    private void WarningLogView_OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshContext();
    }
}