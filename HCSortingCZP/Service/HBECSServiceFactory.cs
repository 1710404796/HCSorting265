using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Service
{
    public class HBECSServiceFactory(ILogger<HBECSService> logger) : IHBECSServiceFactory
    {
        private readonly Dictionary<string, HBECSService> _serviceCache = [];
        public HBECSService Create(string name)
        {
            if (!_serviceCache.ContainsKey(name))
            {
                var meditor = App.GetService<IMediator>();
                var service = new HBECSService(name, logger , meditor);
                _serviceCache[name] = service;
            }

            return _serviceCache[name];
        }
    }
}
