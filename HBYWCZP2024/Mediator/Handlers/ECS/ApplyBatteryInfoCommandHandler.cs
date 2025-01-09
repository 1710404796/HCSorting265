using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Models;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Service;
using HBYWCZP2024.Utils;
using MediatR;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    public class ApplyBatteryInfoCommandHandler(IHBECSServiceFactory hBECSServiceFactory, IMediator mediator) : IRequestHandler<ApplyBatteryInfoCommand, BatteryLevel[]?>
    {

        public async Task<BatteryLevel[]?> Handle(ApplyBatteryInfoCommand request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);
            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.ApplyMaterialEnable)
            {

            }

            var pallet = CZPUtils.GetPalletByInsideNum(request.Key, request.StationNum);

            if (pallet == null)
            {
                throw new Exception("托盘不存在");
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
                   await pallet.BindBattery(data.i, code, true, BatteryLevel.OK);
                })
                    ;

                return data.BatteryCodes.Select(s => !string.IsNullOrWhiteSpace(s) ? BatteryLevel.OK : BatteryLevel.EMPTY).ToArray();
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
