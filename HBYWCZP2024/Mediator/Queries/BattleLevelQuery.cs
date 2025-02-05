using HBYWCZP2024.Models;
using LanguageExt.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Queries
{
    public record BattleLevelQuery(string Key, Pallet Pallet) : IRequest<Pallet>;
    // 直接返回level吧

    public record Pallet(string PalletCode, ImmutableArray<Channel> Channels);
    // ChannelId从1开始
    public record Channel (int ChannelId, Option<BatteryWithLevel> BattleWithLevel);

    public record BatteryWithLevel(string Code, Option<BatteryLevel> Level);
}
