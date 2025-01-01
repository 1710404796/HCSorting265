using HBYWCZP2024.Mediator.ECS;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    internal class GrabCommandHandler : IRequestHandler<GrabCommand, bool>
    {
        public Task<bool> Handle(GrabCommand request, CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            return Task.FromResult (true);
        }
    }
}
