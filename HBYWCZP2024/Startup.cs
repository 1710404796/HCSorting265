using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HBYWCZP2024
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            


            app.UseEndpoints(routes =>
            {
                routes.MapPost("/getStationInfo", (int sid) =>
                {
                    var station = CZPUtils.GetXYLStationByInsideNum(MainStorage.Saves.SelectStageName, sid);
                    if (station == null)
                    {
                        throw new Exception("station不存在");
                    }
                    return new Pallet(station.PalletCode,
                        [.. station.Channels.Take(station.ChannelNum)
                                        .Select((s, i) => new Channel(i + 1, new BatteryWithLevel(s.Code, s.BatteryLevel)))]
                                            );
                });
                routes.MapControllers();
            });
        }
    }

    public class HomeController
    {
        [HttpGet("/")]
        public string Get() => "Hello World";
    }
}