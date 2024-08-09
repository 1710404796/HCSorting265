
using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Service;
using LinxUniverse.Auth;
using LinxUniverse.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HBYWCZP2024.Mediator.Handlers
{
    public class InitMeCommandHandler(IMediator mediator, RoleManager<LinxRole, LinxUser> role, ILogger<InitMeCommandHandler> logger, HBECSService hBECSService)
        : IRequestHandler<InitMeCommand>
    {
        public async Task Handle(InitMeCommand request, CancellationToken cancellationToken)
        {
            FilenameHelper.CreateDir("Data2D");



            foreach (var item in Enum.GetValues<PowerEnum>())
            {
                if (!MainStorage.Saves.PowerTable.ContainsKey(item))
                {
                    MainStorage.Saves.PowerTable[item] = [];
                }

                foreach (var item1 in Enum.GetValues<RoleEnum>() )
                {
                    if (!MainStorage.Saves.PowerTable[item].ContainsKey(item1))
                    {
                        MainStorage.Saves.PowerTable[item][item1] = false;
                    }

                }
            }
            foreach (var item in Enum.GetNames<RoleEnum>())
            {

                await role.CreateAsync(new LinxRole { RoleName = item });
            }

            hBECSService.On("E110", data =>
            {

            });

        }
    }
}