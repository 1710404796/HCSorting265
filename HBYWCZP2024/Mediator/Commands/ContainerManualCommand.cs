using HBYWCZP2024.Mediator.Queries;
using MediatR;

namespace HBYWCZP2024.Mediator.Commands;

public record ContainerManualCommand(string Key, string ContainerCode) : IRequest<Arr<Channel>>;
