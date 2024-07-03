using MediatR;

namespace HBYWCZP2024.Mediator.Commands
{
    public record BusyCommand(bool Value, int PValue = 0, string message = "") : IRequest;
}