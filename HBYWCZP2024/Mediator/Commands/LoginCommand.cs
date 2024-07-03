using LinxUniverse.Mediator;
using MediatR;

namespace HBYWCZP2024.Mediator.Commands
{
    [NotLog]
    public record LoginCommand(string userId) : IRequest<bool>;
    [NotLog]
    public record LogOutCommand() : IRequest;
}