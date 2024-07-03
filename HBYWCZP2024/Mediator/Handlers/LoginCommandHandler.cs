using HBYWCZP2024.Mediator.Commands;
using LinxUniverse.Auth;
using MediatR;
using Microsoft.IdentityModel.Logging;
using System.Security.Claims;

//using ClaimTypes = System.IdentityModel.Claims.ClaimTypes;

namespace HBYWCZP2024.Mediator.Handlers
{
    public class LoginCommandHandler(LinxAuthenticationStateProvider authenticationStateProvider, IMediator mediator) : IRequestHandler<LoginCommand, bool>
    {
        public async Task<bool> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            if (request.userId == "linx0304")
            {
                // 新定义一个provider admin免认证
                var admin = new ClaimsPrincipal(new ClaimsIdentity("login"));
                admin.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")]));
                admin.AddIdentity(new ClaimsIdentity([new Claim("role", "admin")]));

                // Todo: 想一想这里
                (authenticationStateProvider as StandLinxAuthenticationStateProvider)
                    .SetAuthenticationState(Task.FromResult(new AuthenticationState(admin)));

                return true;
            }

            //var res = await mediator.Send(new GetIdCardCheckCommand(request.userId));
            //if (res == null) return false;
            ////var aa = new ClaimsPrincipal(new LinxIdentity(){IsAuthenticated = true});
            //var aa = new ClaimsPrincipal(new ClaimsIdentity("login"));
            //aa.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Name, res.Staff)]));
            //aa.AddIdentity(new ClaimsIdentity([new Claim("cardId", request.userId)]));
            //aa.AddIdentity(new ClaimsIdentity([new Claim("level", res.Level)]));
            //aa.AddIdentity(new ClaimsIdentity([new Claim("role", LoginHelper.GetPowerName(res.Level))]));


            //// 看看信息加哪里
            //(authenticationStateProvider as StandLinxAuthenticationStateProvider)
            //    .SetAuthenticationState(Task.FromResult(new AuthenticationState(aa)));

            return true;
        }
    }
}