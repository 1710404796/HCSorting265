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


namespace HBYWCZP2024.Mediator.Behaviors
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
                _logger.LogError("处理请求发生错误: {request}", typeof(TRequest).Name);
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);

            }
            return default;

        }
    }
}
