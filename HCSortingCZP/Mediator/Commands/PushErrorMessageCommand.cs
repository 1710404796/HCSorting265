using MediatR;

namespace HCSortingCZP.Mediator.Commands
{
    public record PushErrorMessageCommand(string Message) : IRequest;
}