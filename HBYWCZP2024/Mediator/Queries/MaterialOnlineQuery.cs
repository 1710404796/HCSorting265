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
    /// <summary>
    /// 拉带任务电芯检测
    /// </summary>
    /// <param name="Key"></param>
    /// <param name="Channels"></param>
    public record MaterialOnlineQuery(string Key, ImmutableArray<Channel> Channels) 
        : IRequest<Arr<Channel>>;


}
