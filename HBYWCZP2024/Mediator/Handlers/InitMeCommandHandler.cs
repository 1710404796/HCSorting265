
using BCR.Common.Fs;
using Cognex.NET;
using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.PLC;
using HBYWCZP2024.PLC.Tasks;
using HBYWCZP2024.Service;
using LanguageExt;
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
                foreach (var pallet in stage.Value.Stage.Pallets)
                {
                    if (pallet.IsEnable)
                    {
                        pallet.Reader = fs(CognexBCR.create(ConnectAddress.NewIPAddress(pallet.ReaderIpAddr))).Match(
                            Right: reader => Some(reader),
                            Left: s =>
                            {
                                mediator.Send(new WarningBoxCommand(s));
                                return None;
                            }
                        );
                        Console.WriteLine(pallet.Reader);
                    }

                }


                Console.WriteLine($"IP: {stage.Value.PLCPSetting.IP}");
                var ecs = hBECSService.Create(stage.Key);
                var se = pLCTaskServiceFactory.CreateService(stage.Key, new HBYWPLCContext() { Name = stage.Key });
                await se.Init(new LinxUniverse.OPCUA.Meditor.Command.CreateOpcuaClientCommand("LINX", "Linx0304", stage.Value.PLCPSetting.IP), cancellationToken);

                se.RegisterTask<CheckLadaiTask>();
                se.RegisterTask<ApplyBatteryInfoTask>();
                se.RegisterTask<ChaiPanCompletedTask>();
                se.RegisterTask<InstallationCompletedTask>();
                se .RegisterTask<SinglePlacementCompletedTask>();
                se.RegisterTask<AbnormalDischargeTask>();
          

                for (int i = 0; i < stage.Value.Stage.Pallets.Count(s => s.IsEnable); i++)
                {
                    se.RegisterTask(new PalletArrivedTask(i));
                    se.RegisterTask(new GrabActionTask(i));
                    se.RegisterTask(new PalletOutTask(i));

                }
                if (stage.Value.OpcTable == null || stage.Value.OpcTable.Count == 0 || stage.Value.ResetTable)
                {
                    Console.WriteLine("尝试获取table");

                    logger.LogWarning("OpcTable未设定");
                    if (se.Context.Plc.Connected)
                    {
                        stage.Value.OpcTable = se.Context.Plc.UaClient.GetOpcMap("ns=4;i=1");
                        stage.Value.ResetTable = false;
                    }
                    else
                    {
                        Console.WriteLine("pLCTaskService未连接");
                        logger.LogWarning("pLCTaskService未连接");

                    }
                    //MainStorage.Saves.OpcTable = [];

                }
                MainStorage.SaveManager.Save();


               

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