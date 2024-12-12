using Cognex.NET;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Utils;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HBYWCZP2024.Mediator.Handlers
{
    public class ScanPalletCodeCommandHandler(IMediator mediator) : IRequestHandler<ScanPalletCodeCommand, string>
    {
        public async Task<string> Handle(ScanPalletCodeCommand request, CancellationToken cancellationToken)
        {
            // Todo: 实现读取指定托盘码

            if (MainStorage.Saves.KeysSave[request.name].TestMode)
            {
                return $"{request.PalletIdx} TestCode";
            }
            else
            {
                var pallet = CZPUtils.GetPalletByIndex(request.name, request.PalletIdx);

                if (pallet == null)
                {
                    throw new Exception($"托盘{request.PalletIdx}");
                }

                var code = pallet.Reader.Match(
                    Some: reader => fs(CognexBCR.getResultsAndClearCacheRepeat(TimeSpan.FromSeconds(3), 5, reader)),
                    None: () => Left("未找到扫码器")
                    );
                
                var data = code.Match(
                    Right: c => c,
                    Left: e => throw new Exception(e)
                    );
                return data.First().Code;

                // todo: 待测试实现

                //await mediator.Send(new IntelligentTriggerCommand(pallet.SGGuid, 10, 100));

                //var res = await mediator.Send(new GetResultBCRQuery(pallet.SGGuid));

                //if (res != null && res.Count() > 0)
                //{
                //    return res.First().Code;
                //}

                throw new Exception($"未获取到托盘{request.PalletIdx}条码");

            }
        }
    }
}
