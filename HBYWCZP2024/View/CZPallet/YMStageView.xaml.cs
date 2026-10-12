using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Models.CZPallet;
using HBYWCZP2024.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HBYWCZP2024.Mediator.Commands;
using MediatR;
using System;
using System.Collections.Generic;

namespace HBYWCZP2024.View.CZPallet
{
    /// <summary>
    /// YMStageView.xaml 的交互逻辑
    /// </summary>
    [PowerView(PowerEnum.工位状态)]
    public partial class YWStageView : UserControl, IDisposable
    {
        public readonly IMediator mediator;
        private readonly List<GongWeiView> _createdViews = [];
        private bool _disposed;
        public YWStageView()
        {
            mediator = App.GetService<IMediator>();
            DataContext = this;
            //ViewModel = App.GetService<YWStageViewModel>();
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var station in MainStorage.Saves.Stage.Stations.OrderBy(s => s is Pallet ? 0 : 1).ThenBy(s =>
            {
                var p = s as Pallet;
                if (p != null)
                {
                    if (p.Type == PalletType.组盘)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return 2;
                }

            }))
            {
                if (station.IsEnable)
                {
                    var view = new GongWeiView(station);
                    SetViewStyle(view, station);
                }


            }

        }

        private void SetViewStyle(GongWeiView view, XYLStation xYLStation)
        {
            GongWeiSim.Children.Add(view);
            if (xYLStation != null)
            {
                if (xYLStation is Pallet p)
                {
                    view.Width = 800;
                    view.Height = 800;
                }
                else
                {
                    view.Width = 300;
                    view.Height = 300;
                    view.GongweiTitle.FontSize = 12;
                }
            }
            else
            {
                view.Width = 550;
                view.Height = 550;
            }

            view.Margin = new Thickness(5, 10, 5, 10);
            view.Background = Brushes.Transparent;
            view.MouseDown += View_MouseDown;
            _createdViews.Add(view);
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

        private async void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(Properties.Resources.DeleteConfirmation, "", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                foreach (var item in MainStorage.Saves.Stage.Stations)
                {
                    item.ClearStage();
                }
            }
            await mediator.Send(new OperatorLogCommand("清除了当前工位的所有数据"));
        }

        /// <summary>
        /// 释放动态创建的 GongWeiView、事件订阅、绑定和 DataContext。
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            foreach (var view in _createdViews)
            {
                view.MouseDown -= View_MouseDown;
                view.Dispose();
            }
            _createdViews.Clear();

            GongWeiSim.Children.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
