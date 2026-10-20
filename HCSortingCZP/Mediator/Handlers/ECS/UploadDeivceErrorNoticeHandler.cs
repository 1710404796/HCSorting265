using HCSortingCZP.Mediator.ECS;
using HCSortingCZP.Models.ECS;
using HCSortingCZP.Service;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Mediator.Handlers.ECS
{
    internal class UploadDeivceErrorNoticeHandler(IHBECSServiceFactory hBECSServiceFactory) : IRequest<UploadDeivceErrorNotice>
    {
        public Task Handle(UploadDeivceErrorNotice notification, CancellationToken cancellationToken)
        {
            // 占位
            //hBECSService.Send("S110", "12345678");
            HBECSService hBECSService = hBECSServiceFactory.Create(notification.Key);

            hBECSService.Send(new S110() { ErrorCode = "1234" });
            return Task.CompletedTask;
        }
    }
}
