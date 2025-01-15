using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.Queries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers
{
    public class FetchStationCommandHandler : IRequestHandler<FetchStationCommand, Pallet>
    {
        public Task<Pallet> Handle(FetchStationCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
