using HBYWCZP2024.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Queries
{
    public record MaterialOnlineQuery(string Key, ImmutableArray<Channel> Channels) 
        : IRequest<Arr<Channel>>;


}
