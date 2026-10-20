using MediatR;

namespace HCSortingCZP.Mediator.Commands
{
    public record BusyCommand(bool Value, int PValue = 0, string message = "") : IRequest;
}