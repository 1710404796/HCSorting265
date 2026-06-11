using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Models;
using HBYWCZP2024.Service;
using HBYWCZP2024.Service.Models;
using HBYWCZP2024.Utils;
using MediatR;
using System;
using System.Linq;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    public class MaterialOnlineQueryHandler(IHBECSServiceFactory factory, IMediator mediator) : IRequestHandler<MaterialOnlineQuery, Arr<Channel>>
    {
        // 拉带电芯校验主流程：
        // 1. 根据工位配置决定走哪条MES校验链路；
        // 2. 先做必须的前置校验，再取各接口返回的等级结果；
        // 3. 最后把多个来源的判定结果合并成每个通道的最终 BatteryLevel。
        public async Task<Arr<Channel>> Handle(MaterialOnlineQuery request, CancellationToken cancellationToken)
        {
            var save = MainStorage.Saves.KeysSave[request.Key];
            var apiEnable = save.ApiEnableTable;
            var service = factory.Create(request.Key);
             
            // 统一识别本地已知的无效条码。
            // 这类条码本身没有继续送MES校验的意义，所以后续所有MES请求都会先过滤掉，
            // 并在最终结果中直接按NG处理，避免无效数据干扰正常判级流程。
            bool isNG(string code) =>
                code.StartsWith("noread", StringComparison.CurrentCultureIgnoreCase)
                || code.StartsWith("error", StringComparison.CurrentCultureIgnoreCase)
                || string.IsNullOrWhiteSpace(code);

            // MES相关请求失败时，统一先弹出报警，再继续抛出异常中断流程。
            // 这样现场能第一时间看到错误信息，同时也保留原始异常栈供日志排查。
            void ThrowWithWarning(Exception ex)
            {
                mediator.Send(new WcsDataWarningBoxCommand(ex.Message)).Wait();
                throw ex;
            }

            // 从当前请求通道中提取“可继续送MES”的有效条码集合。
            // 这里只保留有真实条码且不属于本地无效码的数据，供需要按条码逐个请求的接口使用。
            string[] GetValidCodes() =>
                request.Channels
                    .Map(s => s.BattleWithLevel)
                    .Choose(s => s)
                    .Map(s => s.Code)
                    .Filter(s => !isNG(s))
                    .ToArray();

            // 把当前通道数据转换成MES接口需要的 ContainerCellInfo[] 结构。
            // 这里只保留有效条码，并同步带上当前位置 ChannelId，供按托盘/位置查询的接口使用。
            ContainerCellInfo[] BuildContainerCellInfos() =>
                request.Channels
                    .Select(x => new ContainerCellInfo
                    {
                        Location = x.ChannelId.ToString(),
                        Identification = x.BattleWithLevel.Match(
                            Some: battleWithLevel => battleWithLevel.Code,
                            None: string.Empty)
                    })
                    .Filter(s => !isNG(s.Identification))
                    .ToArray();

            // 根据外部传入的等级解析规则，把最终等级回填到原始通道集合中。
            // 这里不改变通道结构本身，只更新每个通道里 BatteryWithLevel 的 Level。
            Arr<Channel> ApplyLevels(Func<string, BatteryLevel> resolveLevel) =>
                request.Channels.Map(c =>
                    c with
                    {
                        BattleWithLevel = c.BattleWithLevel.Map(b =>
                            new BatteryWithLevel(b.Code, resolveLevel(b.Code)))
                    }).ToArr();

            // 高温静置入站接口是按单条码逐个校验的。
            // 这里不会返回等级结果，只负责做“是否允许入站”的前置验证。
            // 只要任意一条码校验失败，就认为当前批次不能继续往下走，
            // 直接弹窗并中断，避免后续接口继续处理一批前置条件不成立的数据。
            async Task ValidateHCInStationAsync()
            {
                if (!apiEnable.HCInStationEnable)
                {
                    return;
                }

                var results = GetValidCodes()
                    .AsParallel()
                    .Map(code => service.MesService.HCInStationAsync(new HCInStationRequest(request.Key)
                    {
                        identification = code,
                    }).Result);

                if (results.All(s => s.IsSuccess))
                {
                    return;
                }

                var message = string.Join("\n",
                    results
                        .Filter(s => !s.IsSuccess)
                        .Map(s => s.Match(Succ: _ => "", Fail: fa => fa.Message)));
                await mediator.Send(new WcsDataWarningBoxCommand("进站MES报警： " + message));
                throw new Exception(message);
            }

            // 复判接口(FX)返回的是“条码 -> 等级”映射。
            // 这里把接口返回结果整理成 Map，方便后续按条码直接取等级。
            // 如果请求里没有有效条码，则直接返回空Map，避免发送无意义请求。
            async Task<Map<string, BatteryLevel>> GetFxCodeMapAsync(FxCheckRequest request1)
            {
                if (request1.MateriallotCodeList.Length <= 0)
                {
                    return new Map<string, BatteryLevel>();
                }

                var response = await service.MesService.FxCheckAsync(request1);
                return response.Match(
                    Succ: r => r.Rows
                        .Select(x => (x.identification, CZPUtils.GetLevelFromMes(x.rank)))
                        .DistinctBy(s => s.identification)
                        .ToMap(),
                    Fail: f =>
                    {
                        ThrowWithWarning(f);
                        return default;
                    });
            }

            // G区复判接口同样返回“条码 -> 等级”映射，只是请求结构按托盘/位置组织。
            // 这里统一转换成 Map，后面回填等级时就不需要关心具体接口差异。
            async Task<Map<string, BatteryLevel>> GetGRegionCodeMapAsync(CheckPalletMaterialRequest request1)
            {
                if (request1.ContainerCellInfos.Length <= 0)
                {
                    return new Map<string, BatteryLevel>();
                }

                var response = await service.MesService.CheckPalletMaterialLevel(request1);
                return response.Match(
                    Succ: r => r.Rows
                        .Select(x => (x.Identification, CZPUtils.GetLevelFromMes(x.Grade)))
                        .DistinctBy(s => s.Identification)
                        .ToMap(),
                    Fail: f =>
                    {
                        ThrowWithWarning(f);
                        return default;
                    });
            }

            // 前置筛选接口：只负责找出需要直接标记为 Mark 的电芯。
            // 命中的条码后续不再进入组盘接口，避免被后续返回结果覆盖掉已确认的Mark状态。
            async Task<string[]> GetMarkBatterysAsync(CheckPalletMaterialRequest request1)
            {
                if (!apiEnable.HHFilterQueryEnable || request1.ContainerCellInfos.Length <= 0)
                {
                    return [];
                }

                var response = await service.MesService.CheckPalletMaterialLevel(request1);
                return response.Match(
                    Succ: s => s.Rows
                        .Filter(x => CZPUtils.GetLevelFromMes(x.Grade) == BatteryLevel.Mark)
                        .Map(x => x.Identification)
                        .ToArray(),
                    Fail: f =>
                    {
                        ThrowWithWarning(f);
                        return [];
                    });
            }

            // 组盘接口会返回当前电芯在MES侧的组盘判定结果。
            // 其中 qualityStatus=NG 或 pickOutFlag=Y 的数据，业务上都属于“不再参与正常组盘流转”的结果，
            // 所以后续不会把它们放进 codeMap 里参与等级映射，只保留仍可继续流转的电芯结果。
            // 这样做的目的，是把“可继续用的结果”和“已明确需要剔除的结果”先分开，
            // 后面在回填等级时再统一做兜底处理。
            async Task<(Map<string, BatteryLevel> CodeMap, string MaterialId)> GetMaterialOnlineCodeMapAsync(MaterialOnlineRequest request1)
            {
                if (request1.batteryTrayBindVOList.Length <= 0)
                {
                    return (new Map<string, BatteryLevel>(), "");
                }

                var response = await service.MesService.MaterialOnline(request1);
                var validRows = response.Match(
                    Succ: s => s.rows
                        .Filter(r => !string.Equals(r.qualityStatus, "NG", StringComparison.OrdinalIgnoreCase))
                        .Filter(r => !string.Equals(r.pickOutFlag, "Y", StringComparison.OrdinalIgnoreCase)),
                    Fail: f =>
                    {
                        ThrowWithWarning(f);
                        return [];
                    });

                var codeMap = validRows
                    .Map(r => (r.identification ?? "", CZPUtils.GetLevelFromMes(r.qualityStatus ?? "")))
                    .DistinctBy(s => s.Item1)
                    .ToMap();

                var materialId = response.Match(
                    Succ: s => s.rows.Map(x => x.materialId).Distinct().FirstOrDefault() ?? "",
                    Fail: f =>
                    {
                        ThrowWithWarning(f);
                        return string.Empty;
                    });

                return (codeMap, materialId);
            }

            // 二次补充筛选接口：分别取出需要标记为 Mark 和 NG 的条码集合。
            // 这些结果会在最终回填等级时参与优先级合并，优先于组盘接口的普通等级结果。
            async Task<(string[] Marks, string[] NGs)> GetMarkAndNgBatterysAsync(CheckMaterialRequest request1)
            {
                if (!apiEnable.Mark2Enable || request1.ContainerCellInfos.Length <= 0)
                {
                    return ([], []);
                }

                var response = await service.MesService.CheckPalletLevel(request1);
                var marks = response.Match(
                    Succ: s => s.Rows.Filter(x => x.Grade == "B").Map(x => x.Identification).ToArray(),
                    Fail: f =>
                    {
                        ThrowWithWarning(f);
                        return [];
                    });

                var ngs = response.Match(
                    Succ: s => s.Rows.Filter(x => x.Grade == "NG").Map(x => x.Identification).ToArray(),
                    Fail: f =>
                    {
                        ThrowWithWarning(f);
                        return [];
                    });

                return (marks, ngs);
            }

            // 如果当前工位关闭了电芯校验，则不再请求MES。
            // 仅按本地条码有效性做最基础的 OK/NG 判定，保证流程能继续跑通。
            if (!apiEnable.CheckMaterialEnable)
            {
                return ApplyLevels(code => isNG(code) ? BatteryLevel.NG : BatteryLevel.OK);
            }

            // 先执行高温静置入站前置校验。
            // 这一步没有等级输出，但它是后续继续做判级前的准入条件。
            await ValidateHCInStationAsync();

            // 优先走FX复判接口。
            // 该分支开启时，当前工位的最终等级完全以FX接口返回为准，本地只补无效条码的NG兜底。
            if (apiEnable.FXQueryEnable)
            {
                var fxCheckRequest = new FxCheckRequest(request.Key)
                {
                    MateriallotCodeList = GetValidCodes()
                };
                var fxCodeMap = await GetFxCodeMapAsync(fxCheckRequest);

                return ApplyLevels(code =>
                    isNG(code) ? BatteryLevel.NG : fxCodeMap[code]);
            }

            // 如果开启的是G区复判逻辑，则按G区接口返回的等级做最终回填。
            // 这条链路和FX类似，差别只在于MES请求结构不同。
            if (apiEnable.GRegionFXQueryEnable)
            {
                var checkMaterialRequest = new CheckPalletMaterialRequest(request.Key)
                {
                    ContainerCode = "",
                    ContainerCellInfos = BuildContainerCellInfos()
                };
                var gRegionCodeMap = await GetGRegionCodeMapAsync(checkMaterialRequest);

                return ApplyLevels(code =>
                    isNG(code) ? BatteryLevel.NG : gRegionCodeMap[code]);
            }

            // 默认链路：先做前置Mark筛选，再请求组盘接口，最后叠加补充筛选结果。
            // 这是当前 MaterialOnlineQueryHandler 中最复杂的一条业务分支。
            var materialCheckRequest = new CheckPalletMaterialRequest(request.Key)
            {
                ContainerCode = "",
                ContainerCellInfos = BuildContainerCellInfos()
            };
            var markBatterys = await GetMarkBatterysAsync(materialCheckRequest);

            // 已经在前置接口里识别成Mark的电芯，这里不再送组盘接口。
            // 原因是Mark电芯的业务结论已经确定，如果继续进入组盘接口，
            // 后续返回结果可能把前面已经确认的Mark状态覆盖掉，导致最终判定不一致。
            // 这里同时也会过滤掉本地识别出的无效条码，避免向MES发送无意义请求。
            var materialOnlineRequest = new MaterialOnlineRequest(request.Key)
            {
                batteryTrayBindVOList = [.. request.Channels
                    .Filter(s => s.BattleWithLevel.Match(x => !markBatterys.Contains(x.Code), () => false))
                    .Filter(s => s.BattleWithLevel.Map(x => !isNG(x.Code)).IfNone(false))
                    .Map(c => new Batterytraybindvolist
                    {
                        identification = c.BattleWithLevel.Map(b => b.Code).IfNone(""),
                        locationBat = c.ChannelId.ToString()
                    })]
            };
            var (codeMap, materialId) = await GetMaterialOnlineCodeMapAsync(materialOnlineRequest);

            if (!string.IsNullOrEmpty(materialId))
            {
                // 组盘接口返回的 materialId 会作为后续相关MES接口的物料编码来源缓存下来。
                save.MaterialCode = materialId;
            }

            var checkMaterialRequest1 = new CheckMaterialRequest(request.Key)
            {
                ContainerCode = "",
                ContainerCellInfos = BuildContainerCellInfos()
            };
            var markBatterys1 = await GetMarkAndNgBatterysAsync(checkMaterialRequest1);

            // 最终回填等级时，需要把多个来源的判定结果合并：
            // 1. 前置筛出的Mark电芯优先按Mark处理；
            // 2. 后续补充接口筛出的Mark/NG结果按业务规则覆盖；
            // 3. 本地识别出的无效条码直接按NG；
            // 4. 其余条码再去取 MaterialOnline 的有效结果。
            //
            // 这里特别要做“查不到结果按NG兜底”：
            // MaterialOnline 中被过滤掉的电芯（例如 qualityStatus=NG、pickOutFlag=Y）
            // 不会进入 codeMap，如果仍然强行按条码索引，就会触发“Key doesn't exist in map”异常。
            // 因此这里统一按NG兜底，既符合当前业务含义，也能避免整条任务因为单个条码被MES剔除而失败。
            // switch 最后一项 "_" 表示“前面条件都没命中时走这里”，相当于默认分支。
            return ApplyLevels(code => code switch
            {
                var x when markBatterys.Contains(x) => BatteryLevel.Mark,
                var x when markBatterys1.Marks.Contains(x) => BatteryLevel.Mark,
                var x when markBatterys1.NGs.Contains(x) => BatteryLevel.NG,
                var x when isNG(x) => BatteryLevel.NG,
                _ => codeMap.Find(code).IfNone(BatteryLevel.NG)
            });
        }
    }
}
