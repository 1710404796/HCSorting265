using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Models;
using LinxUniverse.Auth;
using LinxUniverse.DI;
using MediatR;
using Microsoft.AspNetCore.Components.Authorization;
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
    public partial class MainViewModel : ObservableRecipient
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
        private Thread _someThread;

        public bool IsLoading => !IsWcsEnable;

        public IMediator Mediator { get; }
        public LinxAuthenticationStateProvider AuthenticationStateProvider { get; }
        public ILogger<MainViewModel> Logger { get; }
        [ObservableProperty] private string _username;


        [RelayCommand]
        public async Task SignOut()
        {
            Logger.LogInformation("{username} 登出", Username);

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
                ILogger<MainViewModel> logger, CacheService cacheService)
        {

            Task.Run(async () =>
            {

                await mediator.Send(new InitMeCommand());
                //await Task.WhenAll(mediator.Send(new ChangeCstAllCommand(30)));

                //await codeReaderService.Init();
                Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                // 考虑这个位置

                ;

            }).ContinueWith(_ => IsWcsEnable = true);
            authenticationStateProvider.AuthenticationStateChanged += task =>
            {

                // 需要根据类型嗼
                Dispatcher.CurrentDispatcher.Invoke<Task>(async () =>
                {
                    System.Security.Claims.ClaimsPrincipal user = (await task).User;
                    IsLock = user.Identity.IsAuthenticated ? Visibility.Collapsed : Visibility.Visible;
                    if (user.Identity.IsAuthenticated)
                        Username = $"{user.GetUserName()}({user.GetUserRole()})";
                    logger.LogInformation("{username} 登陆", Username);
                    // 输出切换后用户名称
                    // Console.WriteLine(IsLock);

                });
            };
            _someThread = new Thread(SomeLoop);
            _someThread.Start();
            Mediator = mediator;
            AuthenticationStateProvider = authenticationStateProvider;
            Logger = logger;
            _cacheService = cacheService;
        }

        public CancellationTokenSource Maints = new CancellationTokenSource();
        private readonly CacheService _cacheService;

        private async void SomeLoop(object? obj)
        {
            while (!IsWcsEnable)
            {
                await Task.Delay(1000);

            }
            while (_cacheService.Token.IsCancellationRequested)
            {

                //PlcIsRunning = XcplcService.IsPlcRunning;
                await Task.Delay(1000);
            }
        }
    }


}
