using HBYWCZP2024.Attritubes;
using LinxUniverse.Mediator;
using MediatR;

namespace HBYWCZP2024.Mediator.Queries
{
    [NotLog]
    //public record CheckPowerQuery(PowerEnum PowerEnum): IRequest<bool>;
    public record CheckPowerQuery(PowerEnum PowerEnum) : IRequest<bool>;
}