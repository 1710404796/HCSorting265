using HCSortingCZP.Mediator.Commands;
using LinxUniverse.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Logging;
using System.Security.Claims;
using System.Windows;

//using ClaimTypes = System.IdentityModel.Claims.ClaimTypes;

namespace HCSortingCZP.Mediator.Handlers
{
    public class LoginCommandHandler(LinxAuthenticationStateProvider authenticationStateProvider, IMediator mediator, LinxUniverse.Auth.UserManager<LinxUser> userManager) : IRequestHandler<LoginCommand, bool>
    {
        public async Task<bool> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            if (request.userId == "123")
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

            var users = await userManager.GetAllUserAsync();
            var user = users.FirstOrDefault(u => u.Password == request.userId);
            if (user == null)
            {
                MessageBox.Show("没有找到指定ID的用户");
                return false;
            }

            //通过GetUserRolesAsync获取用户角色
            var roles = await userManager.GetUserRolesAsync(user);
            string role = roles.First();

            //创建一个ClaimsPrincipal（主要声明）和一个ClaimsIdentity(请求身份)("login")
            //接着又添加了两个ClaimsIdemtity。
            var aa = new ClaimsPrincipal(new ClaimsIdentity("login"));
            aa.AddIdentity(new ClaimsIdentity([new Claim(ClaimTypes.Name, user.UserName)]));
            aa.AddIdentity(new ClaimsIdentity([new Claim("role", role)]));

            //更新认证状态
            (authenticationStateProvider as StandLinxAuthenticationStateProvider)
                .SetAuthenticationState(Task.FromResult(new AuthenticationState(aa)));



            return true;
        }
    }
}