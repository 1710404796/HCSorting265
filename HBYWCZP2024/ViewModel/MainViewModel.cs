using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Models;
using HBYWCZP2024.PLC;
using LinxUniverse.Auth;
using LinxUniverse.DI;
using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace HBYWCZP2024.ViewModel
{
    public partial class MainViewModel : ObservableRecipient, IDisposable
    {
        public static WcsSaves Saves => MainStorage.Saves;

        [ObservableProperty]
        private Visibility _hasBackGround = Saves.BackGroundEnable;


        [ObservableProperty]
        Visibility _isLock = Visibility.Visible;

        [ObservableProperty]
        Visibility _isBusy = Visibility.Collapsed;

        [ObservableProperty] private int _progg = 0;
        [ObservableProperty] private string _busyMessage = string.Empty;
        [ObservableProperty] private string _errorText = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLoading))]
        // 標準化一下
        private bool _isWcsEnable = false;
        private Task? _someLoopTask;
        private readonly Task _initTask;
        private bool _disposed;
        private LinxUniverse.Auth.AuthenticationStateChangedHandler? _authenticationStateChangedHandler;
        private readonly CancellationTokenSource Maints = new();
        private readonly OPCUAService _opcuaService;

        public bool IsLoading => !IsWcsEnable;

        public IMediator Mediator { get; }
        public LinxAuthenticationStateProvider AuthenticationStateProvider { get; }
        public IPLCTaskServiceFactory<HBYWPLCContext> PlctaskServiceFactory { get; }
        public ILogger<MainViewModel> Logger { get; }
        [ObservableProperty] private string _username;


        [RelayCommand]
        public async Task SignOut()
        {
            Logger.LogInformation($"{Username} 登出");

            // _authenticationStateProvider.
            await Mediator.Send(new LogOutCommand());
        }


        public string PlcStatus => PlcIsRunning ? "运行" : "停止";
        public SolidColorBrush PlcColor => PlcIsRunning ? Brushes.Green : Brushes.Red;

        [NotifyPropertyChangedFor(nameof(PlcStatus))]
        [NotifyPropertyChangedFor(nameof(PlcColor))]
        [ObservableProperty]
        private bool _plcIsRunning = false;

        public MainViewModel(IMediator mediator,
                LinxAuthenticationStateProvider authenticationStateProvider,
                IPLCTaskServiceFactory<HBYWPLCContext> plctaskServiceFactory,
                ILogger<MainViewModel> logger, CacheService cacheService, OPCUAService opcuaService)
        {

            _initTask = Task.Run(async () =>
            {
                try
                {
                    await mediator.Send(new InitMeCommand(), Maints.Token);
                    Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "后台初始化失败");
                }
                finally
                {
                    try
                    {
                        var dispatcher = Application.Current?.Dispatcher;
                        dispatcher?.Invoke(() => IsWcsEnable = true);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "更新初始化状态失败");
                    }
                }
            });
            _authenticationStateChangedHandler = task =>
            {

                // 需要根据类型嗼
                Dispatcher.CurrentDispatcher.Invoke<Task>(async () =>
                {
                    System.Security.Claims.ClaimsPrincipal user = (await task).User;
                    IsLock = user.Identity.IsAuthenticated ? Visibility.Collapsed : Visibility.Visible;
                    if (user.Identity.IsAuthenticated)
                        Username = $"{user.GetUserName()}({user.GetUserRole()})";
                    logger.LogInformation($"{Username} 登陆");
                    // 输出切换后用户名称
                    // Console.WriteLine(IsLock);

                });
            };
            authenticationStateProvider.AuthenticationStateChanged += _authenticationStateChangedHandler;
            _someLoopTask = Task.Run(() => SomeLoop());
            Mediator = mediator;
            AuthenticationStateProvider = authenticationStateProvider;
            PlctaskServiceFactory = plctaskServiceFactory;
            Logger = logger;
            CacheService = cacheService;
            _opcuaService = opcuaService;
        }

        public readonly CacheService CacheService;

        private async Task SomeLoop()
        {
            try
            {
                while (!IsWcsEnable && !Maints.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(1000, Maints.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }

                }
                if (Maints.IsCancellationRequested) return;
                Console.WriteLine("Wcs启动");
                while (!Maints.IsCancellationRequested)
                {
                    var XcplcService = PlctaskServiceFactory.CreateService(MainStorage.Saves.SelectStageName, null!);
                    var isRunning = XcplcService?.IsRunning ?? false;
                    var dispatcher = Application.Current?.Dispatcher;
                    if (dispatcher != null)
                    {
                        await dispatcher.InvokeAsync(() => PlcIsRunning = isRunning);
                    }
                    else
                    {
                        PlcIsRunning = isRunning;
                    }

                    try
                    {
                        await Task.Delay(1000, Maints.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "PLC 状态轮询异常");
            }
        }

        /// <summary>
        /// 程序退出时停止后台初始化/轮询线程，取消并释放所有 CancellationTokenSource。
        /// </summary>
        public async Task StopAsync()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            // 停止初始化任务和 PLC 监听循环使用的令牌
            Maints.Cancel();

            // 取消使用 CacheService 令牌的常驻线程（如 ECS 心跳）
            CacheService.Cancel();

            if (_authenticationStateChangedHandler != null)
            {
                AuthenticationStateProvider.AuthenticationStateChanged -= _authenticationStateChangedHandler;
                _authenticationStateChangedHandler = null;
            }

            Task[] backgroundTasks =
            [
                _initTask,
                _someLoopTask ?? Task.CompletedTask
            ];

            // 给后台任务一点时间退出，避免关闭时继续创建 PLC 服务或访问已释放资源
            try
            {
                await Task.WhenAll(backgroundTasks).WaitAsync(TimeSpan.FromSeconds(3));
            }
            catch (TimeoutException)
            {
                Logger.LogWarning("后台任务未在 3 秒内退出");
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "后台任务退出异常");
            }

            using var opcuaShutdownCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                await _opcuaService.DisconnectAllAsync(opcuaShutdownCts.Token);
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("OPC UA 客户端未在 5 秒内全部断开");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "OPC UA 客户端断开异常");
            }

            _someLoopTask = null;
            Maints.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
        }
    }


}
