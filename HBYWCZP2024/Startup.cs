using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Models;
using HBYWCZP2024.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HBYWCZP2024
{
    public record GrabApiInfo(int ChannelId, string Code, BatteryLevel Level);
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
                routes.MapGet("/getStationInfo", (int sid) =>
                {
                    var station = CZPUtils.GetXYLStationByInsideNum(MainStorage.Saves.SelectStageName, sid);
                    if (station == null)
                    {
                        throw new Exception("station不存在");
                    }
                    return station.Channels.Take(station.ChannelNum)
                                        .Select((s, i) => new GrabApiInfo(i + 1, s.Code, s.BatteryLevel));
                    //return new Pallet(station.PalletCode,
                    //    [.. station.Channels.Take(station.ChannelNum)
                    //                    .Select((s, i) => new Channel(i + 1, new BatteryWithLevel(s.Code, s.BatteryLevel)))]
                                            //);
                });

                routes.MapPost("/updateStationInfo", ([FromQuery] int sid, [FromBody] GrabApiInfo[] channels, [FromQuery] bool isPut) =>
                {
                    var station = CZPUtils.GetXYLStationByInsideNum(MainStorage.Saves.SelectStageName, sid);
                    if (station == null)
                    {
                        throw new Exception("station不存在");
                    }

                    if (isPut)
                    {

                        channels.Iter(
                             async (c) =>
                             await station.BindBattery(c.ChannelId - 1, c.Code, true, c.Level)
                             );
                    }
                    else
                    {
                        channels.Iter((c) => station.UnbindBattery(c.ChannelId - 1));
                    }
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