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