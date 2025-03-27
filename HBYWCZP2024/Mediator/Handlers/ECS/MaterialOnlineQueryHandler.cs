using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Models;
using HBYWCZP2024.Service;
using HBYWCZP2024.Service.Models;
using HBYWCZP2024.Utils;
using LanguageExt.UnitsOfMeasure;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    public class MaterialOnlineQueryHandler(IHBECSServiceFactory factory, IMediator mediator) : IRequestHandler<MaterialOnlineQuery, Arr<Channel>>
    {
        public async Task<Arr<Channel>> Handle(MaterialOnlineQuery request, CancellationToken cancellationToken)
        {
            // 先调用MES获取拆组盘信息
            // 再调用MES高温静置入站

            bool isNG(string s) => s.StartsWith("noread", StringComparison.CurrentCultureIgnoreCase) || string.IsNullOrWhiteSpace(s); 



            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.CheckMaterialEnable)
            {
                return request.Channels.Map(c =>
                    c with
                    {
                        BattleWithLevel = c.BattleWithLevel
                                            .Map(s => s with { Level = isNG(s.Code) ? BatteryLevel.NG : BatteryLevel.OK })
                    }
                ).ToArr();
            }
            
            var service = factory.Create(request.Key);
            if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.HCInStationEnable)
            {
                var okCodes = request.Channels
                .Map(c => c.BattleWithLevel.Map(s => s.Code))
                .Map(s => s.IfNone(""))

                .Filter(s => !isNG(s)).ToArr();
                // 验证器
                // 高温静置入站
                var rss = okCodes
                            .AsParallel()
                            .Map(s =>
                            {
                                return service.MesService.HCInStationAsync(new HCInStationRequest(request.Key)
                                {
                                    identification = s,
                                }).Result;
                            });
                if (!rss.All(s => s.IsSuccess))
                {
                    var message = string.Join("\n",
                        rss
                        .Filter(s => !s.IsSuccess)
                        .Map(s => s.Match(Succ: su => "", Fail: fa => fa.Message)));
                    await mediator.Send(new WcsDataWarningBoxCommand("进站MES报警： " + message));
                    throw new Exception(message);
                }
            }






            if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.FXQueryEnable)
            {
                FxCheckRequest fxCheckRequest = new FxCheckRequest(request.Key)
                {
                    MateriallotCodeList = 
                        request.Channels
                        .Map(s => s.BattleWithLevel)
                        .Choose(s => s)
                        .Map(s => s.Code)
                        .ToArray()
                };
                async Task<Map<string, BatteryLevel>> getCodeMap(FxCheckRequest request)
                {
                    if (request.MateriallotCodeList.Length <= 0)
                    {
                        return new Map<string, BatteryLevel>();
                    }
                    var checkMaterialResponse = await service.MesService.FxCheckAsync(fxCheckRequest);

                    var cmap = checkMaterialResponse.Match(
                        Succ: r => {
                            var channels = r.Rows.Select(x =>
              
                                    (x.identification, CZPUtils.GetLevelFromMes(x.rank))
                               
                            );
                            return channels;
                        },
                        Fail: x => {

                            mediator.Send(new WcsDataWarningBoxCommand(x.Message)).Wait();
                            throw x;
                        }


                        ).DistinctBy(s => s.identification).ToMap();
                    return cmap;
                }
                
                var codeMap = await getCodeMap(fxCheckRequest);

                var data = request.Channels.Map(c =>
                    c with
                    {
                        BattleWithLevel = c.BattleWithLevel.Map
                        (b =>
                        new BatteryWithLevel(
                            b.Code,
                            b.Code switch
                            {
                                var x when isNG(x) => BatteryLevel.NG,
                                _ => codeMap[b.Code]
                            }
                        ))
                    }

                    ).ToArr();



                return data;
            }

            else if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.GRegionFXQueryEnable)
            {
                CheckPalletMaterialRequest checkMaterialRequest = new CheckPalletMaterialRequest(request.Key)
                {
                    ContainerCode = "",
                    ContainerCellInfos = request.Channels.Select(x => new ContainerCellInfo()
                    {
                        Location = x.ChannelId.ToString(),
                        Identification = x.BattleWithLevel.Match(
                            Some: battleWithLevel => battleWithLevel.Code,
                            None: string.Empty
                        ),

                    })
                    .Filter(s => !isNG(s.Identification))
                    .ToArray()
                };

                async Task<Map<string, BatteryLevel>> getCodeMap(CheckPalletMaterialRequest request)
                {
                    if (request.ContainerCellInfos.Length <= 0)
                    {
                        return new Map<string, BatteryLevel>();
                    }
                    var checkMaterialResponse = await service.MesService.CheckPalletMaterialLevel(checkMaterialRequest);

                    var cmap = checkMaterialResponse.Match(
                        Succ: r => {
                            var channels = r.Rows.Select(x =>

                                    (x.Identification, CZPUtils.GetLevelFromMes(x.Grade))

                            );
                            return channels;
                        },
                        Fail: x => {

                            mediator.Send(new WcsDataWarningBoxCommand(x.Message)).Wait();
                            throw x;
                        }


                        ).DistinctBy(s => s.Identification).ToMap();
                    return cmap;
                }

                var codeMap = await getCodeMap(checkMaterialRequest);

                var data = request.Channels.Map(c =>
                    c with
                    {
                        BattleWithLevel = c.BattleWithLevel.Map
                        (b =>
                        new BatteryWithLevel(
                            b.Code,
                            b.Code switch
                            {
                                var x when isNG(x) => BatteryLevel.NG,
                                _ => codeMap[b.Code]
                            }
                        ))
                    }

                    ).ToArr();



                return data;
            }
            else
            {
                CheckPalletMaterialRequest checkMaterialRequest = new(request.Key)
                {
                    ContainerCode = "",
                    ContainerCellInfos = request.Channels.Select(x => new ContainerCellInfo()
                    {
                        Location = x.ChannelId.ToString(),
                        Identification = x.BattleWithLevel.Match(
                            Some: battleWithLevel => battleWithLevel.Code,
                            None: string.Empty
                        ),

                    })
                    .Filter(s => !isNG(s.Identification))
                    .ToArray()
                };
                //var res = await service.MesService.CheckPalletMaterialLevel(checkMaterialRequest);

                async Task<string[]> GetMarkBatterys(CheckPalletMaterialRequest request1)
                {
                    if (
                !MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.HHFilterQueryEnable
                        )
                    {
                        return [];
                    }
                    if (request1.ContainerCellInfos.Length <= 0)
                    {
                        return [];
                    }
                    var res = await service.MesService.CheckPalletMaterialLevel(request1);

                    return res.Match(
                        Succ: s => s.Rows.Filter(s => CZPUtils.GetLevelFromMes(s.Grade) == BatteryLevel.Mark).Map(s => s.Identification),
                        Fail: f =>
                        {
                            mediator.Send(new WcsDataWarningBoxCommand(f.Message)).Wait();
                            throw f;
                        }
                    ).ToArray();

                }

                var markBatterys = await GetMarkBatterys(checkMaterialRequest);



                var mRequest = new MaterialOnlineRequest(request.Key)
                {
                    batteryTrayBindVOList = [.. request.Channels
                    .Filter(s =>
                        s.BattleWithLevel.Match(s => !markBatterys.Contains(s.Code), () => false)
                        )
                    .Filter(s =>
                        s.BattleWithLevel
                        .Map(s => !isNG(s.Code))
                        .IfNone(false)
                        //.Code.StartsWith("Noread", StringComparison.CurrentCultureIgnoreCase))
                    )
                 .Map(c =>
                 new Batterytraybindvolist
                 { identification = c.BattleWithLevel
                                    .Map(b => b.Code)
                                    .IfNone("")
                                    //.Match(Some: s => s, None: () => "")
                 })]
                };
                async Task<(Map<string, BatteryLevel> comap, string mid)> getCodeMap(MaterialOnlineRequest request)
                {
                    if (request.batteryTrayBindVOList.Length <= 0)
                    {
                        return (new Map<string, BatteryLevel>(), "");
                    }
                    var res = await service.MesService.MaterialOnline(mRequest);
                    var cmap = res.Match(
                        Succ: s => s.rows.Map(r =>
                        (r.identification ?? "", CZPUtils.GetLevelFromMes(r.qualityStatus ?? "")
                        //r.qualityStatus?.ToLower() switch
                        //{
                        //    "ok" => BatteryLevel.OK,
                        //    "ng" => BatteryLevel.NG,
                        //    _ => BatteryLevel.NG,
                        //}
                        )
                        ),
                    Fail: f =>
                    {

                        mediator.Send(new WcsDataWarningBoxCommand(f.Message)).Wait();
                        throw f;
                    }


                    ).DistinctBy(s => s.Item1).ToMap();
                    var id = res.Map(s => s.rows.Map(s => s.materialId).Distinct()).Match(
                    Succ: s => s.FirstOrDefault(), Fail: f =>
                    {

                        mediator.Send(new WcsDataWarningBoxCommand(f.Message)).Wait();
                        throw f;
                    }
                    );
                    return (cmap, id ?? "");
                }

                var (codeMap, sthMaterialId) = await getCodeMap(mRequest);

                if (!string.IsNullOrEmpty(sthMaterialId))
                {
                    MainStorage.Saves.KeysSave[request.Key].MaterialCode = sthMaterialId ?? string.Empty;

                }

                var data = request.Channels.Map(c =>
                c with
                {
                    BattleWithLevel = c.BattleWithLevel.Map
                    (b =>
                    new BatteryWithLevel(
                        b.Code,
                        b.Code switch
                        {
                            var x when markBatterys.Contains(x) => BatteryLevel.Mark,
                            var x when isNG(x) => BatteryLevel.NG,
                            _ => codeMap[b.Code]
                        }
                    ))
                }

                ).ToArr();



                return data;
            }

        }
    }
}
