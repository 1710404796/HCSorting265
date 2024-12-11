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
using System.Windows.Shapes;
using HBYWCZP2024.Data.Models;

namespace HBYWCZP2024.View
{
    /// <summary>
    /// PalletDetailWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PalletDetailWindow : Window
    {
        //public PalletLog PalletLog { get; set; }
        public PalletDetailWindow(PalletLog palletLog)
        {
            InitializeComponent();
            System.Collections.ObjectModel.ObservableCollection<Models.StationChannel> stationChannels = new(palletLog.BatteryInfo.Select(s => new Models.StationChannel() { Code = s.BatteryCode, BatteryLevel = s.BatteryLevel }).ToList());
            palletv.Station = new Models.XYLStation
            {
                Channels = stationChannels
                ,
                Column = palletLog.Column,
                ChannelNum = stationChannels.Count
            };
        } 
    }
}
