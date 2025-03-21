using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.View;
using HBYWCZP2024.View.CZPallet;
using HBYWCZP2024.View.User;
using HBYWCZP2024.ViewModel;
using MediatR;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HBYWCZP2024
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel
        {
            get; set;
        }
        public readonly IMediator mediator;
        public MainWindow()
        {
            MainStorage.SaveManager.Load();
            DataContext = this;
            mediator=App.GetService<IMediator>();
            ViewModel = App.GetService<MainViewModel>();
            InitializeComponent();
        }

        //public static void NageTo<T>() where T : FrameworkElement
        //{
        //    (App.Current.MainWindow as MainWindow)!.ContentFrame.Content = App.GetService<T>();
        //}
        public static async void NageTo(FrameworkElement frameworkElement)
        {
            var mediator1 = App.GetService<IMediator>();

            var tt = frameworkElement.GetType().GetCustomAttribute<PowerViewAttribute>();
            if (tt != null)
            {
                if (await mediator1.Send(new CheckPowerQuery(tt.Power)))
                {

                }
                else
                {
                    MessageBox.Show("当前没有权限访问此页面!");
                    (App.Current.MainWindow as MainWindow)!.ContentFrame.Content = null;
                    return;
                }
            }
                (App.Current.MainWindow as MainWindow)!.ContentFrame.Content = frameworkElement;
        }

        public static async void NageTo<T>() where T : FrameworkElement
        {
            var mediator1 = App.GetService<IMediator>();

            var tt = typeof(T).GetCustomAttribute<PowerViewAttribute>();
            if (tt != null)
            {
                if (await mediator1.Send(new CheckPowerQuery(tt.Power)))
                {

                }
                else
                {
                    MessageBox.Show("当前没有权限访问此页面!");
                    (App.Current.MainWindow as MainWindow)!.ContentFrame.Content = null;
                    return;
                }
            }
             (App.Current.MainWindow as MainWindow)!.ContentFrame.Content = App.GetService<T>();
        }



        private void LogDash_Click(object sender, RoutedEventArgs e)
        {
            NageTo<LogDashBoardView>();
        }

        /// <summary>
        /// 程序参数设定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Setting_Click(object sender, RoutedEventArgs e)
        {
            var mediator = App.GetService<IMediator>();
            var checkPowerQuery = new CheckPowerQuery(PowerEnum.程序参数设定);
            bool result = await mediator.Send(checkPowerQuery);
            if (result)
            {
                NageTo<SettingView>();
            }
            else
            {
                MessageBox.Show("当前没有此权限");
            }

        }
        /// <summary>
        /// 权限管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PowerManager_Click(object sender, RoutedEventArgs e)
        {
            var mediator = App.GetService<IMediator>();
            var checkPowerQuery = new CheckPowerQuery(PowerEnum.用户管理界面);
            bool result = await mediator.Send(checkPowerQuery);
            if (result)
            {
                NageTo<UserManagerView>();
            }
            else {
                MessageBox.Show("当前没有此权限");
            }
        }
        private async void Window_Closed(object sender, EventArgs e)
        {
            ViewModel.CacheService.Cancel();
            MainStorage.SaveManager.Save();
            await mediator.Send(new OperatorLogCommand("关闭了软件"));
        }
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var cc = MessageBox.Show("确定要关闭吗", "关闭确认", MessageBoxButton.OKCancel);
            if (cc == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            if (await mediator.Send(new CheckPowerQuery(PowerEnum.关闭软件)))
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
                MessageBox.Show("您当前没有权限关闭软件");
            }
        }



        private void GongWeiBtn_Click(object sender, RoutedEventArgs e)
        {
            NageTo<YWStageView>();

        }

        private void WCSLog_Click(object sender, RoutedEventArgs e)
        {
            NageTo<WCSLogView>();

        }

        private void WarningLog_Click(object sender, RoutedEventArgs e)
        {
            NageTo<WarningLogView>();

        }

        private void CPLog_Click(object sender, RoutedEventArgs e)
        {
            NageTo<PalletChaiLogView>();

        }

        private void ZPLog_Click(object sender, RoutedEventArgs e)
        {
            NageTo<PalletLogView>();

        }

        private async void testzupan_Click(object sender, RoutedEventArgs e)
        {



            var mediator1 = App.GetService<IMediator>();
            var key = "Default";

            var res2 = await mediator1.Send(new GrabActionCommand(key, 2450, false, true)).ConfigureAwait(false); ;

            return;

            var res = await mediator1.Send(new PalletArrivedCommand(key, 2100)).ConfigureAwait(false);
            await mediator1.Send(new InformationBoxCommand("托盘到位: " + res));
            if (!res) return;


            var data  = await mediator1.Send(new ApplyBatteryInfoCommand(key, 2100));
            await mediator1.Send(new InformationBoxCommand("申请信息: " + res));
            if (!res) return;

            //res = await mediator1.Send(new GrabActionCommand(key, 2100, true));
            //await mediator1.Send(new InformationBoxCommand("作业开始: " + res));
            //if (!res) return;

            //res = await mediator1.Send(new GrabActionCommand(key, 2100, false));
            //await mediator1.Send(new InformationBoxCommand("作业结束: " + res));
            //if (!res) return;

            res = await mediator1.Send(new PalletOutCommand(key, 2100));
            await mediator1.Send(new InformationBoxCommand("托盘离开: " + res));
            if (!res) return;
        }

        //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    var cc = MessageBox.Show("确定要关闭吗", "关闭确认", MessageBoxButton.OKCancel);
        //    if (cc == MessageBoxResult.Cancel)
        //    {
        //        e.Cancel = true;
        //    }
        //}

    }
}