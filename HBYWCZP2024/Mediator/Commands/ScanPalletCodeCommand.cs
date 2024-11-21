using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Commands
{
    public record ScanPalletCodeCommand(string name, int PalletIdx) : IRequest<string>;
}
