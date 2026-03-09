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

        static BatteryLevel GetScreenBatteryLevel(char status)
        {
            return status switch
            {
                '0' => BatteryLevel.EMPTY,
                '1' => BatteryLevel.OK,
                '2' => BatteryLevel.NG,
                _ => BatteryLevel.EMPTY,
            };
        }
        public async Task<BatteryLevel[]?> Handle(ApplyBatteryInfoCommand request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);
            
            var pallet = CZPUtils.GetPalletByInsideNum(request.Key, request.StationNum);

            if (pallet == null)
            {
                throw new Exception($"{request.Key},{request.StationNum}托盘不存在");
            }
            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.ApplyMaterialEnable)
            {
                return Enumerable.Repeat(0, 196).Select(s => BatteryLevel.OK).ToArray();
            }

            if (pallet.Type == Models.CZPallet.PalletType.拆盘)
            {
                //var data = await hBECSService.SendAsync<S103, E103>(new() { TrayNumber = pallet.PalletCode, TrayPosition = pallet.PalletId + 1, TrayType = pallet.OKNGType }, pallet.OutsideStationNum);
                //if (data == null)
                //{
                //    await mediator.Send(new WcsDataWarningBoxCommand("ECS通讯异常"));
                //    return null;
                //}

                //data.BatteryCodes.Select((s,i) => (s, i)).ToList().ForEach(async (data) =>
                //{
                //    var code = data.s.Trim();
                //    if (isFake(code))
                //    {
                //        await pallet.BindBattery(data.i, code, true, BatteryLevel.Fake);
                //    }
                //    else
                //    {
                //        await pallet.BindBattery(data.i, code, true, BatteryLevel.OK);
                //    }
                //})
                    ;
                // 存日志

                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.ContainerManualEnable)
                {
                    if (string.IsNullOrWhiteSpace(pallet.PalletCode))
                    {
                        await mediator.Send(new WcsDataWarningBoxCommand($"{request.Key},{request.StationNum}托盘条码为空"), cancellationToken);
                        return null;
                    }
                    try
                    {
                        var mesChannels = await mediator.Send(new ContainerManualCommand(request.Key, pallet.PalletCode), cancellationToken);
                        foreach (var channel in mesChannels)
                        {
                            if (channel.ChannelId <= 0 || channel.ChannelId > pallet.ChannelNum) continue;
                            var (code, lv) = channel.BattleWithLevel.Match(
                                Some: battleWithLevel => (battleWithLevel.Code, battleWithLevel.Level.Match(Some: l => l, None: BatteryLevel.EMPTY)),
                                None: (string.Empty, BatteryLevel.EMPTY)
                            );
                            if (string.IsNullOrWhiteSpace(code)) continue;
                            var finalLevel = isFake(code) ? BatteryLevel.Fake : lv;
                            await pallet.BindBattery(channel.ChannelId - 1, code, true, finalLevel);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"{request.Key},{request.StationNum}托盘手动查询MES发生错误");
                        await mediator.Send(new WcsDataWarningBoxCommand($"{request.Key},{request.StationNum}托盘手动查询MES异常: {ex.Message}"), cancellationToken);
                        return null;
                    }
                }

                
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

                    logger.LogError(ex, $"{request.Key},{request.StationNum}组盘日志发生错误");
                }

                //return data.BatteryCodes.Select(s => !string.IsNullOrWhiteSpace(s) ? BatteryLevel.OK : BatteryLevel.EMPTY).ToArray();
                return pallet.Channels.Take(pallet.ChannelNum).Select(channel => channel.BatteryLevel).ToArray();

            }
            else if (pallet.Type == Models.CZPallet.PalletType.筛选)
            {
                var fmsService = hBECSService.NewFMSService;
                var screenRes = await fmsService.YZScreenRecipeAsync(new YZScreenRecipeRequest(request.Key)
                {
                    trayCode = pallet.PalletCode
                });

                if (!screenRes.IsSuccess)
                {
                    var err = screenRes.Match(Succ: _ => string.Empty, Fail: f => f.Message);
                    await mediator.Send(new WcsDataWarningBoxCommand($"FMS通讯异常: {err}"));
                    return null;
                }

                var cellStrs = screenRes.Match(Succ: s => s.Data?.CellStrs, Fail: _ => null);
                if (string.IsNullOrWhiteSpace(cellStrs))
                {
                    await mediator.Send(new WcsDataWarningBoxCommand("FMS返回电芯状态为空"));
                    return null;
                }

                for (int i = 0; i < pallet.ChannelNum; i++)
                {
                    var status = i < cellStrs.Length ? cellStrs[i] : '0';
                    var level = GetScreenBatteryLevel(status);
                    await pallet.BindBattery(i, pallet.Channels[i].Code, true, level);
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

                    logger.LogError(ex, $"{request.Key},{request.StationNum}组盘日志发生错误");
                }

                return pallet.Channels.Take(pallet.ChannelNum).Select(channel => channel.BatteryLevel).ToArray();


         
            }
            else
            {
                throw new Exception("托盘类型错误");

            }


        }
    }
}
