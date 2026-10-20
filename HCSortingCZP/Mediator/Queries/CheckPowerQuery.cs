using HCSortingCZP.Attritubes;
using LinxUniverse.Mediator;
using MediatR;

namespace HCSortingCZP.Mediator.Queries
{
    [NotLog]
    //public record CheckPowerQuery(PowerEnum PowerEnum): IRequest<bool>;
    public record CheckPowerQuery(PowerEnum PowerEnum) : IRequest<bool>;
}