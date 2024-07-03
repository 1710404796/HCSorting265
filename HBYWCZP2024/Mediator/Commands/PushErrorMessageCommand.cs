using MediatR;

namespace HBYWCZP2024.Mediator.Commands
{
    public record PushErrorMessageCommand(string Message) : IRequest;
}