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
            //托盘离开接口使能
            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.PalletOutEnable)
            {
                return true;
            }

            //按内部编号获取托盘
            var pallet = CZPUtils.GetPalletByInsideNum(request.Key, request.InsideStationNum);
            if (pallet == null)
            {
                throw new Exception($"{request.Key},{request.InsideStationNum}托盘不存在");
            }

            if (string.IsNullOrWhiteSpace(pallet.PalletCode))
            {
                await mediator.Send(new WcsDataWarningBoxCommand($"{request.Key},{request.InsideStationNum}出站托盘条码为空"));
                return false;
            }
            Result<EquipOutResponse> response = await newECSService.EquipOutAsync(new EquipOutRequest(request.Key)
            {
                TrayCode = pallet.PalletCode,
                //TrayType = ((int)pallet.OKNGType).ToString(),
                TrayType = pallet.ECSCZTrayType.GetOUTTrayTypeStr(request.isSpecial),
                Location = pallet.OutsideStationNum.ToString()
            });


            var v = response
                .Match(
                Succ: r => true,
                Fail: ex =>
                {
                    mediator.Send(new WcsDataWarningBoxCommand($"托盘{pallet.PalletCode}，{request.InsideStationNum}离开上报ECS失败: {ex.Message}")).Wait();
                    return false;
                }
                );

            if (!v) return false;



            if (request.isSpecial)
            {

                //G区预分选智能工位使能(筛选)
                if (MainStorage.Saves.ApiEnableTable.FilterSpecialEnable)
                {
                    string Dest1;
                    var TrayTypeStr = pallet.OKNGType.GetTrayTypeStr();
                    if (TrayTypeStr == "15")
                    {
                        Dest1 = MainStorage.Saves.KeysSave[request.Key].FilterSpecialNum;
                        mediator.Send(new WcsDataWarningBoxCommand($"托盘{pallet.PalletCode}，{request.InsideStationNum}进入{Dest1}")).Wait();
                    }

                    var rrn = await newECSService.CvRequestAsync(new CvRequest(request.Key)
                    {
                        TrayCode = pallet.PalletCode,
                        //TrayType = ((int)pallet.OKNGType).ToString(),
                        TrayType = pallet.OKNGType.GetTrayTypeStr(),
                        //TrayType = pallet.OKNGType.GetOUTTrayTypeStr(),
                        Location = pallet.OutsideStationNum.ToString(),                     //当前位置
                        Dest = MainStorage.Saves.KeysSave[request.Key].FilterSpecialNum     //目标位置
                    });
                    return rrn
                        .Match(
                        Succ: r => true,
                        Fail: ex =>
                        {
                            mediator.Send(new WcsDataWarningBoxCommand($"托盘{pallet.PalletCode}，{request.InsideStationNum}离开上报ECS失败: {ex.Message}")).Wait();
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
                //TrayType = pallet.OKNGType.GetOUTTrayTypeStr(),
                Location = pallet.OutsideStationNum.ToString()
            });
            return rrn1
                .Match(
                Succ: r => true,
                Fail: ex =>
                {
                    mediator.Send(new WcsDataWarningBoxCommand($"托盘{pallet.PalletCode}，{request.InsideStationNum}离开上报ECS失败: {ex.Message}")).Wait();
                    return false;
               });




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
