using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Models;
using HBYWCZP2024.Service;
using HBYWCZP2024.Service.Models;
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

            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.CheckMaterialEnable)
            {
                return request.Channels.Map(c =>
                    c with
                    {
                        BattleWithLevel = c.BattleWithLevel.Map(s => s with { Level = BatteryLevel.OK })
                    }
                ).ToArr();
            }

            var service = factory.Create(request.Key);



            var mRequest = new MaterialOnlineRequest(request.Key)
            {
                batteryTrayBindVOList = [.. request.Channels
                    .Filter(s =>
                    s.BattleWithLevel
                    .Map(s => !s
                        .Code.StartsWith("Noread", StringComparison.CurrentCultureIgnoreCase))
                        .Match(Some: s => s, None: false)
                    )
                 .Map(c =>
                 new Batterytraybindvolist
                 { identification = c.BattleWithLevel.Map(b => b.Code).Match(Some: s => s, None: () => "")
                 })]
            };

            var res = await service.MesService.MaterialOnline(mRequest);
            var codeMap = res.Match(
            Succ: s => s.rows.Map(r =>
            (r.identification ?? "", r.qualityStatus?.ToLower() switch
            {
                "ok" => BatteryLevel.OK,
                "ng" => BatteryLevel.NG,
                _ => BatteryLevel.NG,
            })),
                Fail: f =>
                {

                    mediator.Send(new WcsDataWarningBoxCommand(f.Message)).Wait();
                    throw f;
                }


                ).DistinctBy(s => s.Item1).ToMap();
            var sthMaterialId = res.Map(s => s.rows.Map(s => s.materialId).Distinct()).Match(
                Succ: s => s.FirstOrDefault(), Fail: f =>
                {

                    mediator.Send(new WcsDataWarningBoxCommand(f.Message)).Wait();
                    throw f;
                }


                );
            MainStorage.Saves.KeysSave[request.Key].MaterialCode = sthMaterialId ?? string.Empty;

            var data = request.Channels.Map(c =>
            c with
            {
                BattleWithLevel = c.BattleWithLevel.Map(b => new BatteryWithLevel(b.Code,
            b.Code switch
            {
                var x when x.StartsWith("noread", StringComparison.CurrentCultureIgnoreCase) => BatteryLevel.NG,
                _ => codeMap[b.Code]
            }


             ))
            }

            ).ToArr();

            // 高温静置入站
            var rss = data.Filter(data => data.BattleWithLevel.Match(s => s.Level == BatteryLevel.OK, () => false))
                 .AsParallel()
                .Map( s =>
            {
                 return service.MesService.HighTempOnlineAsync(new HighTempOnlineRequest(request.Key)
                {
                    identification = s.BattleWithLevel.Match(s => s.Code, () => ""),
                }).Result;
            });
            if (!rss.All(s => s.IsSuccess))
            {
                var message = string.Join("\n", rss.Filter(s => !s.IsSuccess).Map(s => s.Match(Succ: su => "", Fail: fa => fa.Message)));
               await mediator.Send(new WcsDataWarningBoxCommand(message));
                throw new Exception(message);
            }
            return data;
        }
    }
}
