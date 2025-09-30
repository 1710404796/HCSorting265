using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Service;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    internal class PalletStatusChangeCommandHandler(IHBECSServiceFactory hBECSServiceFactory,
        //NewECSService newMesService,
        IMediator mediator) : IRequestHandler<PalletStatusChangeCommand, bool>
    {
        public Task<bool> Handle(PalletStatusChangeCommand request, CancellationToken cancellationToken)
        {
            var hBECSService = hBECSServiceFactory.Create(request.name);



            throw new NotImplementedException();
        }
    }
}
