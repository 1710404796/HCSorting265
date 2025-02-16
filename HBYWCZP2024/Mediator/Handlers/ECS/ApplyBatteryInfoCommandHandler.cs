using HBYWCZP2024.Data;
using HBYWCZP2024.Data.Models;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Models;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Service;
using HBYWCZP2024.Utils;
using MediatR;
using Microsoft.Extensions.Logging;
//using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    public class ApplyBatteryInfoCommandHandler(IHBECSServiceFactory hBECSServiceFactory, IMediator mediator, LinxContext context, ILogger<ApplyBatteryInfoCommandHandler> logger) : IRequestHandler<ApplyBatteryInfoCommand, BatteryLevel[]?>
    {
        bool isFake(string code)
        {
            return 
                //code.Length == 24 && 
                code.All(c => c == 'J');
        }
        public async Task<BatteryLevel[]?> Handle(ApplyBatteryInfoCommand request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);
            
            var pallet = CZPUtils.GetPalletByInsideNum(request.Key, request.StationNum);

            if (pallet == null)
            {
                throw new Exception("托盘不存在");
            }
            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.ApplyMaterialEnable)
            {
                return Enumerable.Repeat(0, 24).Select(s => BatteryLevel.EMPTY).ToArray();
            }

            if (pallet.Type == Models.CZPallet.PalletType.拆盘)
            {
                var data = await hBECSService.SendAsync<S103, E103>(new() { TrayNumber = pallet.PalletCode, TrayPosition = pallet.PalletId + 1, TrayType = pallet.OKNGType });
                if (data == null)
                {
                    await mediator.Send(new WcsDataWarningBoxCommand("ECS通讯异常"));
                    return null;
                }

                data.BatteryCodes.Select((s,i) => (s, i)).ToList().ForEach(async (data) =>
                {
                    var code = data.s.Trim();
                    if (isFake(code))
                    {
                        await pallet.BindBattery(data.i, code, true, BatteryLevel.Fake);
                    }
                    else
                    {
                        await pallet.BindBattery(data.i, code, true, BatteryLevel.OK);
                    }
                })
                    ;
                // 存日志
                try
                {
                    // 存一下托盘日志
                    var palletlog = new PalletLog
                    {
                        PalletCode = pallet.PalletCode,
                        BatteryInfo = pallet.Channels.Select(s => new BatteryInfo
                        {
                            BatteryCode = s.Code,
                            BatteryLevel = s.BatteryLevel,
                        }).Take(pallet.ChannelNum).ToList(),
                        Column = pallet.Column,
                        PalletType = pallet.Type

                    };

                    context.Add(palletlog);

                    await context.SaveChangesAsync(cancellationToken);

                }
                catch (Exception ex)
                {

                    logger.LogError(ex, "组盘日志发生错误");
                }

                //return data.BatteryCodes.Select(s => !string.IsNullOrWhiteSpace(s) ? BatteryLevel.OK : BatteryLevel.EMPTY).ToArray();
                return pallet.Channels.Take(pallet.ChannelNum).Select(channel => channel.BatteryLevel).ToArray();

            }
            else if (pallet.Type == Models.CZPallet.PalletType.筛选)
            {
                var data = await hBECSService.SendAsync<S103, E103>(new() { TrayNumber = pallet.PalletCode, TrayPosition = pallet.PalletId + 1, TrayType = pallet.OKNGType });
                if (data == null)
                {
                    await mediator.Send(new WcsDataWarningBoxCommand("ECS通讯异常"));
                    return null;
                }

                var palletR = new Pallet(data.TrayNumber, 
                    data.BatteryCodes
                    .Select((x, i) => new Channel(i + 1, new BatteryWithLevel(x, None)))
                    .Filter(s => !string.IsNullOrWhiteSpace( s.BattleWithLevel.Map(s => s.Code).IfNone("")))
                    .Filter(s => !isFake(s.BattleWithLevel.Map(s => s.Code).IfNone("")))
                    
                    .ToImmutableArray()
                    );
                // 这里有条码 // bind一下去
                var palletAfterQuery = await mediator.Send(new BattleLevelQuery(request.Key, palletR));

                if (palletAfterQuery == null)
                {
                    await mediator.Send(new WcsDataWarningBoxCommand("MES通讯异常"));
                    return null;
                }

                foreach (var channel in palletAfterQuery.Channels)
                {
                    var (code, lv) = channel.BattleWithLevel.Match(
                        Some: battleWithLevel => (battleWithLevel.Code, battleWithLevel.Level.Match(Some: l => l, None: BatteryLevel.EMPTY)),
                        None: (string.Empty, BatteryLevel.EMPTY)
                    );
                    await pallet.BindBattery(channel.ChannelId - 1, code, true, lv);
                }

                // 存日志
                try
                {
                    // 存一下托盘日志
                    var palletlog = new PalletLog
                    {
                        PalletCode = pallet.PalletCode,
                        BatteryInfo = pallet.Channels.Select(s => new BatteryInfo
                        {
                            BatteryCode = s.Code,
                            BatteryLevel = s.BatteryLevel,
                        }).Take(pallet.ChannelNum).ToList(),
                        Column = pallet.Column,
                        PalletType = pallet.Type

                    };

                    context.Add(palletlog);

                    await context.SaveChangesAsync(cancellationToken);

                }
                catch (Exception ex)
                {

                    logger.LogError(ex, "组盘日志发生错误");
                }

                return pallet.Channels.Take(pallet.ChannelNum).Select(channel => channel.BatteryLevel).ToArray();


                //return palletAfterQuery.Channels.Select(x => x.BattleWithLevel.Match(
                //    Some: battleWithLevel => battleWithLevel.Level.Match(
                //        Some: level => level,
                //        None: BatteryLevel.Unknown
                //    ),
                //    None: BatteryLevel.Unknown
                //)).ToArray();



            }
            else
            {
                throw new Exception("托盘类型错误");
            }


        }
    }
}
