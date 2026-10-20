using LinxUniverse.Mediator;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace HCSortingCZP.Mediator.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        private readonly IMediator _mediator;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }


        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var attr = request.GetType().GetCustomAttribute<NotLogAttribute>();
            bool logEnable = attr == null;
            try
            {

                if (logEnable)
                {
                    _logger.LogInformation($"正在执行: {typeof(TRequest).Name}");
                    _logger.LogDebug("请求数据: {data}", request);
                }

                var response = await next();
                if (logEnable)
                {
                    _logger.LogDebug("回复数据: {data}", response);

                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "处理请求发生错误: \"{request}\"，原因: {message}",
                    typeof(TRequest).Name,
                    GetChineseErrorMessage(ex));
                throw;
            }

        }
        private static string GetChineseErrorMessage(Exception ex)
        {
            var message = ex.Message;
            return message switch
            {
                var m when m.Contains("BadTooManySessions", StringComparison.OrdinalIgnoreCase)
                    => "PLC OPC UA 会话数已达上限(BadTooManySessions)",
                var m when m.Contains("BadSecureChannelIdInvalid", StringComparison.OrdinalIgnoreCase)
                    => "PLC OPC UA 安全通道ID无效(BadSecureChannelIdInvalid)",
                var m when m.Contains("BadSessionIdInvalid", StringComparison.OrdinalIgnoreCase)
                    => "PLC OPC UA 会话ID无效(BadSessionIdInvalid)",
                var m when m.Contains("BadConnectionClosed", StringComparison.OrdinalIgnoreCase)
                    => "PLC OPC UA TCP连接已关闭(BadConnectionClosed)",
                _ => message
            };
        }
    }
}
