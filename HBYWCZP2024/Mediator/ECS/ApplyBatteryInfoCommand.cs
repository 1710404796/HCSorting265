using HBYWCZP2024.Models;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Service;
using MediatR;
namespace HBYWCZP2024.Mediator.ECS;

/// <summary>
/// 拆组完成
/// </summary>
/// <param name="Key">机械手工位号</param>
/// <param name="StationNum">站台号</param>
/// <param name="Start">开始</param>
/// <param name="isSpecial">是否特殊</param>
public record GrabActionCommand(string Key, int StationNum, bool Start, bool isSpecial) : IRequest<bool>;

/// <summary>
/// 4.4 托盘到达
/// </summary>
/// <param name="data"></param>
public record PalletArrivedCommand(string Key, int InsideStationNum) : IRequest<bool>;
//public record PalletOutCommand(string Key, int InsideStationNum) : IRequest<bool>;
/// <summary>
/// 托盘退出
/// </summary>
/// <param name="Key"></param>
/// <param name="InsideStationNum">工位号</param>
/// <param name="isSpecial">是否特殊</param>
public record PalletOutCommand(string Key, int InsideStationNum, bool isSpecial = false) : IRequest<bool>;

/// <summary>
/// 物流线路径请求
/// </summary>
/// <param name="Key"></param>
/// <param name="InsideStationNum"></param>
/// <param name="isSpecial"></param>
public record CvRequestCommand(string Key, int InsideStationNum,bool isSpecial = false) : IRequest<bool>;


public record GrabCommand(string Key, GrabInfo GrabInfo) : IRequest<bool>;

public record GrabInfo(XYLStation From, XYLStation To, Arr<BatteryMoveInfo> Infos);
public record BatteryMoveInfo(string Code, int FromChannel, int ToChannel);

/// <summary>
/// 4.3 请求电芯信息
/// </summary>
/// <param name="InStationNum"></param>
public record ApplyBatteryInfoCommand(string Key, int StationNum) : IRequest<BatteryLevel[]?>;

/// <summary>
/// 4.2
/// </summary>
/// <param name="MaterialInfos"></param>
[Obsolete]
public record CheckMaterialOnlineCommand(string Key, MaterialInfo[] MaterialInfos) : IRequest<(int channel, BatteryLevel level)[]?>;

public record MaterialInfo(string MaterialCode, int Channel);


public record AskFakeCellPalletCommand(int StationNum) : IRequest;
public record MoveFakeCellPalletCommand(int StationNum) : IRequest<bool>;
/// <summary>
/// 状态变更即上报
/// </summary>
/// <param name="Key"></param>
/// <param name="InsideStationNum"></param>
/// <param name="EquipStatus"></param>
public record EquipStatusReportCommand(string Key, int InsideStationNum, EquipStatus EquipStatus) : IRequest<bool>;



