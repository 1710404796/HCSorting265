using HBYWCZP2024.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace HBYWCZP2024.View.CZPallet
{
    /// <summary>
    /// PalletView.xaml 的交互逻辑
    /// </summary>
    public partial class PalletView : UserControl
    {
        /// <summary>
        /// 电池通道
        /// </summary>
        private Border[] _borders;
        private readonly ILogger<PalletView> _logger;


        public XYLStation Station { get; set; }

        public PalletView()
        {
            DataContext = this;
            // ViewModel好像需要别人给
            InitializeComponent();

            _logger = App.GetService<ILogger<PalletView>>();
        }


        public Border GetBatteryBorder(int id)
        {
            // id需不需要处理一下
            Border res = new Border
            {
                Margin = new Thickness(10),
                Width = 220,
                Height = 30,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(5),
            };
            res.SetBinding(Border.BorderBrushProperty, new Binding($"Channels[{id}].BorderBrush")
            {
                Source = Station,
                Mode = BindingMode.OneWay,
            });
            Grid grid = new();

            res.Child = grid;
            TextBlock textBlock1 = new() { Text = (id + 1).ToString() };
            textBlock1.SetResourceReference(TextBlock.ForegroundProperty, "SystemControlPageTextBaseHighBrush");


            TextBlock textBlock2 = new()
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };
            //ViewModel.Pallet.BindBattery(2, "sadasadasdasdasddasdassd");
            textBlock2.SetResourceReference(TextBlock.ForegroundProperty, "SystemControlPageTextBaseHighBrush");
            textBlock2.SetBinding(TextBlock.TextProperty, new Binding($"Channels[{id}].Code")
            //textBlock2.SetBinding(TextBlock.TextProperty, new Binding($"aa[{id}]")
            {
                Source = Station,
                Mode = BindingMode.OneWay,

            });

            // 还需要有点击修改事件

            res.MouseDown += (sender, e) => Res_MouseDown(sender, e, id);

            grid.Children.Add(textBlock1);
            grid.Children.Add(textBlock2);
            return res;
        }

        /// <summary>
        /// 双击编辑条码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Res_MouseDown(object sender, MouseButtonEventArgs e, int id)
        {
            //App.Current.MainWindow.IsEnabled= false;
            var zqd = new ChannelModify(Station, id);
            zqd.ShowDialog();


            //App.Current.MainWindow.IsEnabled = true;



            // 双击编辑
            //throw new NotImplementedException();
        }

        private void InitBorder()
        {
            if (Station != null)
            {
                _borders = new Border[Station.ChannelNum];

                for (int i = 0; i < Station.Column; i++)
                {
                    StackPanel stackPanel = new();
                    PalletStack.Children.Add(stackPanel);
                    stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                    stackPanel.Orientation = Orientation.Vertical;
                    for (int j = 0; j < Station.ChannelNum / Station.Column; j++)
                    {




                        int border1Id = i * Station.ChannelNum / Station.Column + j;

                        _borders[border1Id] = GetBatteryBorder(border1Id);

                        stackPanel.Children.Add(_borders[border1Id]);
                    }

                }
                PalletBorder.Height = _borders.Length * 50 / Station.Column + 40;
                PalletBorder.Width = Station.Column * 210 + 140;

                //if (Station.ChannelNum < 10)
                //{
                //    for (int i = 0; i < Station.ChannelNum; i++)
                //    {
                //        StackPanel stackPanel = new();
                //        stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                //        stackPanel.Orientation = Orientation.Horizontal;

                //        PalletStack.Children.Add(stackPanel);


                //        int border1Id = i;

                //        _borders[border1Id] = GetBatteryBorder(border1Id);

                //        stackPanel.Children.Add(_borders[border1Id]);
                //    }

                //    PalletBorder.Height = _borders.Length * 50 + 40;
                //    PalletBorder.Width = 1 * 200 + 140;
                //}
                //else
                //{


                //    for (int i = 0; i < Station.ChannelNum / 2; i++)
                //    {
                //        StackPanel stackPanel = new();
                //        stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
                //        stackPanel.Orientation = Orientation.Horizontal;

                //        PalletStack.Children.Add(stackPanel);


                //        int border1Id = i;
                //        int border2Id = border1Id + Station.ChannelNum / 2;


                //        _borders[border1Id] = GetBatteryBorder(border1Id);
                //        _borders[border2Id] = GetBatteryBorder(border2Id);

                //        //_borders[border1Id] = new();
                //        //_borders[border2Id] = new();
                //        //_borders[border2Id].Margin = _borders[border1Id].Margin = new Thickness(10);
                //        //_borders[border2Id].Width = _borders[border1Id].Width = 100;
                //        //_borders[border2Id].Height = _borders[border1Id].Height = 30;
                //        //_borders[border2Id].BorderThickness = _borders[border1Id].BorderThickness = new Thickness(2);
                //        //_borders[border2Id].CornerRadius = _borders[border1Id].CornerRadius = new CornerRadius(5);
                //        //_borders[border2Id].BorderBrush = _borders[border1Id].BorderBrush = Brushes.Green;




                //        // 设置文本 文本记得加上实际代码 如果有的话
                //        //TextBlock textBlock= new() { Text = border1Id.ToString() };
                //        //TextBlock textBlock1= new() { Text = border2Id.ToString() };

                //        //textBlock.SetResourceReference(TextBlock.ForegroundProperty, "SystemControlPageTextBaseHighBrush");
                //        //textBlock1.SetResourceReference(TextBlock.ForegroundProperty, "SystemControlPageTextBaseHighBrush");

                //        //_borders[border1Id].Child= textBlock;
                //        //_borders[border2Id].Child= textBlock1;




                //        stackPanel.Children.Add(_borders[border1Id]);
                //        stackPanel.Children.Add(_borders[border2Id]);
                //    }
                //    PalletBorder.Height = _borders.Length * 25 + 40;
                //    PalletBorder.Width = 2 * 200 + 140;
                //}



            }
            else
            {
                _logger.LogError("试图显示null托盘");

            }
        }

        public void Refesh()
        {
            // 如何销毁（？ 清除后再初始化
            PalletStack.Children.Clear();
            InitBorder();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化

            InitBorder();
        }
        /// <summary>
        /// 跳转至工位详细信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PalletBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            return;
            //MainWindow.NageTo(new DiaoSuDetail ( new DiaoSuDetailViewModel { PalletViewModel = ViewModel });
        }
    }
}
