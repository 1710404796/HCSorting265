using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Utils;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers
{
    public class FetchStationCommandHandler : IRequestHandler<FetchStationCommand, Pallet>
    {
        public async Task<Pallet> Handle(FetchStationCommand request, CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();

            var station = CZPUtils.GetXYLStationByOutsideNum(request.name, request.stationNum);

            if (station == null)
            {
                throw new Exception($"托盘{request.stationNum} 不存在");
            }

            var response = await httpClient.GetFromJsonAsync<Pallet>($"http://{station.ServerIp}/getStationInfo");

            throw new NotImplementedException();
        }
    }
}
