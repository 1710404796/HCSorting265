using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.View;
using HBYWCZP2024.View.CZPallet;
using HBYWCZP2024.View.User;
using HBYWCZP2024.ViewModel;
using HBYWCZP2024.Utils;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Text;
using System.Threading;
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
        private readonly ILogger<MainWindow> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Dictionary<FrameworkElement, IServiceScope> _pageScopes = [];
        private readonly Dictionary<Type, FrameworkElement> _pageCache = [];
        private CancellationTokenSource? _navigationCts;
        private bool _shutdownApproved;
        private bool _shutdownInProgress;

        public MainWindow()
        {
            MainStorage.SaveManager.Load();
            DataContext = this;
            mediator=App.GetService<IMediator>();
            ViewModel = App.GetService<MainViewModel>();
            _logger = App.GetService<ILogger<MainWindow>>();
            _scopeFactory = App.GetService<IServiceScopeFactory>();
            InitializeComponent();
        }

        //public static void NageTo<T>() where T : FrameworkElement
        //{
        //    (App.Current.MainWindow as MainWindow)!.ContentFrame.Content = App.GetService<T>();
        //}
        public static void NageTo(FrameworkElement frameworkElement)
        {
            var mainWindow = (App.Current.MainWindow as MainWindow);
            _ = mainWindow?.NavigateToAsync(() => frameworkElement, frameworkElement.GetType());
        }

        public static void NageTo<T>() where T : FrameworkElement
        {
            var mainWindow = (App.Current.MainWindow as MainWindow);
            _ = mainWindow?.NavigateToAsync(() => mainWindow.CreatePageInScope<T>(), typeof(T));
        }

        /// <summary>
        /// 从独立子作用域解析页面；切换离开时释放作用域，避免旧页面/ViewModel/DbContext 被根容器长期持有。
        /// </summary>
        private FrameworkElement CreatePageInScope<T>() where T : FrameworkElement
        {
            if (_pageCache.TryGetValue(typeof(T), out var cachedPage))
            {
                return cachedPage;
            }

            var scope = _scopeFactory.CreateScope();
            try
            {
                var page = scope.ServiceProvider.GetRequiredService<T>();
                _pageScopes.Add(page, scope);
                if (ShouldCachePage(typeof(T)))
                {
                    _pageCache[typeof(T)] = page;
                }

                return page;
            }
            catch
            {
                scope.Dispose();
                throw;
            }
        }

        private static bool ShouldCachePage(Type pageType)
        {
            return pageType == typeof(PalletLogView) || pageType == typeof(PalletChaiLogView);
        }

        /// <summary>
        /// 切换页面：先卸载旧页面并释放其资源、事件、DataContext 和绑定，再挂载新页面。
        /// </summary>
        private async Task NavigateToAsync(Func<FrameworkElement> factory, Type pageType)
        {
            _navigationCts?.Cancel();
            var cts = _navigationCts = new CancellationTokenSource();

            try
            {
                var attribute = pageType.GetCustomAttribute<PowerViewAttribute>();
                if (attribute != null)
                {
                    var allowed = await mediator.Send(new CheckPowerQuery(attribute.Power), cts.Token);
                    if (!allowed)
                    {
                        if (cts.IsCancellationRequested)
                        {
                            return;
                        }

                        MessageBox.Show("当前没有权限访问此页面!");
                        ReleaseCurrentPage();
                        return;
                    }
                }

                if (cts.IsCancellationRequested)
                {
                    return;
                }

                var page = factory();
                if (cts.IsCancellationRequested)
                {
                    ReleasePage(page);
                    return;
                }

                ShowPage(page);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "页面导航失败: {PageType}", pageType);
            }
            finally
            {
                if (ReferenceEquals(_navigationCts, cts))
                {
                    _navigationCts = null;
                }

                cts.Dispose();
            }
        }

        private void ShowPage(FrameworkElement frameworkElement)
        {
            var oldPage = ContentFrame.Content as FrameworkElement;

            if (!ReferenceEquals(oldPage, frameworkElement))
            {
                ContentFrame.Content = null;
                if (oldPage != null && !ShouldCachePage(oldPage.GetType()))
                {
                    ReleasePage(oldPage);
                }
            }

            ContentFrame.Content = frameworkElement;
        }

        /// <summary>
        /// 无权限访问目标页面时，直接释放当前页面。
        /// </summary>
        private void ReleaseCurrentPage()
        {
            var oldPage = ContentFrame.Content as FrameworkElement;

            ContentFrame.Content = null;
            ReleasePage(oldPage);
        }

        /// <summary>
        /// 释放页面本身以及页面树内的 DataContext/Binding，防止切换页面后旧页面残留。
        /// </summary>
        private void ReleasePage(FrameworkElement? page)
        {
            if (page == null)
            {
                return;
            }

            try
            {
                ViewDisposer.Release(page, ex => _logger.LogError(ex, "释放页面失败: {PageType}", page.GetType()));
            }
            finally
            {
                if (_pageCache.TryGetValue(page.GetType(), out var cached) && ReferenceEquals(cached, page))
                {
                    _pageCache.Remove(page.GetType());
                }

                if (_pageScopes.Remove(page, out var scope))
                {
                    try
                    {
                        scope.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "释放页面作用域失败: {PageType}", page.GetType());
                    }
                }
            }
        }

        private void DisposeAllCachedPages()
        {
            foreach (var page in _pageCache.Values.ToArray())
            {
                ReleasePage(page);
            }

            _pageCache.Clear();
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
        private void Setting_Click(object sender, RoutedEventArgs e) 
        {
            NageTo<SettingView>();
        }

        /// <summary>
        /// 用户管理界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserManager_Click(object sender, RoutedEventArgs e)
        {
            NageTo<UserManagerView>();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            ReleaseCurrentPage();
            DisposeAllCachedPages();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_shutdownApproved)
            {
                return;
            }

            e.Cancel = true;
            if (_shutdownInProgress)
            {
                return;
            }

            _shutdownInProgress = true;

            try
            {
                var cc = MessageBox.Show("确定要关闭吗", "关闭确认", MessageBoxButton.OKCancel);
                if (cc == MessageBoxResult.Cancel)
                {
                    return;
                }

                try
                {
                    if (!await mediator.Send(new CheckPowerQuery(PowerEnum.关闭软件)))
                    {
                        MessageBox.Show("您当前没有权限关闭软件");
                        return;
                    }

                    await ViewModel.StopAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "关闭前清理失败");
                    return;
                }

                try
                {
                    MainStorage.SaveManager.Save();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "保存配置失败");
                }

                try
                {
                    await mediator.Send(new OperatorLogCommand("关闭了软件"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "写入关闭日志失败");
                }

                ReleaseCurrentPage();
                _shutdownApproved = true;
                Dispatcher.BeginInvoke(new Action(Close));
            }
            finally
            {
                _shutdownInProgress = false;
            }
        }


        /// <summary>
        /// 工位状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GongWeiBtn_Click(object sender, RoutedEventArgs e)
        {
            NageTo<YWStageView>();

        }

        /// <summary>
        /// WCS交互日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WCSLog_Click(object sender, RoutedEventArgs e)
        {
            NageTo<WCSLogView>();

        }

        /// <summary>
        /// 报警日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WarningLog_Click(object sender, RoutedEventArgs e)
        {
            NageTo<WarningLogView>();

        }

        /// <summary>
        /// 拆盘日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CPLog_Click(object sender, RoutedEventArgs e)
        {
            NageTo<PalletChaiLogView>();

        }

        /// <summary>
        /// NG拉带电芯日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NgBeltCellLog_Click(object sender, RoutedEventArgs e)
        {
            NageTo<NgBeltCellLogView>();
        }

        /// <summary>
        /// 组盘日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
