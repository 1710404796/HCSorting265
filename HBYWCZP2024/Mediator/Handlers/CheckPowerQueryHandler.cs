using HBYWCZP2024;
using HBYWCZP2024.Mediator.Queries;
using LinxUniverse.Auth;
using MediatR;
using System.Windows;


namespace HBYWCZP2024.Mediator.Handlers
{
    public class CheckPowerQueryHandler(LinxAuthenticationStateProvider authenticationStateProvider) : IRequestHandler<CheckPowerQuery, bool>
    {
        public async Task<bool> Handle(CheckPowerQuery request, CancellationToken cancellationToken)
        {
            //var aa = LoginHelper.GetPowerId((await authenticationStateProvider.GetAuthenticationStateAsync()).User.GetUserRole()) - 1;
            //if (aa == 5 || (MainStorage.Saves.PowerTable.ContainsKey(request.PowerEnum) && 
            //                MainStorage.Saves.PowerTable[request.PowerEnum][aa]))
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            return true;
        }
    }
}