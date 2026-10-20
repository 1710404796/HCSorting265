using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Mediator.ECS
{
    /// <summary>
    /// 上传设备错误通知
    /// </summary>
    /// <param name="Key"></param>
    /// <param name="ErrorCode"></param>
    public record UploadDeivceErrorNotice(string Key, ushort ErrorCode) : IRequest;
}
