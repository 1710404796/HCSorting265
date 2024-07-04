
using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Mediator.Commands;
using LinxUniverse.Auth;
using LinxUniverse.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HBYWCZP2024.Mediator.Handlers
{
    public class InitMeCommandHandler(IMediator mediator, RoleManager<LinxRole, LinxUser> role, ILogger<InitMeCommandHandler> logger)
        : IRequestHandler<InitMeCommand>
    {
        public async Task Handle(InitMeCommand request, CancellationToken cancellationToken)
        {
            FilenameHelper.CreateDir("Data2D");
            foreach (var item in Enum.GetNames<RoleEnum>())
            {

                await role.CreateAsync(new LinxRole { RoleName = item });
            }
            //messageboxmanager
            //MainStorage.Cst[0] =
            //    await mediator.Send(new CreateCSTLightCommand(Com: SerialPortType.COM1));
            //if (string.IsNullOrEmpty(MainStorage.Saves.ExtCamKey))
            //{
            //    logger.LogWarning("广角镜头序列码未设定");
            //    return;
            //}
            //MainStorage.CamId = await mediator.Send(new CreateCameraCommand(new CameraDeciveInfo
            //{
            //    CameraType = CameraType.HK2d,
            //    Key = MainStorage.Saves.ExtCamKey,

            //}));

            //if (MainStorage.CamId != null)
            //{
            //    var cam = await mediator.Send(new GetCameraQuery(MainStorage.CamId)) as HKGigeTriggerCamera;
            //    cam.InitCamera();
            //    // todo: 曝光是否要标准化
            //    cam.SetExposureTime(MainStorage.Saves.ExtCamExposure);
            //    MainStorage.Cam = cam; 
            //}
            //else
            //{
            //    await mediator.Send(new WarningBoxCommand("广角相机初始化失败", SaveLog: false));
            //}
            //MainStorage.Cst[1] =
            //await mediator.Send(new CreateCSTLightCommand(Com: SerialPortType.COM2));

        }
    }
}