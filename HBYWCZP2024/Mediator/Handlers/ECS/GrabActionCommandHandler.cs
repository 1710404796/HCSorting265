using HBYWCZP2024.Data;
using HBYWCZP2024.Data.Models;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Models;
using HBYWCZP2024.Models.CZPallet;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Service;
using HBYWCZP2024.Service.Models;
using HBYWCZP2024.Utils;
using LanguageExt.UnitsOfMeasure;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    public class GrabActionCommandHandler(IHBECSServiceFactory hBECSServiceFactory,
        IMediator mediator, LinxContext context, ILogger<GrabActionCommandHandler> logger) : IRequestHandler<GrabActionCommand, bool>
    {
        public async Task<bool> Handle(GrabActionCommand request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);
            var ecsService = hBECSService.NewECSService;
            var fmsService = hBECSService.NewFMSService;

            var pallet = CZPUtils.GetPalletByOutsideNum(request.Key, request.StationNum);
            bool isCodeRuleNG(string s) => s.StartsWith("noread", StringComparison.CurrentCultureIgnoreCase) || string.IsNullOrWhiteSpace(s);
            bool isCodeRuleNG1(string s) => s.StartsWith("noread", StringComparison.CurrentCultureIgnoreCase);
            if (pallet == null)
            {
                throw new Exception($"{request.Key},{request.StationNum}托盘不存在");
            }

            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.GrabActionInsertEnable)
            {
                if (pallet.Type == Models.CZPallet.PalletType.组盘)
                {
                    return true;
                }

            }
            if (request.Start)
            {
                var data = await mediator.Send(new EquipStatusReportCommand(request.Key, pallet.InsideNum, EquipStatus.R));
                //PalletStatus = request.isSpecial ? PalletStatus.不满 : PalletStatus.满盘,

                if (!data)
                {
                    await mediator.Send(new WcsDataWarningBoxCommand("ECS上报状态失败"));
                    return false;
                }

            }
            else
            {
                // 上报数据

                // 拆组盘结束。步骤较多
                // 1. 向FMS上报结果，这里感觉也需要加一下处理
                // 2. 向MES上报结果
                // 3. 向FMS上报组盘完成



                // TODO:上报数据结果权限添加

                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.UploadFmsDataEnable)
                {
                    string tempLv = "";
                    Map<string,string> codeMapTemp = new ();
                    if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.TrayLevelUpdate)
                    {

                        if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.GRegionFXQueryEnable)
                        {
                            if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.AllLevelUploadEnable)
                            {
                                CheckPalletMaterialRequest checkMaterialRequest = new CheckPalletMaterialRequest(request.Key)
                                {
                                    ContainerCode = "",
                                    ContainerCellInfos = pallet.Channels
                                    .Filter(s => s.BatteryLevel != BatteryLevel.EMPTY)
                                    .Filter(s => !isCodeRuleNG(s.Code))
                                    //.Take(1)
                                    .Select(x => new ContainerCellInfo()
                                    {
                                        Location = x.Location.ToString(),
                                        Identification = x.Code

                                    })
                                    .ToArray()
                                };
                                async Task<Map<string, string>> getCodeMap(CheckPalletMaterialRequest request)
                                {
                                    if (request.ContainerCellInfos.Length <= 0)
                                    {
                                        return [];
                                    }
                                    var checkMaterialResponse = await hBECSService.MesService.CheckPalletMaterialLevel(checkMaterialRequest);

                                    var cmap = checkMaterialResponse.Match(
                                        Succ: r =>
                                        {
                                            var channels = r.Rows.Select(x =>

                                                  (x.Identification, x.Grade)

                                            );
                                            return channels;
                                        },
                                        Fail: x =>
                                        {

                                            mediator.Send(new WcsDataWarningBoxCommand(x.Message)).Wait();
                                            throw x;
                                        }


                                        ).DistinctBy(s => s.Identification)
                                        .ToMap();
                                    return cmap;
                                }

                                codeMapTemp = await getCodeMap(checkMaterialRequest);
                                //tempLv = codeMap;
                            }
                            else
                            {

                                CheckPalletMaterialRequest checkMaterialRequest = new CheckPalletMaterialRequest(request.Key)
                                {
                                    ContainerCode = "",
                                    ContainerCellInfos = pallet.Channels
                                .Filter(s => s.BatteryLevel != BatteryLevel.EMPTY)
                                .Take(1)
                                .Select(x => new ContainerCellInfo()
                                {
                                    Location = x.Location.ToString(),
                                    Identification = x.Code

                                })
                                .ToArray()
                                };
                                async Task<string> getCodeMap(CheckPalletMaterialRequest request)
                                {
                                    if (request.ContainerCellInfos.Length <= 0)
                                    {
                                        return "";
                                    }
                                    var checkMaterialResponse = await hBECSService.MesService.CheckPalletMaterialLevel(checkMaterialRequest);

                                    var cmap = checkMaterialResponse.Match(
                                        Succ: r =>
                                        {
                                            var channels = r.Rows.Select(x =>

                                                   x.Grade

                                            );
                                            return channels;
                                        },
                                        Fail: x =>
                                        {

                                            mediator.Send(new WcsDataWarningBoxCommand(x.Message)).Wait();
                                            throw x;
                                        }


                                        ).FirstOrDefault();
                                    return cmap ?? "";
                                }

                                var codeMap = await getCodeMap(checkMaterialRequest);
                                tempLv = codeMap;
                            }

                        }
                        else if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.FXQueryEnable)
                        {
                            if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.AllLevelUploadEnable)
                            {
                                FxCheckRequest fxCheckRequest = new FxCheckRequest(request.Key)
                                {
                                    MateriallotCodeList =
                                    pallet.Channels
                                    .Filter(s => s.BatteryLevel != BatteryLevel.EMPTY)
                                    //.Take(1)
                                    .Filter(s => !isCodeRuleNG(s.Code))

                                    .Select(x => x.Code)
                                    .ToArray()
                                };
                                async Task<Map<string, string>> getCodeMap(FxCheckRequest request)
                                {
                                    //return "NG";
                                    if (request.MateriallotCodeList.Length <= 0)
                                    {
                                        return [];
                                    }
                                    var checkMaterialResponse = await hBECSService.MesService.FxCheckAsync(fxCheckRequest);

                                    var cmap = checkMaterialResponse.Match(
                                        Succ: r =>
                                        {
                                            var channels = r.Rows.Select(x =>

                                                    (x.identification,
                                                    x.rank)

                                            )
                                            //.Map(s => s.rank.ToUpper() == "C" ? "NG" : s)
                                            ;
                                            return channels;
                                        },
                                        Fail: x =>
                                        {

                                            mediator.Send(new WcsDataWarningBoxCommand(x.Message)).Wait();
                                            throw x;
                                        }


                                        ).DistinctBy(s => s.identification).ToMap();
                                    return cmap;// ?? "";
                                }
                                codeMapTemp = await getCodeMap(fxCheckRequest);
                            }
                            else
                            {

                                FxCheckRequest fxCheckRequest = new FxCheckRequest(request.Key)
                                {
                                    MateriallotCodeList =
                                    pallet.Channels
                                    .Filter(s => s.BatteryLevel != BatteryLevel.EMPTY)
                                    .Take(1)
                                    .Select(x => x.Code)
                                    .ToArray()
                                };
                                async Task<string> getCodeMap(FxCheckRequest request)
                                {
                                    //return "NG";
                                    if (request.MateriallotCodeList.Length <= 0)
                                    {
                                        return "";
                                    }
                                    var checkMaterialResponse = await hBECSService.MesService.FxCheckAsync(fxCheckRequest);

                                    var cmap = checkMaterialResponse.Match(
                                        Succ: r =>
                                        {
                                            var channels = r.Rows.Select(x =>

                                                    x.rank

                                            )
                                            .Map(s => s.ToUpper() == "C" ? "NG" : s)
                                            ;
                                            return channels;
                                        },
                                        Fail: x =>
                                        {

                                            mediator.Send(new WcsDataWarningBoxCommand(x.Message)).Wait();
                                            throw x;
                                        }


                                        ).FirstOrDefault();
                                    return cmap ?? "";
                                }

                                tempLv = await getCodeMap(fxCheckRequest);

                            }
                        }

                    }
                    TrayType Get(Pallet pallet)
                    {
                        if (pallet.Type == PalletType.筛选)
                        {
                            if (pallet.Channels.Take(pallet.ChannelNum).All(s => string.IsNullOrWhiteSpace(s.Code)))
                            {
                                //return TrayType.空筛选类型;
                                return TrayType.空托盘;
                            }
                        }
                        return pallet.OKNGType;
                    }
                    async Task<bool> GetData()
                    {
                        if (pallet.Type == PalletType.筛选)
                        {
                            if (string.IsNullOrWhiteSpace(pallet.PalletCode))
                            {
                                await mediator.Send(new WcsDataWarningBoxCommand($"{request.Key},{request.StationNum}托盘条码不能为空"));
                                return false;
                            }

                            if (MainStorage.Saves.ApiEnableTable.GRegionCvRequestEnable == false)
                            {
                                var res = await fmsService.ScreenUploadAsync(new ScreenRecipeRequest(request.Key)
                                {
                                    trayCode = pallet.PalletCode,
                                    //trayType = ((int)Get(pallet)).ToString(),
                                    trayType = pallet.OKNGType.GetTrayTypeStr(), // TODO: 特殊任务特殊传
                                    CellList = pallet.Channels.Take(pallet.ChannelNum).Select((s, i) => new CellInfo
                                    {
                                        CellCode = s.Code,
                                        Channel = i + 1,
                                        GradingType =
                                        MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.TrayLevelUpdate ?
                                        (
                                            codeMapTemp.Find(s.Code).Match(
                                                Some: lv => CZPUtils.GetLevelFromMes(lv),
                                                None: () => isCodeRuleNG1(s.Code) ? BatteryLevel.Fake : s.BatteryLevel
                                            )
                                        ).ToString() :
                                        (
                                            tempLv.ToUpper() == "NG" ? BatteryLevel.NG :
                                            isCodeRuleNG1(s.Code) ? BatteryLevel.Fake : s.BatteryLevel
                                        ).ToString(),
                                        CellType = s.GetCellType()
                                    })
                                    .Filter(s => !string.IsNullOrWhiteSpace(s.CellCode))
                                    .ToArray()
                                });
                                return res.IsSuccess;
                            }

                            return true;
                        }
                        else if (pallet.Type == PalletType.组盘)
                        {
                            // 一注组盘可能要传
                            //var res = await fmsService.MpUploadAsync(new MpUploadRequest(request.Key)
                            //{
                            //    trayCode = pallet.PalletCode,
                            //    cellCodes = pallet.Channels.Take(pallet.ChannelNum).Select((s, i) => new Cellcode
                            //    {
                            //        cellCode = s.Code,
                            //        channel = i + 1,
                            //        gradingType = MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.TrayLevelUpdate ?
                            //        (
                            //            codeMapTemp.Find(s.Code).Match(
                            //                Some: lv => lv,
                            //                None: () => !isCodeRuleNG1(s.Code) ? BatteryLevel.Fake.ToString() : s.BatteryLevel.ToString()
                            //            )
                            //        ) :
                            //        (
                            //            tempLv.ToUpper() == "NG" ? BatteryLevel.NG :
                            //            isCodeRuleNG1(s.Code) ? BatteryLevel.Fake : s.BatteryLevel
                            //        ).ToString(),
                            //        cellType = s.GetCellType()
                            //    })
                            //     .Filter(s => !string.IsNullOrWhiteSpace(s.cellCode))

                            //    .ToArray()
                            //    //cellCodes =  s
                            //}
                            //);
                            //return res.IsSuccess;




                            var cells = pallet.Channels.Take(pallet.ChannelNum).Select((s, i) => new Cellcode
                            {
                                cellCode = s.Code,
                                channel = i + 1,
                                gradingType = MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.TrayLevelUpdate ?
                                (
                                codeMapTemp.Find(s.Code).Match(
                                    Some: lv => lv,
                                    None: () => !isCodeRuleNG1(s.Code) ? BatteryLevel.Fake.ToString() : s.BatteryLevel.ToString()
                                    )
                                ) :
                                (
                                        tempLv.ToUpper() == "NG" ? BatteryLevel.NG :
                                        isCodeRuleNG1(s.Code) ? BatteryLevel.Fake : s.BatteryLevel
                                    ).ToString(),
                                cellType = s.GetCellType()
                            }).Filter(s => !string.IsNullOrWhiteSpace(s.cellCode)).ToArray();

                            // 检查是否整盘都是NG
                            //bool allNG = cells.Length > 0 && cells.All(cell => cell.gradingType?.ToUpper() == BatteryLevel.NG.ToString().ToUpper() || cell.gradingType?.ToUpper() == "NOREAD");
                            //bool allNG = cells.Length > 0 && cells.All(cell => cell.gradingType?.ToUpper() == BatteryLevel.NG.ToString().ToUpper());
                            //bool allNG = cells.Length > 0 && cells.All(cell => cell.cellCode?.ToUpper() == "NOREAD");


                            // 启用则上报FMS  否则不上报  默认不启用
                            if (pallet.OKNGType == TrayType.NG托盘)
                            {
                                if (MainStorage.Saves.ApiEnableTable.NGStationUpload)
                                {
                                    var res = await fmsService.MpUploadAsync(new MpUploadRequest(request.Key)
                                    {
                                        trayCode = pallet.PalletCode,
                                        cellCodes = cells
                                    });
                                    return res.IsSuccess;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                // 组盘工位NG托盘不上传
                                var res = await fmsService.MpUploadAsync(new MpUploadRequest(request.Key)
                                {
                                    trayCode = pallet.PalletCode,
                                    cellCodes = cells
                                });
                                return res.IsSuccess;
                            }


                            //var cellCodes = pallet.Channels.Take(pallet.ChannelNum)
                            //        .Select((s, i) => new Cellcode
                            //        {
                            //            cellCode = s.Code,
                            //            channel = i + 1,
                            //            gradingType = MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.TrayLevelUpdate ?
                            //            (
                            //                codeMapTemp.Find(s.Code).Match(
                            //                    Some: lv => CZPUtils.GetLevelFromMes(lv),
                            //                    None: () => isCodeRuleNG1(s.Code) ? BatteryLevel.Fake : s.BatteryLevel
                            //                )
                            //            ).ToString() :
                            //            (
                            //                tempLv.ToUpper() == "NG" ? BatteryLevel.NG :
                            //                isCodeRuleNG1(s.Code) ? BatteryLevel.Fake : s.BatteryLevel
                            //            ).ToString(),
                            //            cellType = s.GetCellType()
                            //        })
                            //        .Filter(s => !string.IsNullOrWhiteSpace(s.cellCode))
                            //        .ToArray();
                            //    if (cellCodes.Length != pallet.ChannelNum)
                            //    {
                            //    logger.LogError( $"条码数量与通道数量不匹配！通道数量：{pallet.ChannelNum}，有效条码数量：{cellCodes.Length}");
                            //    throw new InvalidOperationException($"电芯条码数量与通道数量不匹配！通道数量：{pallet.ChannelNum}，有效条码数量：{cellCodes.Length}");

                            //}

                            //    var res = await fmsService.MpUploadAsync(new MpUploadRequest(request.Key)
                            //    {
                            //        trayCode = pallet.PalletCode,
                            //        cellCodes = cellCodes
                            //    });
                            //    return res.IsSuccess;
                        }


                        else if (pallet.Type == PalletType.拆盘)
                        {
                            var res = await fmsService.MpunbindWholeTrayAsync(new MpUnbindWholeTrayRequest(request.Key)
                            { trayCode = pallet.PalletCode }
                           );
                            return res.IsSuccess;

                        }
                        return false;
                    }

                    var data = await GetData();
                    if (!data)
                    {
                        await mediator.Send(new WcsDataWarningBoxCommand("FMS通讯异常"));
                        return false;
                    }

                }

                // 作业结束
                if (MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.GrabActionRemoveEnable)
                {
                    var data = await mediator.Send(new EquipStatusReportCommand(request.Key, pallet.InsideNum, EquipStatus.E));
                    //PalletStatus = request.isSpecial ? PalletStatus.不满 : PalletStatus.满盘,

                    if (!data)
                    {
                        await mediator.Send(new WcsDataWarningBoxCommand("ECS上报状态失败"));
                        return false;
                    }

                }


            }
            //else
            //{
            //    throw new Exception("托盘类型错误");
            //}

            // TODO: 结束要用106命令


        SaveLog:

            if (!request.Start && pallet.Type == PalletType.组盘)
            {
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
            }

            return true;

        }
    }
}
