using HCSortingCZP;
using HCSortingCZP.Attritubes;
using HCSortingCZP.Mediator.Queries;
using Humanizer;
using LinxUniverse.Auth;
using MediatR;
using System.Windows;


namespace HCSortingCZP.Mediator.Handlers
{
    public class CheckPowerQueryHandler(LinxAuthenticationStateProvider authenticationStateProvider) : IRequestHandler<CheckPowerQuery, bool>
    {
        public async Task<bool> Handle(CheckPowerQuery request, CancellationToken cancellationToken)
        {
            string aa = (await authenticationStateProvider.GetAuthenticationStateAsync()).User.GetUserRole();

            if (aa == "admin") return true;

            //RoleEnum roleEnum = (RoleEnum)Enum.Parse(typeof(RoleEnum), aa, true);
            RoleEnum roleEnum = aa.DehumanizeTo<RoleEnum>();

            //if (Enum.TryParse<RoleEnum>(aa, true, out roleEnum))
            {
                if (roleEnum == RoleEnum.SuperAdmin || (MainStorage.Saves.PowerTable.ContainsKey(request.PowerEnum) &&
                               MainStorage.Saves.PowerTable[request.PowerEnum][roleEnum]))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}