using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.ViewModel;
using MediatR;


namespace HBYWCZP2024.Mediator.Handlers
{
    public class PushErrorMessageCommandHandler(MainViewModel viewModel) : IRequestHandler<PushErrorMessageCommand>
    {
        public Task Handle(PushErrorMessageCommand request,
            CancellationToken cancellationToken)
        {
            viewModel.ErrorText = request.Message;
            //if (string.IsNullOrEmpty(viewModel.ErrorText))
            //{
            //    viewModel.Status = true;
            //}
            //else
            //{
            //    viewModel.Status = false;
            //}
            return Task.CompletedTask;
        }
    }
}