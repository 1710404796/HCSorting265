using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Service;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    internal class GrabCommandHandler(IHBECSServiceFactory hBECSServiceFactory) : IRequestHandler<GrabCommand, bool>
    {
        public Task<bool> Handle(GrabCommand request, CancellationToken cancellationToken)
        {

            //throw new NotImplementedException();
            // Todo: 新增MES接口 // 无需新增
            return Task.FromResult (true);
        }
    }
}
