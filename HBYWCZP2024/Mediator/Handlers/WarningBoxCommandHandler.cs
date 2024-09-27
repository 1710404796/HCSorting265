using HBYWCZP2024.Mediator.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace HBYWCZP2024.Mediator.Handlers
{
    public class WarningBoxCommandHandler(IMediator mediator) : IRequestHandler<WarningBoxCommand, MessageBoxResult>
    {
        public async Task<MessageBoxResult> Handle(WarningBoxCommand request, CancellationToken cancellationToken)
        {
            if (request.SaveLog)
                await mediator.Send(new AddToWarningCommand(request.Message));
            return MessageBox.Show(request.Message, request.Caption,
                request.Button,
                MessageBoxImage.Warning,
                MessageBoxResult.None,
                MessageBoxOptions.ServiceNotification);
        }
    }

    public class InformationBoxCommandHandler(IMediator mediator) : IRequestHandler<InformationBoxCommand, MessageBoxResult>
    {
        public async Task<MessageBoxResult> Handle(InformationBoxCommand request, CancellationToken cancellationToken)
        {

            return MessageBox.Show(request.Message, request.Caption,
                request.Button,
                MessageBoxImage.Information,
                MessageBoxResult.None,
                MessageBoxOptions.ServiceNotification);
        }
    }


    public class PlcDataWarningBoxCommandHandler(IMediator mediator, ILogger<PlcDataWarningBoxCommandHandler> logger) : IRequestHandler<PlcDataWarningBoxCommand, MessageBoxResult>
    {
        public Task<MessageBoxResult> Handle(PlcDataWarningBoxCommand request, CancellationToken cancellationToken)
        {
            return mediator.Send(new WarningBoxCommand($"PLC数据校验: \n{request.Message}", request.Caption, request.Button, request.SaveLog));

        }
    }

    public class WcsDataWarningBoxCommandHandler(IMediator mediator, ILogger<WcsDataWarningBoxCommandHandler> logger) : IRequestHandler<WcsDataWarningBoxCommand, MessageBoxResult>
    {
        public Task<MessageBoxResult> Handle(WcsDataWarningBoxCommand request, CancellationToken cancellationToken)
        {
            return mediator.Send(new WarningBoxCommand($"WCS返回数据校验: \n{request.Message}", request.Caption, request.Button, request.SaveLog));
        }
    }

}