using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Service;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    internal class BattleLevelQueryHandler(HBECSServiceFactory hBECSServiceFactory, IMediator mediator) : IRequestHandler<BattleLevelQuery, IEnumerable<BattleWithLevel>>
    {
        public Task<IEnumerable<BattleWithLevel>> Handle(BattleLevelQuery request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);

            CheckMaterialRequest checkMaterialRequest = new CheckMaterialRequest(request.Key)
            {
                ContainerCode = request.Pallet.PalletCode,
                ContainerCellInfos = request.Pallet.Channels.Select(x => new ContainerCellInfo()
                {
                    Identification = x.ChannelId.ToString(),
                    Location = x.BattleWithLevel.Match(
                        Some: battleWithLevel => battleWithLevel.Code,
                        None: string.Empty
                    )
                }).ToArray()
            };

            throw new NotImplementedException();
        }
    }
}
