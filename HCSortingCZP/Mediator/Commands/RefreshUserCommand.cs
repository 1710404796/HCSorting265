using LinxUniverse.Mediator;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Mediator.Commands
{
    [NotLog]
    public record RefreshUserCommand : IRequest;
}
