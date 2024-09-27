using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Service;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    internal class UploadDeivceErrorNoticeHandler(HBECSService hBECSService) : IRequest<UploadDeivceErrorNotice>
    {
        public Task Handle(UploadDeivceErrorNotice notification, CancellationToken cancellationToken)
        {
            // 占位
            //hBECSService.Send("S110", "12345678");
            hBECSService.Send(new S110() { ErrorCode = "1234" });
            return Task.CompletedTask;
        }
    }
}
