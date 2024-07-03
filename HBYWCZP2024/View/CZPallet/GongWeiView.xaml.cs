using HBYWCZP2024.Models;
using System.Windows.Controls;

namespace HBYWCZP2024.View.CZPallet
{
    /// <summary>
    /// GongWeiView.xaml 的交互逻辑
    /// </summary>
    public partial class GongWeiView : UserControl
    {
        //public GongWeiViewModel ViewModel { get; init; }

        public XYLStation Station { get; set; }


        public string Title { get; set; }
        public GongWeiView(XYLStation station)
        {
            Station = station;
            DataContext = this;
            //ViewModel = App.GetService<GongWeiViewModel>();
            Title = $"调度号{Station.InsideNum} {Station.Name} 工位号{Station.OutsideStationNum}";

            InitializeComponent();
            Pallet1.Station = Station;

        }
    }
}
