using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers
{
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
