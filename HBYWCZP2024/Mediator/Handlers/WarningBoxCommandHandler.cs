using HBYWCZP2024.Mediator.Commands;
using MediatR;
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
}