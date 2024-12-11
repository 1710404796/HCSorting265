using HBYWCZP2024.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Queries
{
    public record BattleLevelQuery(IEnumerable<BattleWithLevel> Battles) : IRequest<IEnumerable<BattleWithLevel>>;
    // 直接返回level吧
    public record BattleWithLevel(string Code, BatteryLevel Level);
}
