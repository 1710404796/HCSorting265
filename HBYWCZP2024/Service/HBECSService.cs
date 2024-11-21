using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Utils;
using LinxUniverse.SciSocket;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
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

        public HBECSService(string name, ILogger<HBECSService> logger)
        {
            Name = name;
            this.logger = logger;
            _clientSocket = new LinxClientSocket(new LinxClientSocketOption
            {
                Logger = logger,
                Address = MainStorage.Saves.KeysSave[name].ECSIP,
                Port = MainStorage.Saves.KeysSave[name].ECSPort,
                SplitChar = '#',
            });
            Init();
        }

        public void Init()
        {
            _clientSocket.OnComplete += ClientSocket_OnComplete;
            _clientSocket.RecviceLine += ClientSocket_RecviceLine;

            _clientSocket.Connect();
        }

        private void ClientSocket_RecviceLine(string obj)
        {
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
        public async Task<T2> SendAsync<T1, T2>(T1 req) where T1 : IECSContent where T2 : IECSContent, new()
        {
            await Task.Yield();
            lock (_lockobj)
            {
                var packMessage = MessageHelper.GetSocketMessage(req.Type, MainStorage.Saves.KeysSave[Name].DeviceCode, req.GetContent());

                _clientSocket.Send(Encoding.UTF8.GetBytes(packMessage));
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(60000);

                var res = WaitFor<T2>(cts.Token);
                return res;

            }

        }

        public T WaitFor<T>(CancellationToken? token) where T : IECSContent, new()
        {
            T res = new T();
            token ??= CancellationToken.None;
            lock (_lockobj)
            {


                while (!token.Value.IsCancellationRequested)
                {
                    while (Messages.Count != 0)
                    {
                        var peek = Messages.Dequeue();

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
