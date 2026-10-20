using HCSortingCZP.Mediator.Commands;
using HCSortingCZP.Mediator.ECS;
using HCSortingCZP.Models;
using HCSortingCZP.Models.CZPallet;
using HCSortingCZP.PLC.Tasks;
using HCSortingCZP.Utils;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Controls;


namespace HCSortingCZP.View.CZPallet
{
    /// <summary>
    /// DiaoSuDetail.xaml 的交互逻辑
    /// </summary>
    public partial class StationDetail : UserControl, IDisposable
    {
        private readonly IMediator meditor;
        private readonly ILogger<StationDetail> logger;
        private bool _disposed;

        public XYLStation Station { get; set; }

        public StationDetail(XYLStation station)
        {
            Station = station;
            DataContext = this;
            InitializeComponent();
            PalletD.Station = station;
            meditor = App.GetService<IMediator>();
            logger = App.GetService<ILogger<StationDetail>>();
        }

        /// <summary>
        /// 释放内部 PalletView、绑定和 DataContext，避免页面切换后残留。
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            PalletD.Dispose();
            GC.SuppressFinalize(this);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //PalletD
            if (Station is not Pallet)
            {
                Pikachu.Visibility = Visibility.Collapsed;
                //BindWithHuman.Visibility = Visibility.Collapsed;
                ScanAgain.Visibility = Visibility.Collapsed;
                PalletCodeBlock.Visibility = Visibility.Collapsed;
                //Confirm.Visibility = Visibility.Collapsed;
                BanSaoma.Visibility = Visibility.Collapsed;
                Force.Visibility = Visibility.Collapsed;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.NageTo<YWStageView>();
        }

        private void BindWithHuman_Click(object sender, RoutedEventArgs e)
        {
            // 可能要提供专门的界面
            if (MessageBox.Show("是否要开始人工绑定盘流程？", "组盘确认", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //Station.ClearStage();
                //var dia = new ReadOtherInput("请扫托盘条码");
                //dia.ShowDialog();
                //// 此处还需校验托盘种类 // Todo：托盘种类可能对应电芯数量
                //Station.PalletCode = dia.Result;

                //for (int i = 0; i < Station.ChannelNum; i++)
                //{
                //    dia = new ReadOtherInput($"请扫通道{i + 1}电芯条码");
                //    dia.ShowDialog();
                //    // 失败提示 应该是，而不该直接ng
                //    Station.BindBattery(i, dia.Result, true);
                //}

                //MessageBox.Show("绑定完成！");

                //// 如果需要复核 则在复核时上传
                //// 考虑保存
                //this.Focus();
            }


            // 任务流程 1.扫托盘码 2. 阻盘开始 3. 无限绑定 4. 阻盘完成

            // WaitforPalletCode -> 检测pallet类型
            // 
        }

        /// <summary>
        /// 重新扫码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ScanAgain_Click(object sender, RoutedEventArgs e)
        {
            await Task.Yield();
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            if (Station is not Pallet aa)
            {
                return;
            }

            ScanAgain.IsEnabled = false;
            try
            {
                aa.PalletCode = (await meditor.Send(new ScanPalletCodeCommand(MainStorage.Saves.SelectStageName, aa.PalletId)))?.Trim() ?? string.Empty;
                RuntimeStateStore.Save();
                await meditor.Send(new OperatorLogCommand("进行了重新扫码"));
            }
            catch (Exception ex)
            {
                aa.PalletCode = "ERROR";
                logger.LogError(ex, "重新扫码失败: {StationName}, PalletId={PalletId}", Station.Name, aa.PalletId);
                ShowNonBlockingWarning($"{aa.Name} 扫码失败: {ex.Message}\n请检查托盘码、扫码器连接后重试。");
            }
            finally
            {
                ScanAgain.IsEnabled = true;
            }
        }

        private void ShowNonBlockingWarning(string message)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await meditor.Send(new WarningBoxCommand(message));
                }
                catch (Exception warningEx)
                {
                    logger.LogError(warningEx, "显示重新扫码失败报警失败");
                }
            });
        }

        private async void Clear_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show($"确认要清除此{Station.Name},{Station.InsideNum}记忆吗？", " 写入确认", MessageBoxButton.OKCancel);
            if (res != MessageBoxResult.OK) return;

            //App.GetService<ILogger<MainViewModel>>().LogInformation($"清除了{ViewModel.PalletViewModel.Pallet.Name}记忆");
            await meditor.Send(new OperatorLogCommand($"清除{Station.Name},{Station.InsideNum}所有数据"));
            Station.ClearStage();
            RuntimeStateStore.Save();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            //App.GetService<ILogger<MainViewModel>>().LogInformation($"人工确认异常条码点击");

            //(ViewModel.PalletViewModel.Pallet as Pallet)?.ReCheckCode();

        }

        /// <summary>
        /// 强制写入托盘条码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Force_Click(object sender, RoutedEventArgs e)
        {
            await Task.Yield();
            if (Station is not Pallet pall)
            {
                return;
            }

            Pikachu.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            var palletCode = pall.PalletCode?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(palletCode))
            {
                ShowNonBlockingWarning("请先输入托盘条码后再强制写入。");
                return;
            }

            if (string.Equals(palletCode, "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                ShowNonBlockingWarning(
                    "当前托盘扫码失败，请先输入正确的托盘条码后再强制写入。");
                return;
            }

            var res = MessageBox.Show($"确认要强制写入 {pall.Name} 条码吗？\n\n{palletCode}", "写入确认", MessageBoxButton.OKCancel);
            if (res != MessageBoxResult.OK) return;

            Force.IsEnabled = false;
            try
            {
                pall.PalletCode = palletCode;
                pall.TempData[PalletArrivedTask.ManualForcePalletCodeKey] = palletCode;
                RuntimeStateStore.Save();
                await meditor.Send(new OperatorLogCommand($"强制写入托盘条码{palletCode}成功"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "强制写入托盘条码失败: {StationName}, PalletId={PalletId}", pall.Name, pall.PalletId);
                ShowNonBlockingWarning($"{pall.Name} 强制写入失败: {ex.Message}");
            }
            finally
            {
                Force.IsEnabled = true;
            }
        }

        private async void CvRequest_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("确定要重新调用CvRequest接口吗？", " 确认", MessageBoxButton.OKCancel);
            if (res != MessageBoxResult.OK) return;
            var pall = Station as Pallet;
            if (pall != null)
            {
                await meditor.Send(new CvRequestCommand(Station.Name, Station.OutsideStationNum, false));
            }

        }



        //private void BanSaoma_Click(object sender, RoutedEventArgs e)
        //{
        //    if ( ViewModel.PalletViewModel.Pallet is Pallet p)
        //    {
        //        p.BanSaoma = !(sender as CheckBox).IsChecked ?? false;
        //    }
        //}
    }
}
