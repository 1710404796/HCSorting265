using HBYWCZP2024.Mediator.Commands;
using LinxUniverse.Auth;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers
{
    internal class OperatorLogCommandHandler(ILogger<OperatorLogCommand> logger, LinxAuthenticationStateProvider authenticationStateProvider) : IRequestHandler<OperatorLogCommand>
    {
        public async Task Handle(OperatorLogCommand request, CancellationToken cancellationToken)
        {
            var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;
            if (user == null)
            {
                logger.LogWarning($"Operator: Unknown Log: {request.msg}");
                return;
            }
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                logger.LogInformation($"Operator: {user.GetUserName()}({user.GetUserRole()}) Log: {request.msg}");

            }
            else
            {
                logger.LogWarning($"Operator: Unknown Log: {request.msg}");
            }
        }
    }
}
