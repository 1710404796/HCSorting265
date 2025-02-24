using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Utils;
using LinxUniverse.DI;
using LinxUniverse.SciSocket;
using MediatR;
using Microsoft.Extensions.Logging;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace HBYWCZP2024.Service
{
    public class HBECSService
    {
        Dictionary<string, Action<string>> HandlerDict = [];
        Queue<HWSocketMessage> Messages = new Queue<HWSocketMessage>();
        public void On(string command, Action<string> action)
        {
            if (HandlerDict.ContainsKey(command))
            {
                logger.LogWarning("{cmd} 已被注册!", command);
                return;
            }
            HandlerDict[command] = action;

        }


        private LinxClientSocket _clientSocket;
        private readonly ILogger<HBECSService> logger;

        public string Name { get; }
        public IMediator Mediator { get; }
        public MesService MesService { get; }
        public Thread HeartbeatThread;
        public HBECSService(string name, ILogger<HBECSService> logger, IMediator mediator)
        {
            Name = name;
            this.logger = logger;
            Mediator = mediator;
            var ipaddr = IPEndPoint.Parse(MainStorage.Saves.KeysSave[name].ECSIP);
            _clientSocket = new LinxClientSocket(new LinxClientSocketOption
            {
                Address = ipaddr.Address.ToString(),
                Port = (ushort)ipaddr.Port,
                SplitChar = '#',
            });
            _clientSocket.Logger = logger;

            IHttpClientFactory httpClientFactory = App.GetService<IHttpClientFactory>();
            var httplogger = App.GetService<ILogger<MesService>>();
            MesService = new MesService(httpClientFactory.CreateClient(name), name, httplogger);
            Init();
            HeartbeatThread = new Thread(DoHeartBeat);
            HeartbeatThread.Start();
        }

        async void DoHeartBeat()
        {
            var cache = App.GetService<CacheService>();

            while (!cache.Token.IsCancellationRequested)
            {
                try
                {
                    await SendAsync<S000, E000>(new S000());
                    await Task.Delay(MainStorage.Saves.HeartbeartDuring);
                }
                catch (Exception ex)
                {
                    logger.LogError( ex, "心跳");
                }
           
            }
        }

        public void Init()
        {
            _clientSocket.OnComplete += ClientSocket_OnComplete;
            _clientSocket.RecviceLine += ClientSocket_RecviceLine;

            _clientSocket.Connect();
        }

        private void ClientSocket_RecviceLine(string obj)
        {
            logger.LogInformation("接收数据 {msg}", obj);

            var message = MessageHelper.Parse(obj);

            Handler(message);
        }

        private void Handler(HWSocketMessage message)
        {
            logger.LogInformation("{msg}", message);
            if (HandlerDict.ContainsKey(message.Command))
            {
                HandlerDict[message.Command]?.Invoke(message.Content);
            }
            Messages.Enqueue(message);
        }

        private void ClientSocket_OnComplete()
        {
            _clientSocket.Connect();
        }

        object _lockobj = new object();
        public void Send(string command, string content, Action after)
        {
            lock (_lockobj)
            {
                var packMessage = MessageHelper.GetSocketMessage(command, MainStorage.Saves.KeysSave[Name].DeviceCode, content);

                _clientSocket.Send(Encoding.UTF8.GetBytes(packMessage));

                after.Invoke();
            }

        }

        public void Send<T1>(T1 req) where T1 : IECSContent
        {

            var packMessage = MessageHelper.GetSocketMessage(req.Type, MainStorage.Saves.KeysSave[Name].DeviceCode, req.GetContent());

            _clientSocket.Send(Encoding.UTF8.GetBytes(packMessage));
        }
        public async Task<T2> SendAsync<T1, T2>(T1 req, int stationNum = 0) where T1 : IECSContent where T2 : IECSContentRes, new()
        {
            //if (_clientSocket.)
            //await Task.Yield();
            try
            {
                lock (_lockobj)
                {
                    var packMessage = MessageHelper.GetSocketMessage(req.Type, MainStorage.Saves.KeysSave[Name].DeviceCode, req.GetContent());
                    logger.LogInformation("{name}, 发送数据: {data}", Name, packMessage);
                    _clientSocket.Send(Encoding.UTF8.GetBytes(packMessage));
                    CancellationTokenSource cts = new CancellationTokenSource();
                    cts.CancelAfter(60000);

                    var res = WaitFor<T2>(cts.Token);
                    logger.LogInformation("{name}, 接收数据: {data}", Name, JsonSerializer.Serialize(res));
                    if (res.ErrorCode != "0000")
                    {
                        throw new Exception("错误码: " + MainStorage.GetError(res.ErrorCode));
                    }

                    return res;

                }
            }
            catch (Exception ex)
            {

                var wres = await Mediator.Send(new WcsDataWarningBoxCommand
                    (Name + "工作站\n" + $"站台号: {stationNum}" + $"等待{typeof(T2).Name}消息，发生问题 Error:" + ex.Message + "\n点击ok重新发送此请求，点击cancel终止此任务！", Button: System.Windows.MessageBoxButton.OKCancel));
                //throw;
                if (wres == System.Windows.MessageBoxResult.OK)
                {
                    return await SendAsync<T1, T2>(req, stationNum);
                }
                else
                {
                    throw;

                }
            }
        }

        public T WaitFor<T>(CancellationToken? token) where T : IECSContent, new()
        {
            T res = new T();
            token ??= CancellationToken.None;
            //lock (_lockobj)
            {


                while (!token.Value.IsCancellationRequested)
                {
                    while (Messages.Count != 0)
                    {
                        var peek = Messages.Dequeue();
                        if (peek.Command == "E110") {
                            // 报警
                            var err = new E110();
                            err.Parse(peek.Content);
                            logger.LogError("{name}, E110报警 错误: {err}", Name, err.ErrorCode);

                            throw new Exception(MainStorage.GetError(err.ErrorCode));
                        }
                        if (peek.Command == res.Type)
                        {
                            res.Parse(peek.Content);
                            return res;
                        }

                    }
                    Thread.Sleep(100);
                    
                }
            }

            throw new TimeoutException("等待超时");
        }





    }
}
