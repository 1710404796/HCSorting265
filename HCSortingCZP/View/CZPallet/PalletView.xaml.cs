using HCSortingCZP.Attritubes;
using HCSortingCZP.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace HCSortingCZP.View.CZPallet
{
    /// <summary>
    /// PalletView.xaml 的交互逻辑
    /// </summary>
public partial class PalletView : UserControl, IDisposable
    {
        private sealed class FullCodeWrapConverter : IValueConverter
        {
            public static readonly FullCodeWrapConverter Instance = new();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                var code = value as string;
                if (string.IsNullOrWhiteSpace(code))
                {
                    return string.Empty;
                }

                code = code.Trim();
                code = new string(code.Where(c => !char.IsWhiteSpace(c)).ToArray());
                const int charsPerLine = 12;
                if (code.Length <= charsPerLine)
                {
                    return code;
                }

                System.Text.StringBuilder sb = new(code.Length + code.Length / charsPerLine);
                for (int i = 0; i < code.Length; i += charsPerLine)
                {
                    if (i > 0)
                    {
                        sb.AppendLine();
                    }
                    sb.Append(code.Substring(i, Math.Min(charsPerLine, code.Length - i)));
                }

                return sb.ToString();
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }
        }

        /// <summary>
        /// 电池通道
        /// </summary>
        private Border[] _borders;
        private readonly List<(Border Border, MouseButtonEventHandler Handler)> _borderHandlers = [];
        private bool _disposed;

        private readonly ILogger<PalletView> _logger;

        public bool ClickAvaliable { get; set; } = true;


        public XYLStation Station { get; set; }

        public PalletView()
        {
            DataContext = this;
            // ViewModel好像需要别人给
            InitializeComponent();

            _logger = App.GetService<ILogger<PalletView>>();
        }


        public Border GetBatteryBorder(int id, int col)
        {
            // 计算所需的总行数
            int rows = (int)Math.Ceiling((double)Station.ChannelNum / Station.Column);
            const double targetWidth = 2500;        // 宽
            const double targetHeight = 1450;       // 高
            const double spacingX = 2;              // 水平间距
            const double spacingY = 2;              // 垂直间距

            // 通道宽度计算   (总宽 - (列数+1)×水平间距) / 列数
            double cardWidth = Math.Clamp((targetWidth - (Station.Column + 1) * spacingX) / Station.Column, 130, 380);

            // 高度计算     (总高 - (行数+1)×垂直间距) / 行数
            double cardHeight = Math.Clamp((targetHeight - (rows + 1) * spacingY) / rows, 78, 180);

            // 通道号字体大小：卡片高度的 28%，限制在 12~20 像素（通常用于显示通道编号）。
            double indexFontSize = Math.Clamp(cardHeight * 0.28, 12, 20);

            // 电芯条码字体大小：卡片高度的 28%，限制在 12~24 像素（用于显示通道名称或其它标识）。
            double codeFontSize = Math.Clamp(cardHeight * 0.28, 12, 24);

            // 边框粗细：卡片高度的 2%，限制在 1~3 像素。
            double borderThickness = Math.Clamp(cardHeight * 0.02, 1, 3);

            // id需不需要处理一下
            Border res = new Border
            {
                Margin = new Thickness(col >= 0 ? spacingX : 1, spacingY, 1, 1),
                Width = cardWidth,
                Height = cardHeight,
                BorderThickness = new Thickness(borderThickness),
                CornerRadius = new CornerRadius(2),
                Cursor = Cursors.Hand,
                Background = Brushes.Transparent
            };

            res.SetBinding(Border.BorderBrushProperty, new Binding($"Channels[{id}].BorderBrush")
            {
                Source = Station,
                Mode = BindingMode.OneWay,
            });
            res.SetBinding(Border.ToolTipProperty, new Binding($"Channels[{id}].Code")
            {
                Source = Station,
                Mode = BindingMode.OneWay,
            });

            Grid grid = new();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            res.Child = grid;

            // 通道号文本块
            TextBlock textBlock1 = new()
            {
                Text = (id + 1).ToString(),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(6, 2, 0, 0),
                FontSize = indexFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            textBlock1.IsHitTestVisible = false;
            Grid.SetRow(textBlock1, 0);

            // 电芯条码文本块
            TextBlock textBlock2 = new()
            {
                Margin = new Thickness(2, 0, 2, 1),
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold,
                FontSize = codeFontSize,
                FontFamily = new FontFamily("Consolas"),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                LineHeight = codeFontSize,
            };

            //ViewModel.Pallet.BindBattery(2, "sadasadasdasdasddasdassd");
            textBlock2.SetResourceReference(TextBlock.ForegroundProperty, "SystemControlPageTextBaseHighBrush");
            textBlock2.SetBinding(TextBlock.TextProperty, new Binding($"Channels[{id}].Code")
            //textBlock2.SetBinding(TextBlock.TextProperty, new Binding($"aa[{id}]")
            {
                Source = Station,
                Mode = BindingMode.OneWay,
                Converter = FullCodeWrapConverter.Instance
            });
            textBlock2.IsHitTestVisible = false;
            Grid.SetRow(textBlock2, 1);

            // 还需要有点击修改事件
            if (ClickAvaliable)
            {
                MouseButtonEventHandler handler = (sender, e) => Res_MouseDown(sender, e, id);
                res.MouseLeftButtonDown += handler;
                _borderHandlers.Add((res, handler));
            }

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
            if (Station.Column == 0) return;
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

                        _borders[border1Id] = GetBatteryBorder(border1Id, i);

                        stackPanel.Children.Add(_borders[border1Id]);
                    }

                }
                int rows = (int)Math.Ceiling((double)Station.ChannelNum / Station.Column);
                const double targetWidth = 2500;
                const double targetHeight = 1450;
                const double spacingX = 2;
                const double spacingY = 2;
                double cardWidth = Math.Clamp((targetWidth - (Station.Column + 1) * spacingX) / Station.Column, 130, 380);
                double cardHeight = Math.Clamp((targetHeight - (rows + 1) * spacingY) / rows, 78, 180);

                PalletBorder.Height = rows * (cardHeight + spacingY) + spacingY + 10;
                PalletBorder.Width = Station.Column * (cardWidth + spacingX) + spacingX + 10;

            }
            else
            {
                _logger.LogError("试图显示null托盘");

            }
        }

        public void Refesh()
        {
            foreach (var (border, handler) in _borderHandlers)
            {
                border.MouseLeftButtonDown -= handler;
            }
            _borderHandlers.Clear();

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

        /// <summary>
        /// 释放动态生成的通道 Border、事件订阅、绑定和 DataContext。
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            foreach (var (border, handler) in _borderHandlers)
            {
                border.MouseLeftButtonDown -= handler;
            }
            _borderHandlers.Clear();

            PalletStack.Children.Clear();
            _borders = [];
            GC.SuppressFinalize(this);
        }
    }
}
