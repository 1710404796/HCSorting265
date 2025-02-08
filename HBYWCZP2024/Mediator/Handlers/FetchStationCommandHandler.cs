using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.Queries;
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
    public class FetchStationCommandHandler(ILogger<FetchStationCommandHandler> logger) : IRequestHandler<FetchStationCommand, bool>
    {
        public async Task<bool> Handle(FetchStationCommand request, CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();

            var station = CZPUtils.GetXYLStationByOutsideNum(MainStorage.Saves.SelectStageName, request.stationNum);
            
            if (station == null)
            {
                throw new Exception($"托盘{request.stationNum} 不存在");
            }
            logger.LogInformation("同步{num}工位信息", request.stationNum);
            var response = await httpClient.GetFromJsonAsync<GrabApiInfo[]>($"http://{station.ServerIp}/getStationInfo?sid={request.stationNum}");
            if (response != null)
            {
                response.Iter(s =>
                {
                    station.Channels[s.ChannelId - 1].Code = s.Code;
                    station.Channels[s.ChannelId - 1].BatteryLevel = s.Level;
                });
                return true;

            }
            throw new Exception($"获取{request.stationNum}工位信息失败");
        }
    }
}
