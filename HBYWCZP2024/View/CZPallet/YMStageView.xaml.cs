using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HBYWCZP2024.View.CZPallet
{
    /// <summary>
    /// YMStageView.xaml 的交互逻辑
    /// </summary>
    public partial class YWStageView : UserControl
    {

        public YWStageView()
        {
            DataContext = this;
            //ViewModel = App.GetService<YWStageViewModel>();
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var station in MainStorage.Saves.Stage.Stations)
            {
                if (station.IsEnable)
                {
                    var view = new GongWeiView(station);
                    SetViewStyle(view);
                }


            }

        }

        private void SetViewStyle(GongWeiView view)
        {
            GongWeiSim.Children.Add(view);
            view.Width = 300;
            view.Height = 300;
            view.Margin = new Thickness(5, 10, 5, 10);
            view.Background = Brushes.Transparent;
            view.MouseDown += View_MouseDown;
            //view.ViewModel
        }

        private void View_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var obj = sender as GongWeiView;
            //var aa = new sta
            //{
            //    PalletViewModel = new PalletViewModel
            //    { Pallet = obj.ViewModel.DiaoDuStage }
            //};
            MainWindow.NageTo(new StationDetail(
                obj!.Station

            )
            );


        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in MainStorage.Saves.Stage.Stations)
            {
                item.ClearStage();
            }
        }
    }
}
