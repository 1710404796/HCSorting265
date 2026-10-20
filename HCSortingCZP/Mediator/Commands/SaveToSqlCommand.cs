using HCSortingCZP.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace HCSortingCZP.Mediator.Commands
{
    public record SaveToSqlCommand(
        string TableName,
        string EquipNum,
        IReadOnlyCollection<CupBatteryModel> CupBatteryModels,
        string Message,
        DateTime? Time = null,
        string? Key = null
    ) : IRequest<bool>;
}
