using BCR.Common.Fs;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Utils;
using Keyence.NET;
using MediatR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Keyence.NET.KeyenceBCR;


namespace HBYWCZP2024.Mediator.Handlers
{
    public class ScanPalletCodeCommandHandler : IRequestHandler<ScanPalletCodeCommand, string>
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> ScanLocks = new();
        private static readonly TimeSpan ReaderReconnectWait = TimeSpan.FromSeconds(2);

        public async Task<string> Handle(ScanPalletCodeCommand request, CancellationToken cancellationToken)
        {
            // Todo: 实现读取指定托盘码

            if (MainStorage.Saves.KeysSave[request.name].TestMode)
            {
                return $"{request.PalletIdx} TestCode";
            }
            else
            {
                var scanLock = ScanLocks.GetOrAdd($"{request.name}:{request.PalletIdx}", _ => new SemaphoreSlim(1, 1));
                await scanLock.WaitAsync(cancellationToken);

                try
                {
                    var pallet = CZPUtils.GetPalletByIndex(request.name, request.PalletIdx);

                    if (pallet == null)
                    {
                        throw new Exception($"托盘{request.PalletIdx} 不存在");
                    }
                    var f = async (KeyenceBCR.Reader reader) =>
                    {
                        var ff = await KeyenceBCR.asTask(KeyenceBCR.getOneResultAndClearCacheRepeatAsync(TimeSpan.FromSeconds(3), 1, reader));
                        var cc = fs(ff);
                        return cc;
                    };
                    var code = await pallet.Reader.ToEither("未找到扫码器").BindAsync(  f
                        );
                    if (code.IsLeft)
                    {
                        var oldReader = pallet.Reader;
                        pallet.Reader = None;
                        oldReader.IfSome(reader => KeyenceBCR.close(reader));

                        string? reconnectError = null;
                        pallet.Reader = fs(KeyenceBCR.create(ConnectAddress.NewIPAddress(pallet.ReaderIpAddr))).Match(
                                Right: reader => Some(reader),
                                Left: e =>
                                {
                                    reconnectError = e;
                                    return None;
                                }
                            );

                        if (pallet.Reader.IsSome)
                        {
                            await Task.Delay(ReaderReconnectWait, cancellationToken);
                            code = await pallet.Reader.ToEither("扫码器重连后未就绪").BindAsync(f);
                        }
                        else if (!string.IsNullOrWhiteSpace(reconnectError))
                        {
                            throw new Exception($"扫码器重连失败: {reconnectError}");
                        }
                    }
                    var data = code.Match(
                        Right: c => c,
                        Left: e => throw new Exception(e)
                        );
                    var scannedCode = data.CodeDatas.FirstOrDefault()?.Code;
                    if (string.IsNullOrWhiteSpace(scannedCode))
                    {
                        var message = $"未获取到托盘{request.PalletIdx}条码";
                        throw new Exception(message);
                    }

                    return scannedCode;

                    // todo: 待测试实现

                    //await mediator.Send(new IntelligentTriggerCommand(pallet.SGGuid, 10, 100));

                    //var res = await mediator.Send(new GetResultBCRQuery(pallet.SGGuid));

                    //if (res != null && res.Count() > 0)
                    //{
                    //    return res.First().Code;
                    //}

                    throw new Exception($"未获取到托盘{request.PalletIdx}条码");
                }
                finally
                {
                    scanLock.Release();
                }

            }
        }
    }
}
