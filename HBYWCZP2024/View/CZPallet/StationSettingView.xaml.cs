using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Models.CZPallet;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Utils;
using HBYWCZP2024.ViewModel;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;


namespace HBYWCZP2024.View.CZPallet
{
    /// <summary>
    /// DiaosuStage.xaml 的交互逻辑 用于配置工位号对饮关系
    /// </summary>
    [PowerView(PowerEnum.调度号工位表)]
    public partial class StationSettingView : Window
    {
        private const int lent = 14;

        TextBox[] stationName;// = new TextBox[lent];
        TextBox[] stationInName;// = new TextBox[lent];
        TextBox[] rfidReaderIP;// = new TextBox[lent];
        TextBox[] StationNum;// = new TextBox[lent];
        TextBox[] channelNum;// = new TextBox[lent];
        TextBox[] columnNum;// = new TextBox[lent];
        ComboBox[] ECSTypeBoxs;// = new ComboBox[lent];
        ComboBox[] comboBoxes;// = new ComboBox[lent];
        ComboBox[] OKNGBoxes;// = new ComboBox[lent];
        CheckBox[] isStationEnable;// = new ComboBox[lent];

        //public List<PalletType> PalletTypes { get; }

        //    = new() { PalletType.组盘, PalletType.拆盘, //PalletType.异常口, PalletType.假电芯盘
        //    };
        public List<PalletType> PalletTypes { get; } = new() { PalletType.组盘, PalletType.拆盘, PalletType.筛选, PalletType.假电芯盘 };

        public List<string> PalletTypesStr { get; }

            = new() { Properties.Resources.Assembly, Properties.Resources.Disassembly, "筛选"
}
            ;

        public List<string> OKNGPalletTypeStr { get; } = new() { "空托盘", "实托盘", "满盘OK", "NG托盘", "复测托盘" ,"拆盘托盘", "组盘托盘", "挑选位托盘", "组盘位托盘", "补盘位托盘" };
        public List<string> ECSPalletTypeStr { get; } = new() { "空托盘", "实托盘", "满盘OK", "NG托盘", "复测托盘" ,"拆盘托盘", "组盘托盘", "挑选位托盘", "组盘位托盘", "补盘位托盘"};

        public StationSettingView()
        {
            InitializeComponent();
            Init();

        }

        public void Init()
        {
            var stations = MainStorage.Saves.Stage.Stations;
            stationInName = new TextBox[stations.Length];
            stationName = new TextBox[stations.Length];
            rfidReaderIP = new TextBox[stations.Length];
            StationNum = new TextBox[stations.Length];
            channelNum = new TextBox[stations.Length];
            columnNum = new TextBox[stations.Length];
            ECSTypeBoxs = new ComboBox[stations.Length];
            comboBoxes = new ComboBox[stations.Length];
            OKNGBoxes = new ComboBox[stations.Length];
            isStationEnable = new CheckBox[stations.Length];

            int idx = 0;
            foreach (var station in stations)
            {
                stationInName[idx] = new()
                {
                    Width = 100,
                    Text = station.InsideNum.ToString(),

                };
                stationName[idx] = new()
                {
                    Width = 150,
                    Text = station.Name,
                    IsReadOnly = station is not Pallet
                };

                rfidReaderIP[idx] = new()
                {
                    Width = 150,
                    Text = station.ReaderIpAddr,
                    IsEnabled = station is Pallet,
                };

                StationNum[idx] = new()
                {
                    Width = 150,
                    Text = station.OutsideStationNum.ToString(),
                    //IsEnabled = i > 5,
                };

                channelNum[idx] = new()
                {
                    Width = 150,
                    Text = station.ChannelNum.ToString(),
                    //IsEnabled = diaoDuStage is Pallet,
                };

                columnNum[idx] = new()
                {
                    Width = 150,
                    Text = station.Column.ToString(),
                    //IsEnabled = diaoDuStage is Pallet,
                };

                comboBoxes[idx] = new()
                {
                    Width = 150,
                    ItemsSource = PalletTypesStr,

                    IsEnabled = station is Pallet,
                };
                if (station is Pallet a)
                {
                    comboBoxes[idx].SelectedIndex = PalletTypes.IndexOf(a.Type);
                }

                OKNGBoxes[idx] = new()
                {
                    Width = 150,
                    ItemsSource = OKNGPalletTypeStr,

                    IsEnabled = station is Pallet,
                };
                if (station is Pallet b)
                {
                    OKNGBoxes[idx].SelectedIndex = (int)b.OKNGType;
                }

                ECSTypeBoxs[idx] = new()
                {
                    Width = 150,
                    ItemsSource = ECSPalletTypeStr,

                    IsEnabled = station is Pallet,
                };
                if (station is Pallet c)
                {
                    ECSTypeBoxs[idx].SelectedIndex = (int)c.ECSCZTrayType;
                }

                isStationEnable[idx] = new()
                {
                    IsChecked = station.IsEnable,
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    Margin = new(10, 0, 0, 0)
                    //Content = "启用",
                    //IsEnabled = station is Pallet
                };


                idx++;
            }


            for (int i = 0; i < stationName.Length; i++)
            {
                StackPanel stackPanel = new()
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new(5)
                };
                //stackPanel.Children.Add(new TextBlock
                //{
                //    //Text = ((i + 1) * 100).ToString(),
                //    Text = stations[i].InsideNum.ToString(),
                //    Width = 50,
                //    VerticalAlignment = VerticalAlignment.Center
                //});

                //stackPanel.Children.Add(new TextBlock
                //{
                //    //Text = ((i + 1) * 100).ToString(),
                //    Text = stations[i].InsideNum.ToString(),
                //    Width = 50,
                //    VerticalAlignment = VerticalAlignment.Center
                //});

                stackPanel.Children.Add(stationInName[i]);
                stackPanel.Children.Add(stationName[i]);
                stackPanel.Children.Add(rfidReaderIP[i]);
                stackPanel.Children.Add(StationNum[i]);
                stackPanel.Children.Add(comboBoxes[i]);
                stackPanel.Children.Add(OKNGBoxes[i]);
                stackPanel.Children.Add(ECSTypeBoxs[i]);
                stackPanel.Children.Add(channelNum[i]);
                stackPanel.Children.Add(columnNum[i]);
                stackPanel.Children.Add(isStationEnable[i]);
                DiaoPanel.Children.Add(stackPanel);

            }
        }
        [JsonIgnore]
        static Lazy<ILogger> _lazyLogger = new Lazy<ILogger>(App.GetService<ILogger<MainViewModel>>);
        [JsonIgnore]
        static ILogger _logger => _lazyLogger.Value;
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < stationName.Length; i++)
            {
                var station = MainStorage.Saves.Stage.Stations[i];
                station.Name = stationName[i].Text;
                //station.SaoMaIP = textBoxes1[i].Text;
                station.InsideNum = int.Parse(stationInName[i].Text);
                station.OutsideStationNum = int.Parse(StationNum[i].Text);
                station.ChannelNum = int.Parse(channelNum[i].Text);
                station.Column = int.Parse(columnNum[i].Text);
                station.ReaderIpAddr = rfidReaderIP[i].Text;
                station.IsEnable = isStationEnable[i].IsChecked ?? false;


                if (station is Pallet pallet)
                {
                    if (OKNGBoxes[i].SelectedIndex >= 0 && OKNGBoxes[i].SelectedIndex <= OKNGBoxes[i].Items.Count)
                    {
                        pallet.OKNGType = (TrayType)OKNGBoxes[i].SelectedIndex;
                        //switch
                        //{
                        //    0 => TrayType.Engineering,
                        //    1 => TrayType.NG,
                        //    2 => TrayType.空筛选类型,
                        //    3 => TrayType.拆盘托盘,
                        //    4 => TrayType.组盘托盘,
                        //};
                    }

                    if (ECSTypeBoxs[i].SelectedIndex >= 0 && ECSTypeBoxs[i].SelectedIndex <= ECSTypeBoxs[i].Items.Count)
                    {

                        pallet.ECSCZTrayType = (ECSTrayType)(ECSTypeBoxs[i].SelectedIndex);

                    }
                    _logger.LogDebug($"工位号：{station.OutsideStationNum}，" +
                        $"FMS下拉框索引：{(TrayType)OKNGBoxes[i].SelectedIndex}，" +
                        $"FMS托盘类型：{pallet.OKNGType.GetTrayTypeStr()}，" +
                        $"ECS下拉框索引：{(ECSTrayType)(ECSTypeBoxs[i].SelectedIndex )}，" +
                        $"ECS托盘类型：{pallet.ECSCZTrayType.GetOUTTrayTypeStr()}");
                    pallet.Type = PalletTypes[comboBoxes[i].SelectedIndex];
                }



            }

            MainStorage.SaveManager.Save();
            Close();
            //Dictionary<int, Model.DiaoDuStage> dictionary = MainViewModel.Saves.StageData.GetDiaoDuSet();
            //try
            //{
            //    for (int i = 0; i < textBoxes.Length; i++)
            //    {
            //        // 断定非null
            //        var pat = dictionary[(i + 1) * 100];
            //        pat.Name = textBoxes[i].Text;
            //        pat.SaoMaIP = textBoxes1[i].Text;
            //        pat.TrueGoneWeiNum = int.Parse(textBoxes2[i].Text);
            //        pat.ChannelNum = int.Parse(channelNum[i].Text);
            //        if (pat is Pallet pallet)
            //            pallet.PalletType = (PalletType)comboBoxes[i].SelectedItem;
            //        //MainViewModel.Saves.GrapStation[(i + 1) * 100] = textBoxes[i].Text;
            //    }

            //    Close();
            //}
            //catch (System.Exception ex)
            //{

            //    MessageBox.Show("保存失败!\n" + ex.Message);
            //}


        }
    }
}
