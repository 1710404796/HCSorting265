using HBYWCZP2024.Mediator.Queries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Commands
{
    public record FetchStationCommand(string name, int stationNum) : IRequest<Pallet>;
}
