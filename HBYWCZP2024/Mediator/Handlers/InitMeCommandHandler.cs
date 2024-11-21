
using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.PLC;
using HBYWCZP2024.PLC.Tasks;
using HBYWCZP2024.Service;
using LinxUniverse.Auth;
using LinxUniverse.DI;
using LinxUniverse.OPCUA;
using LinxUniverse.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HBYWCZP2024.Mediator.Handlers
{
    public class InitMeCommandHandler(IMediator mediator, RoleManager<LinxRole, LinxUser> role, ILogger<InitMeCommandHandler> logger
        , IHBECSServiceFactory hBECSService, IPLCTaskServiceFactory<HBYWPLCContext> pLCTaskServiceFactory, CacheService cacheService
        //,PLCTaskService<HBYWPLCContext> pLCTaskService
        )
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

            if (MainStorage.Saves.KeysSave.Count == 0)
            {
                MainStorage.Saves.KeysSave.Add("Default", new());

            }
            var ss = MainStorage.Saves.KeysSave.Values.ToList();
            MainStorage.Saves.KeysSave.Clear();
            foreach (var item in ss)
            {
                MainStorage.Saves.KeysSave[item.Name] = item;
            }

            if (!MainStorage.Saves.KeysSave.ContainsKey(MainStorage.Saves.SelectStageName))
            {
                MainStorage.Saves.SelectStageName = MainStorage.Saves.KeysSave.Keys.First();
            }

            foreach (var stage in MainStorage.Saves.KeysSave)
            {
                var ecs = hBECSService.Create(stage.Key);
                var se = pLCTaskServiceFactory.CreateService(stage.Key, new HBYWPLCContext() { Name = stage.Key });
                await se.Init(new LinxUniverse.OPCUA.Meditor.Command.CreateOpcuaClientCommand("LINX", "Linx0304", MainStorage.Saves.KeysSave[stage.Key].OpcIP), cancellationToken);
                // todo: 注册所有任务
                //pLCTaskService.RegisterTask<ApplyBatteryInfoTask>();
                //se.RegisterTask(new ApplyBatteryInfoTask() { HBECSServiceKey = stage.Key });
                //se.RegisterTask<ApplyBatteryInfoTask>();
            }



            //hBECSService.On("E110", data =>
            //{

            //});

        }
    }
}