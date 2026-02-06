using MediatR;
using System;

namespace HBYWCZP2024.Mediator.Commands
{
    public record SaveToSqlCommand(
        string TableName,
        string EquipNum,
        string ContainerCode,
        string Identification,
        string Message,
        DateTime? Time = null
    ) : IRequest<bool>;
}
