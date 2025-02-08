using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Utils;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers
{
    internal class PushDataToStationCommandHandler(ILogger<FetchStationCommandHandler> logger) : IRequestHandler<PushDataToStationCommand, bool>
    {
        public async Task<bool> Handle(PushDataToStationCommand request, CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();

            var station = CZPUtils.GetXYLStationByOutsideNum(MainStorage.Saves.SelectStageName, request.stationNum);

            if (station == null)
            {
                throw new Exception($"托盘{request.stationNum} 不存在");
            }
            logger.LogInformation("同步{num}工位信息", request.stationNum);
            //var response = await httpClient.PostAsJsonAsync($"http://{station.ServerIp}/getStationInfo?sid={request.stationNum}", request.Channels.Map(s => new GrabApiInfo(s.ChannelId, s.BattleWithLevel.));

            return true;
        }
    }
}
