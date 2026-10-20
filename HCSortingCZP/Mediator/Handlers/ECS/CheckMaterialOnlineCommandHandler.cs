using HCSortingCZP.Mediator.ECS;
using HCSortingCZP.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Mediator.Handlers.ECS
{
    [Obsolete]
    internal class CheckMaterialOnlineCommandHandler : IRequestHandler<CheckMaterialOnlineCommand, (int channel, BatteryLevel level)[]?>
    {
        public async Task<(int channel, BatteryLevel level)[]?> Handle(CheckMaterialOnlineCommand request, CancellationToken cancellationToken)
        {

            return request.MaterialInfos.Select(x =>
            (x.Channel,
                x.MaterialCode.ToLower() switch
                {
                    "noread" => BatteryLevel.NG,
                    _ => BatteryLevel.OK
                }
            )).ToArray();
        }
    }
}
