using HBYWCZP2024.Models;
using HBYWCZP2024.Models.ECS;
using MediatR;
namespace HBYWCZP2024.Mediator.ECS;

public record GrabActionCommand(string Key, int StationNum, bool Start) : IRequest<bool>;

/// <summary>
/// 4.4
/// </summary>
/// <param name="data"></param>
public record GrabCommand(string Key, S105 data) : IRequest<bool>;

/// <summary>
/// 4.3
/// </summary>
/// <param name="InStationNum"></param>
public record ApplyBatteryInfoCommand(string Key, int StationNum) : IRequest<BatteryLevel[]?>;

/// <summary>
/// 4.2
/// </summary>
/// <param name="MaterialInfos"></param>
public record CheckMaterialOnlineCommand(string Key, MaterialInfo[] MaterialInfos) : IRequest<(int channel, BatteryLevel level)[]?>;

public record MaterialInfo(string MaterialCode, int Channel);


public record AskFakeCellPalletCommand(int StationNum) : IRequest;
public record MoveFakeCellPalletCommand(int StationNum) : IRequest<bool>;