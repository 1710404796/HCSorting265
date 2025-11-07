using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Service;
using HBYWCZP2024.Utils;
using LanguageExt.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Handlers.ECS
{
    internal class CvRequestCommandHandler(IHBECSServiceFactory hBECSServiceFactory,
        //NewECSService newMesService,
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
                if (MainStorage.Saves.ApiEnableTable.FilterSpecialEnable)
                {
                    var rrn = await newECSService.CvRequestAsync(new CvRequest(request.Key)
                    {
                        TrayCode = pallet.PalletCode,
                        TrayType = ((int)pallet.OKNGType).ToString(),
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
                TrayType = ((int)pallet.OKNGType).ToString(),
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
