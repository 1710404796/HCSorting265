using HCSortingCZP.Mediator.Commands;
using HCSortingCZP.Mediator.ECS;
using HCSortingCZP.Service;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HCSortingCZP.Mediator.Handlers.ECS
{
    internal class YZScreenUploadCommandHandler(IHBECSServiceFactory hBECSServiceFactory, IMediator mediator)
        : IRequestHandler<YZScreenUploadCommand, bool>
    {
        public async Task<bool> Handle(YZScreenUploadCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.TrayCode))
            {
                await mediator.Send(new WcsDataWarningBoxCommand("托盘条码不能为空"), cancellationToken);
                return false;
            }

            var fmsService = hBECSServiceFactory.Create(request.Key).NewFMSService;
            var res = await fmsService.YZScreenUploadAsync(new YZScreenUploadRequest(request.Key)
            {
                trayCode = request.TrayCode,
                oriConveyorCode = request.OriConveyorCode,
                startTime = request.StartTime,
                endTime = request.EndTime,
                channel = request.Channels
            });

            if (res.IsSuccess)
            {
                return true;
            }

            var err = res.Match(Succ: _ => string.Empty, Fail: f => f.Message);
            await mediator.Send(new WcsDataWarningBoxCommand($"FMS筛选结果上传失败: {err}"), cancellationToken);
            return false;
        }
    }
}
