using HCSortingCZP.Mediator.Queries;
using MediatR;

namespace HCSortingCZP.Mediator.Commands;

public record ContainerManualCommand(string Key, string ContainerCode) : IRequest<Arr<Channel>>;
