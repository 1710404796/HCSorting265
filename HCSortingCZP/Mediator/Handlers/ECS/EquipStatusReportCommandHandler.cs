using HCSortingCZP.Mediator.Commands;
using HCSortingCZP.Mediator.ECS;
using HCSortingCZP.Models.ECS;
using HCSortingCZP.Service;
using HCSortingCZP.Utils;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Mediator.Handlers.ECS
{
    //internal class EquipStatusReportCommandHandler(IHBECSServiceFactory hBECSServiceFactory,
    //    IMediator mediator) : IRequestHandler<EquipStatusReportCommand, bool>
    //{
    //    public async Task<bool> Handle(EquipStatusReportCommand request, CancellationToken cancellationToken)
    //    {
    //        var hbecsService = hBECSServiceFactory.Create(request.Key);
    //        var newECSService = hbecsService.NewECSService;

    //        var pallet = CZPUtils.GetPalletByInsideNum(request.Key, request.InsideStationNum);

    //        if (pallet == null)
    //        {
    //            throw new Exception($"{request.Key},{request.InsideStationNum}托盘不存在");
    //        }

    //        // 如果设备号不存在，则不进行MES上报
    //        if (string.IsNullOrEmpty(MainStorage.Saves.KeysSave[request.Key].EquipNum))
    //        {
    //            return true;
    //        }

    //        pallet.TempData["EquipStatus"] = request.EquipStatus;
    //        //if (pallet.Type == Models.CZPallet.PalletType.筛选)
    //        {
    //            var loc = pallet.OKNGType == TrayType.NG托盘 ? "ng托盘" : "筛选托盘";
    //            if (pallet.Type != Models.CZPallet.PalletType.筛选)
    //            {
    //                loc = (pallet.PalletId + 1).ToString();
    //            }
    //            return (await newECSService.EquipStatusReportAsync([new EquipStatusRequest(request.Key)
    //            {
    //                TrayCode = pallet.PalletCode,
    //                Location = pallet.OutsideStationNum.ToString(),
    //                StatusEnum = request.EquipStatus
    //            }])).Map(r => true).IfFail(ex =>
    //            {
    //                mediator.Send(new WcsDataWarningBoxCommand($"托盘{pallet.PalletCode}状态上报MES失败: {ex.Message}")).Wait();
    //                return false;
    //            });
    //        }
    //        //else
    //        //{
    //        //    return (await newECSService.StageStatusReportAsync([new StageStatusRequest(request.Key)
    //        //    {
    //        //        TrayCode = pallet.PalletCode,
    //        //        Location = pallet.PalletId.ToString(),
    //        //        StatusEnum = request.EquipStatus
    //        //    }])).Map(r => true).IfFail(ex =>
    //        //    {
    //        //        mediator.Send(new WcsDataWarningBoxCommand($"托盘{pallet.PalletCode}状态上报MES失败: {ex.Message}")).Wait();
    //        //        return false;
    //        //    });


    //        //}
           
    //    }
    //}
}
