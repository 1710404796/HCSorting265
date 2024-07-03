using HBYWCZP2024.Mediator.Commands;
using LinxUniverse.Auth;
using MediatR;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers
{
    internal class RefreshUserCommandHandler(LinxAuthenticationStateProvider authenticationStateProvider) : IRequestHandler<RefreshUserCommand>
    {

        public Task Handle(RefreshUserCommand request, CancellationToken cancellationToken)
        {
            (authenticationStateProvider as StandLinxAuthenticationStateProvider<LinxUser>).TestFlag = true;
            return Task.CompletedTask;
        }
    }
}
