using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Service;
using HBYWCZP2024.Utils;
using LanguageExt.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    internal class CvRequestCommandHandler(IHBECSServiceFactory hBECSServiceFactory,
        //NewECSService newMesService,
        ILogger<CvRequestCommandHandler> logger,
        IMediator mediator) : IRequestHandler<CvRequestCommand, bool>
    {
        public async Task<bool> Handle(CvRequestCommand request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);
            var newECSService = hBECSService.NewECSService;
            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.PalletOutEnable)
            {
                return true;
            }

            var pallet = CZPUtils.GetPalletByInsideNum(request.Key, request.InsideStationNum);
            if (pallet == null)
            {
                throw new Exception($"{request.Key},{request.InsideStationNum}托盘不存在");
            }


            if (request.isSpecial)
            {
                //特殊工位筛选
                if (MainStorage.Saves.ApiEnableTable.FilterSpecialEnable)
                {
                    var Dest = MainStorage.Saves.KeysSave[request.Key].FilterSpecialNum;

                    if (MainStorage.Saves.ApiEnableTable.GRegionCvRequestEnable)
                    {
                        var batteryLevels = pallet.Channels
                            .Take(pallet.ChannelNum)
                            .Select(s => s.BatteryLevel)
                            .Filter(s => s != Models.BatteryLevel.EMPTY && s != Models.BatteryLevel.None);
                        var next = 
                            MainStorage.Saves.StationLevelMap
                            // 智能分选 .Filter(s => string.IsNullOrWhiteSpace( CZPUtils.GetAllXYLStationByInsideNum(s.Station)?.PalletCode))
                            .FirstOrDefault(
                            s => batteryLevels
                                    .Contains((s.Level - 10)
                                    .GetLevelFromDigit()
                            ));
                        if (next != null)
                        {
                            logger.LogInformation("存在下一级电芯站台, {station}", next.Station);
                            Dest = next.Station.ToString();
                        }
                        else
                        {
                            // 没有下一级了 撤退
                            logger.LogInformation("没有下一级电芯了");
                            Dest = "";
                        }
                    }

                    var rrn = await newECSService.CvRequestAsync(new CvRequest(request.Key)
                    {
                        TrayCode = pallet.PalletCode,
                        //TrayType = ((int)pallet.OKNGType).ToString(),
                        TrayType = pallet.OKNGType.GetTrayTypeStr(),
                        Location = pallet.OutsideStationNum.ToString(),
                        Dest = MainStorage.Saves.KeysSave[request.Key].FilterSpecialNum
                    });
                    return rrn
                        .Match(
                        Succ: r => true,
                        Fail: ex =>
                        {
                            mediator.Send(new WcsDataWarningBoxCommand($"托盘{pallet.PalletCode}离开上报ECS失败: {ex.Message}")).Wait();
                            return false;
                        }
                        );
                }
            }

            var rrn1 = await newECSService.CvRequestAsync(new CvRequest(request.Key)
            {
                TrayCode = pallet.PalletCode,
                //TrayType = ((int)pallet.OKNGType).ToString(),
                TrayType = pallet.OKNGType.GetTrayTypeStr(),
                Location = pallet.OutsideStationNum.ToString()
            });
            return rrn1
                .Match(
                Succ: r => true,
                Fail: ex =>
                {
                    mediator.Send(new WcsDataWarningBoxCommand($"托盘{pallet.PalletCode}离开上报ECS失败: {ex.Message}")).Wait();
                    return false;
                });

        }
    }
}
