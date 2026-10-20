using MediatR;

namespace HCSortingCZP.Mediator.Commands
{
    public record AddToWarningCommand(string Message) : IRequest;
}