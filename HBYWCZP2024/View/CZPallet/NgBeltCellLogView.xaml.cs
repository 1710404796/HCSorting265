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
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshContext();
    }
}
