using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Utils;
using LinxUniverse.SciSocket;
using Microsoft.Extensions.Logging;
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
                Address = MainStorage.Saves.ECSIP,
                Port = MainStorage.Saves.ECSPort,
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
            throw new NotImplementedException();
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

        public async Task<T> WaitFor<T> ( CancellationToken? token) where T : IECSContent, new()
        {
            T res = new T();
            token ??= CancellationToken.None;
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
                await Task.Delay(200);
            }
            return default;
        }





    }
}
