using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Models.CZPallet;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Service;
using HBYWCZP2024.Utils;
using LanguageExt.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LanguageExt;
using static LanguageExt.Prelude;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    internal class PalletOutCommandHandler(IHBECSServiceFactory hBECSServiceFactory, 
        //NewECSService newMesService,
        IMediator mediator) : IRequestHandler<PalletOutCommand, bool>
    {
        public async Task<bool> Handle(PalletOutCommand request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);
            var newECSService = hBECSService.NewECSService;
            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.PalletOutEnable)
            {
                return true;
            }

            var pallet= CZPUtils.GetPalletByInsideNum(request.Key, request.InsideStationNum);
            if (pallet == null)
            {
                throw new Exception("托盘不存在");
            }

            Result<EquipOutResponse> response = await newECSService.EquipOutAsync(new EquipOutRequest(request.Key)
            {
                TrayCode = pallet.PalletCode,
                TrayType = ((int)pallet.OKNGType).ToString(),
                Location = pallet.OutsideStationNum.ToString()
            });
            var rrn = await newECSService.CvRequestAsync(new CvRequest(request.Key)
            {
                TrayCode = pallet.PalletCode,
                TrayType = ((int)pallet.OKNGType).ToString(),
                Location = pallet.OutsideStationNum.ToString()
            });

            var v = response
                .Match(
                Succ: r => true,
                Fail: ex =>
                {
                    mediator.Send(new WcsDataWarningBoxCommand($"托盘{pallet.PalletCode}离开上报ECS失败: {ex.Message}")).Wait();
                    return false;
                }
                );

            if (!v) return false;

            return rrn
                .Match(
                Succ: r => true,
                Fail: ex =>
                {
                    mediator.Send(new WcsDataWarningBoxCommand($"托盘{pallet.PalletCode}离开上报ECS失败: {ex.Message}")).Wait();
                    return false;
                }
                );



            //TrayType Get(Pallet pallet)
            //{
            //    if (pallet.Type == PalletType.筛选)
            //    {
            //        if (pallet.Channels.Take(pallet.ChannelNum).All(s => string.IsNullOrWhiteSpace(s.Code)))
            //        {
            //            return TrayType.空筛选类型;
            //        }
            //    }
            //    return pallet.OKNGType;
            //}
            //var type = Get(pallet);
            //var data = await hBECSService.SendAsync<S107, E107>(new () { TrayNumber = pallet.PalletCode, TrayPosition = pallet.PalletId + 1, TrayType = type });
            //return data != null;// && data.ErrorCode == "0";
        }
    }
}
