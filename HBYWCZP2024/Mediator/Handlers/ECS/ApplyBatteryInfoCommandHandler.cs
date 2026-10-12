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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
//using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    public class ApplyBatteryInfoCommandHandler(IHBECSServiceFactory hBECSServiceFactory, IMediator mediator, IDbContextFactory<LinxContext> dbContextFactory, ILogger<ApplyBatteryInfoCommandHandler> logger) : IRequestHandler<ApplyBatteryInfoCommand, BatteryLevel[]?>
    {
        static readonly ConcurrentDictionary<string, (DateTime Time, BatteryLevel[] Levels)> _batteryInfoCache = new();
        static readonly TimeSpan _cacheWindow = TimeSpan.FromSeconds(5);

        public static void ClearCache(string palletCode)
        {
            _batteryInfoCache.TryRemove(palletCode, out _);
        }

        bool isFake(string code)
        {
            return 
                //code.Length == 24 && 
                code.All(c => c == 'J');
        }

        static BatteryLevel GetScreenBatteryLevel(char status, string? responseStepCode, string firstStepCode, string secondStepCode)
        {
            return status switch
            {
                '0' => BatteryLevel.EMPTY,
                '1' => BatteryLevel.OK,
                '2' => BatteryLevel.NG,
                '3' when string.Equals(responseStepCode?.Trim(), firstStepCode.Trim(), StringComparison.OrdinalIgnoreCase) => BatteryLevel.复测1,
                '3' when string.Equals(responseStepCode?.Trim(), secondStepCode.Trim(), StringComparison.OrdinalIgnoreCase) => BatteryLevel.复测2,
                '3' => BatteryLevel.NG2,
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

            if (_batteryInfoCache.TryGetValue(pallet.PalletCode, out var cached)
                && DateTime.UtcNow - cached.Time < _cacheWindow)
            {
                var cachedSummary = string.Join(",", cached.Levels.Select((lv, i) => $"{i + 1}={lv}"));
                logger.LogInformation($"二次下发给PLC电芯信息, 托盘: {pallet.PalletCode}, 通道状态: {cachedSummary}");
                return cached.Levels;
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
                        var statusSummary = new StringBuilder();
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
                            statusSummary.Append($"{channel.ChannelId}={finalLevel},");
                       }
                        logger.LogInformation("给PLC下发电芯状态, 托盘条码: {PalletCode}, 通道状态: {Status}",
                            pallet.PalletCode, statusSummary.ToString().TrimEnd(','));
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

                    await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                    context.PalletLogs.Add(palletlog);

                    await context.SaveChangesAsync(cancellationToken);

                }
                catch (Exception ex)
                {

                    logger.LogError(ex, $"{request.Key},{request.StationNum}组盘日志发生错误");
                }

                //return data.BatteryCodes.Select(s => !string.IsNullOrWhiteSpace(s) ? BatteryLevel.OK : BatteryLevel.EMPTY).ToArray();
                var result = pallet.Channels.Take(pallet.ChannelNum).Select(channel => channel.BatteryLevel).ToArray();
                RuntimeStateStore.Save();
                _batteryInfoCache[pallet.PalletCode] = (DateTime.UtcNow, result);
                return result;

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

                var screenData = screenRes.Match(Succ: s => s.Data, Fail: _ => null);
                var cellStrs = screenData?.CellStrs;
                if (string.IsNullOrWhiteSpace(cellStrs))
                {
                    await mediator.Send(new WcsDataWarningBoxCommand("FMS返回电芯状态为空"));
                    return null;
                }

                var responseStepCode = screenData?.StepCode;
                var firstStepCode = MainStorage.Saves.KeysSave[request.Key].StepCode ?? string.Empty;
                var secondStepCode = MainStorage.Saves.KeysSave[request.Key].StepCode2 ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(responseStepCode) && (responseStepCode != firstStepCode && responseStepCode != secondStepCode))
                {
                    // await mediator.Send(new WcsDataWarningBoxCommand($"FMS返回的工序码：{responseStepCode}与配置的工序码不匹配请检查，配置值：{firstStepCode}，{secondStepCode}"));
                    throw new InvalidOperationException($"FMS返回的工序码：{responseStepCode}与配置的工序码不匹配请检查，配置值：{firstStepCode}，{secondStepCode}");
                    // return null;
                }

               //筛选电芯
                var screenStatusSummary = new StringBuilder();
               for (int i = 0; i < pallet.ChannelNum; i++)
               {
                   var status = i < cellStrs.Length ? cellStrs[i] : '0';
                   var level = GetScreenBatteryLevel(status, responseStepCode, firstStepCode, secondStepCode);
                   await pallet.BindBattery(i, pallet.Channels[i].Code, true, level);
                   var plcCode = CZPUtils.GetPlcBatteryLevelCode(level);
                    screenStatusSummary.Append($"{i + 1}={level}->{plcCode},");
               }
                logger.LogInformation("给PLC下发电芯状态及PLC映射, 托盘条码: {PalletCode}, 通道状态: {Status}",
                    pallet.PalletCode, screenStatusSummary.ToString().TrimEnd(','));



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

                    await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                    context.PalletLogs.Add(palletlog);

                    await context.SaveChangesAsync(cancellationToken);

                }
                catch (Exception ex)
                {

                    logger.LogError(ex, $"{request.Key},{request.StationNum}组盘日志发生错误");
                }

                var result = pallet.Channels.Take(pallet.ChannelNum).Select(channel => channel.BatteryLevel).ToArray();
                RuntimeStateStore.Save();
                _batteryInfoCache[pallet.PalletCode] = (DateTime.UtcNow, result);
                return result;

         
         
            }
            else
            {
                throw new Exception("托盘类型错误");

            }


        }
    }
}
