using HCSortingCZP.Mediator.Commands;
using HCSortingCZP.Mediator.ECS;
using HCSortingCZP.Models.CZPallet;
using HCSortingCZP.Models.ECS;
using HCSortingCZP.Service;
using HCSortingCZP.Utils;
using HCSortingCZP.ViewModel;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace HCSortingCZP.Mediator.Handlers.ECS
{
    internal class PalletOutCommandHandler(IHBECSServiceFactory hBECSServiceFactory,
        //NewECSService newMesService,
        IMediator mediator) : IRequestHandler<PalletOutCommand, bool>
    {

        [JsonIgnore]
        static Lazy<ILogger> _lazyLogger = new Lazy<ILogger>(App.GetService<ILogger<MainViewModel>>);
        [JsonIgnore]
        static ILogger _logger => _lazyLogger.Value;
        public async Task<bool> Handle(PalletOutCommand request, CancellationToken cancellationToken)
        {
            HBECSService hBECSService = hBECSServiceFactory.Create(request.Key);
            var newECSService = hBECSService.NewECSService;

            var pallet = CZPUtils.GetPalletByInsideNum(request.Key, request.InsideStationNum);
            _logger.LogInformation($"工位：{request.Key}，站台：{request.InsideStationNum}，托盘：{pallet.PalletCode} 准备申请叫料");

            if (pallet.CallMaterialEnable)
            {
                var callOk = await mediator.Send(new YZCallMaterialCommand(request.Key, request.InsideStationNum), cancellationToken);
                if (!callOk)
                {
                    return false;
                }
            }

            //托盘离开接口使能
            if (!MainStorage.Saves.KeysSave[request.Key].ApiEnableTable.PalletOutEnable)
            {
                return true;
            }

            //按内部编号获取托盘
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

            // if (pallet.OKNGType == TrayType.NG托盘)
            // {
            //     var callOk = await mediator.Send(new YZCallMaterialCommand(request.Key, request.InsideStationNum), cancellationToken);
            //     if (!callOk)
            //     {
            //         return false;
            //     }
            // }

            _logger.LogInformation($"工位：{request.Key}，站台：{request.InsideStationNum}，托盘：{pallet.PalletCode} 离开上报ECS成功");
            ApplyBatteryInfoCommandHandler.ClearCache(pallet.PalletCode);
            

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

            int maxRetries = 10;
            int retryIntervalMinutes = 3;
            Exception lastException = null;

            for (int retryCount = 1; retryCount <= maxRetries; retryCount++)
            {
                try
                {
                    var rrn1 = await newECSService.CvRequestAsync(new CvRequest(request.Key)
                    {
                        TrayCode = pallet.PalletCode,
                        TrayType = pallet.OKNGType.GetTrayTypeStr(),
                        Location = pallet.OutsideStationNum.ToString(),
                    });

                    var success = rrn1.Match(
                    Succ: r => true,
                        Fail: ex =>
                        {
                            _logger.LogError(ex, $"工位：{request.Key}，站台：{request.InsideStationNum}，托盘：{pallet.PalletCode} 请求失败，第“{retryCount}”次尝试");
                            lastException = ex;
                            return false;
                        }
                    );
                    if (success)
                    {
                        _logger.LogInformation($"工位：{request.Key}，站台：{request.InsideStationNum}，托盘：{pallet.PalletCode} 请求完成，共尝试“{retryCount}”次");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"工位：{request.Key}，站台：{request.InsideStationNum}，托盘：{pallet.PalletCode} 请求发生异常，第“{retryCount}”次尝试");
                    lastException = ex;
                }

                if (retryCount < maxRetries)
                {
                    _logger.LogError($"工位：{request.Key}，站台：{request.InsideStationNum}，托盘：{pallet.PalletCode}，{retryIntervalMinutes}分钟后重试尝试申请");
                    await Task.Delay(TimeSpan.FromMinutes(retryIntervalMinutes));
                }
            }
            _logger.LogError($"工位：{request.Key}，站台：{request.InsideStationNum}，托盘：{pallet.PalletCode}，{maxRetries}次重试均失败");
            return false;


            //var rrn1 = await newECSService.CvRequestAsync(new CvRequest(request.Key)
            //{
            //    TrayCode = pallet.PalletCode,
            //    //TrayType = ((int)pallet.OKNGType).ToString(),
            //    TrayType = pallet.OKNGType.GetTrayTypeStr(),
            //    //TrayType = pallet.OKNGType.GetOUTTrayTypeStr(),
            //    Location = pallet.OutsideStationNum.ToString()
            //});
            //return rrn1
            //    .Match(
            //    Succ: r => true,
            //    Fail: ex =>
            //    {
            //        mediator.Send(new WcsDataWarningBoxCommand($"托盘{pallet.PalletCode}，{request.InsideStationNum}离开上报ECS失败: {ex.Message}")).Wait();
            //        return false;
            //    });




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
