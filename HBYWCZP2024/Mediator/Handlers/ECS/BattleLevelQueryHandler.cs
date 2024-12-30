using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Service;
using HBYWCZP2024.Utils;
using LanguageExt.UnitsOfMeasure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    internal class BattleLevelQueryHandler(IHBECSServiceFactory hBECSServiceFactory, IMediator mediator) : IRequestHandler<BattleLevelQuery, Pallet>
    {
        public async Task< Pallet> Handle(BattleLevelQuery request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);
            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.CheckMaterialEnable)
            {
                return request.Pallet with { 
                    Channels = request.Pallet.Channels
                    .Select(x => 
                        x with { 
                            BattleWithLevel = x.BattleWithLevel
                            .Map(s => new BatteryWithLevel(s.Code, Some(Models.BatteryLevel.OK))) 
                    }).ToImmutableArray() };
            }
            CheckPalletMaterialRequest checkMaterialRequest = new CheckPalletMaterialRequest(request.Key)
            {
                ContainerCode = request.Pallet.PalletCode,
                ContainerCellInfos = request.Pallet.Channels.Select(x => new ContainerCellInfo()
                {
                    Location = x.ChannelId.ToString(),
                    Identification = x.BattleWithLevel.Match(
                        Some: battleWithLevel => battleWithLevel.Code,
                        None: string.Empty
                    ),
                    
                }).ToArray()
            };

            var checkMaterialResponse = await hBECSService.MesService.CheckPalletMaterialLevel(checkMaterialRequest);

            var channels = checkMaterialResponse.Match(
                Right: r => {
                    var channels = r.Rows.Select(x =>
                    new Channel(int.Parse(x.Location),
                        Some(
                            new BatteryWithLevel
                            (x.Identification, Some(CZPUtils.GetLevelFromMes(x.Grade)))
                        )
                        ));
                    return channels;
                },
                Left: x => {

                    mediator.Send(new WcsDataWarningBoxCommand(x)).Wait();
                    throw new Exception(x); }


                );
            


            return request.Pallet with { Channels = [.. channels] };
        }
    }
}
