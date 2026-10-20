using LinxUniverse.Mediator;
using MediatR;

namespace HCSortingCZP.Mediator.Commands
{
    [NotLog]
    public record LoginCommand(string userId) : IRequest<bool>;
    [NotLog]
    public record LogOutCommand() : IRequest;
}