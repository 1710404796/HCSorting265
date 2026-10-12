using HBYWCZP2024.Models;
using System;
using System.Windows.Controls;

namespace HBYWCZP2024.View.CZPallet
{
    /// <summary>
    /// GongWeiView.xaml 的交互逻辑
    /// </summary>
    public partial class GongWeiView : UserControl, IDisposable
    {
        //public GongWeiViewModel ViewModel { get; init; }

        public XYLStation Station { get; set; }
        private bool _disposed;


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

        /// <summary>
        /// 释放内部 PalletView 以及整棵控件树的绑定和 DataContext。
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            Pallet1.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
