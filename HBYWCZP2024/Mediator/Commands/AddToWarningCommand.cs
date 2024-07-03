using MediatR;

namespace HBYWCZP2024.Mediator.Commands
{
    public record AddToWarningCommand(string Message) : IRequest;
}