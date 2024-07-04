using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.View;
using HBYWCZP2024.View.CZPallet;
using HBYWCZP2024.ViewModel;
using MediatR;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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
        public MainWindow()
        {
            MainStorage.SaveManager.Load();
            DataContext = this;
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

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            NageTo<SettingView>();

        }

        private void PowerManager_Click(object sender, RoutedEventArgs e)
        {
            NageTo<UserManagerView>();

        }
        private void Window_Closed(object sender, EventArgs e)
        {
            ViewModel.CacheService.Cancel();
            MainStorage.SaveManager.Save();
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
    }
}