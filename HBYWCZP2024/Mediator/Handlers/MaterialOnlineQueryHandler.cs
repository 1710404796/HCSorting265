using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Models;
using HBYWCZP2024.Service;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers
{
    public class MaterialOnlineQueryHandler(IHBECSServiceFactory factory, IMediator mediator) : IRequestHandler<MaterialOnlineQuery, Arr<Channel>>
    {
        public async Task<Arr<Channel>> Handle(MaterialOnlineQuery request, CancellationToken cancellationToken)
        {
            var service = factory.Create(request.Key);

            var mRequest = new MaterialOnlineRequest(request.Key)
            {
                batteryTrayBindVOList = [.. request.Channels
                 .Map(c => 
                 new Batterytraybindvolist 
                 { identification = c.BattleWithLevel.Map(b => b.Code).Match(Some: s => s, None: () => "") 
                 })]
            };

            var res =await service.MesService.MaterialOnline(mRequest);

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
            return request.Channels.Map(c =>
            c with { BattleWithLevel = c.BattleWithLevel.Map(b => new BatteryWithLevel(b.Code,
            codeMap[b.Code])) }
            
            ).ToArr();
        }
    }
}
