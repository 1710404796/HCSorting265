using MediatR;
using System.Windows;

namespace HCSortingCZP.Mediator.Commands
{
    public record WarningBoxCommand(string Message,
        string Caption = "报警提示", MessageBoxButton Button = MessageBoxButton.OK, bool SaveLog = true) : IRequest<MessageBoxResult>;

    public record InformationBoxCommand(string Message,
        string Caption = "提示", MessageBoxButton Button = MessageBoxButton.OK) : IRequest<MessageBoxResult>;

    public record PlcDataWarningBoxCommand(string Message,
    string Caption = "报警提示", MessageBoxButton Button = MessageBoxButton.OK, bool SaveLog = true) : IRequest<MessageBoxResult>;

    public record WcsDataWarningBoxCommand(string Message,
        string Caption = "报警提示", MessageBoxButton Button = MessageBoxButton.OK, bool SaveLog = true, int OutStationNum = 0) : IRequest<MessageBoxResult>;
}