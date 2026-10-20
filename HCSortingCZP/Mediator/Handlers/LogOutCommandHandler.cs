using HCSortingCZP.Mediator.Commands;
using LinxUniverse.Auth;
using MediatR;

namespace HCSortingCZP.Mediator.Handlers
{
    public class LogOutCommandHandler(LinxAuthenticationStateProvider authenticationStateProvider) : IRequestHandler<LogOutCommand>
    {
        public Task Handle(LogOutCommand request, CancellationToken cancellationToken)
        {
            (authenticationStateProvider as StandLinxAuthenticationStateProvider).ForceSignOut();
            return Task.CompletedTask;
        }
    }
}