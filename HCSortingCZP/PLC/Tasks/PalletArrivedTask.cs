using HCSortingCZP.Mediator.Commands;
using HCSortingCZP.Mediator.ECS;
using HCSortingCZP.Models.CZPallet;
using HCSortingCZP.Utils;
using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HCSortingCZP.PLC.Tasks
{
    /// <summary>
    /// 托盘到位任务 任务号1
    /// </summary>
    [OPCDelectInfo(10, 11)]
    public class PalletArrivedTask : HBBaseTask
    {
        public const string ManualForcePalletCodeKey = "PalletArrivedTask.ManualForcePalletCode";

        public PalletArrivedTask(int subIdx) : base(1)
        {
            TaskName = $"托盘：{subIdx + 1} 托盘到位";
            //TaskName = $"{subIdx + 1} {Properties.Resources.TrayPositioningTask}";
            SubIdx = subIdx;
        }

        public override async Task<bool> DoSth()
        {
            var pallet = CZPUtils.GetPalletByIndex(Context.Name, SubIdx);
            if (pallet == null || !pallet.IsEnable)
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.TrayPositioningTask}: {Properties.Resources.Tray}{SubIdx + 1}{Properties.Resources.DoesNotExist}"));
                //Context.Logger.LogError("");
                return false;
            }
            Context.Logger.LogInformation("准备获取托盘：{idx}条码", SubIdx);
            var palletCode = await GetPalletCodeAsync(pallet);
            if (string.IsNullOrWhiteSpace(palletCode))
            {
                return false;
            }

            pallet.ClearStage();
            pallet.PalletCode = palletCode;
            RuntimeStateStore.Save();

            var arrived = await Context.Mediator.Send(new PalletArrivedCommand(Context.Name, pallet.InsideNum));
            if (arrived)
            {
                RuntimeStateStore.Save();
            }

            return arrived;

            //return await base.DoSth();


        }

        private async Task<string?> GetPalletCodeAsync(Pallet pallet)
        {
            while (true)
            {
                string palletCode;
                try
                {
                    palletCode = (await Context.Mediator.Send(new ScanPalletCodeCommand(Context.Name, SubIdx)))?.Trim() ?? string.Empty;
                }
                catch (Exception ex)
                {
                    pallet.PalletCode = "ERROR";
                    Context.Logger.LogError(ex, "托盘{idx}扫码失败", SubIdx + 1);
                    var retry = await ShowScanFailureBoxAsync(pallet, $"错误信息：{ex.Message}");
                    if (retry)
                    {
                        continue;
                    }

                    return await WaitForManualForceCodeAsync(pallet);
                }

                if (string.IsNullOrWhiteSpace(palletCode))
                {
                    pallet.PalletCode = "ERROR";
                    var retry = await ShowScanFailureBoxAsync(pallet, Properties.Resources.TrayBarcodeNotScanned);
                    if (retry)
                    {
                        continue;
                    }

                    return await WaitForManualForceCodeAsync(pallet);
                }

                if (!pallet.CheckCodeRegular(palletCode))
                {
                    pallet.PalletCode = "ERROR";
                    var retry = await ShowScanFailureBoxAsync(pallet, Properties.Resources.UpperMachineErrorOrInvalidTrayType);
                    if (retry)
                    {
                        continue;
                    }

                    return await WaitForManualForceCodeAsync(pallet);
                }

                return palletCode;
            }
        }

        private async Task<bool> ShowScanFailureBoxAsync(Pallet pallet, string reason)
        {
            var res = await Context.Mediator.Send(new WarningBoxCommand(
                $"{Properties.Resources.Tray}{SubIdx + 1} {Properties.Resources.Workstation}{pallet.OutsideStationNum} 3秒内未识别到托盘码。\n{reason}\n\n确定：重新扫码\n取消：关闭提示，改为在界面输入托盘码后点击“强制写入条码”。",
                Button: MessageBoxButton.OKCancel));

            return res == MessageBoxResult.OK;
        }

        private async Task<string?> WaitForManualForceCodeAsync(Pallet pallet)
        {
            Context.Logger.LogWarning("托盘{idx}扫码失败，等待人工强制写入条码", SubIdx + 1);
            pallet.TempData.Remove(ManualForcePalletCodeKey);

            while (true)
            {
                if (pallet.TempData.TryGetValue(ManualForcePalletCodeKey, out var value)
                    && value is string manualCode
                    && !string.IsNullOrWhiteSpace(manualCode))
                {
                    pallet.TempData.Remove(ManualForcePalletCodeKey);
                    return manualCode.Trim();
                }

                if (TaskCodeDelectDB.Actions(SubIdx) != TaskCode)
                {
                    Context.Logger.LogWarning("托盘{idx}等待人工强制写入时 PLC 任务码已变化，结束等待", SubIdx + 1);
                    return null;
                }

                await Task.Delay(200);
            }
        }

        private string TrayWritePrefix => Context.ExDb ? "WCS_TO_PLC_Tray2" : "WCS_TO_PLC_Tray";


        protected override Task WriteDataToPlc()
        {
            var pallet = CZPUtils.GetPalletByIndex(Context.Name, SubIdx);

            byte[] data = new byte[32];
            // 可能要把条码写回去
            //LinxUniverse.PLC.Common.Utils.SetStringCharArray(data, 0, pallet.PalletCode);
            //Encoding.UTF8.GetBytes(pallet.PalletCode).CopyTo(data, 0);
            //Context.Plc.WriteBytes(S7.Net.DataType.DataBlock, 11, 16 + 34 * SubIdx, data);
            Context.Plc.WriteTagNode($"{TrayWritePrefix}.Tray.[{SubIdx}]", pallet!.PalletCode, Context.Name);

            //Context.Plc.WriteNode()
            return base.WriteDataToPlc();
        }

        protected override Task<bool> CleanData()
        {
            byte[] data = new byte[34];

            Context.Plc.WriteTagNode($"{TrayWritePrefix}.Tray.[{SubIdx}]", string.Empty, Context.Name);
            //Context.Plc.WriteBytes(S7.Net.DataType.DataBlock, 11, 16 + 34 * SubIdx, data);
            return Task.FromResult(true);

        }
    }


    /// <summary>
    /// 拆组盘开始任务 任务号2
    /// </summary>
    [OPCDelectInfo(10, 11)]
    public class GrabActionTask : HBBaseTask
    {
        public GrabActionTask(int subIdx) : base(2)
        {
            TaskName = $"托盘：{subIdx + 1} 拆组盘开始";
            SubIdx = subIdx;
        }

        public override async Task<bool> DoSth()
        {
            var pallet = CZPUtils.GetPalletByIndex(Context.Name, SubIdx);
            if (pallet == null || !pallet.IsEnable)
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.TrayDisassemblyTaskStarted}: {Properties.Resources.Tray}{SubIdx + 1}{Properties.Resources.NotFound}"));
                return false;
            }
            Context.Logger.LogInformation("托盘：{idx} 上报拆组盘接口", SubIdx + 1);
            //new GrabActionRequest
            //{
            //    SelfActionType = pallet.Type switch { PalletType.组盘 => ActionType.insertStart, PalletType.拆盘 => ActionType.removeStart, _ => ActionType.removeStart },
            //    ContainerCode = pallet.PalletCode,
            //    StationCode = pallet.OutsideStationNum.ToString(),
            //     //MaterialQuantity = pallet.GetActivecnt(),
            //};

            var res = await Context.Mediator.Send(new GrabActionCommand(Context.Name, pallet.InsideNum, true, false));
            return res;//&& await Context.Mediator.Send(new PalletArrivedCommand(Context.Name, pallet.InsideNum));

            //return res;

            return await base.DoSth();
        }
    }

    /// <summary>
    /// 特殊托盘离开任务 任务号13
    /// </summary>
    [OPCDelectInfo(10, 11)]
    public class PalletOutTask2 : HBBaseTask
    {
        public PalletOutTask2(int subIdx) : base(13)
        {
            TaskName = $"托盘：{subIdx + 1} {Properties.Resources.TrayDepartureTask}";
            SubIdx = subIdx;
        }

        public override async Task<bool> DoSth()
        {
            var pallet = CZPUtils.GetPalletByIndex(Context.Name, SubIdx);
            if (pallet == null || !pallet.IsEnable)
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.TrayDepartureTask}: {Properties.Resources.Tray}{SubIdx + 1}{Properties.Resources.NotFound}"));
                return false;
            }

            if (!await Context.Mediator.Send(new PalletOutCommand(Context.Name, pallet.InsideNum, true)))
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.TrayDepartureTask}: {Properties.Resources.Tray}：{SubIdx + 1}，{pallet.InsideNum}上传失败"));

            }
            else 
            {
                pallet.ClearStage();
                RuntimeStateStore.Save();
            }
            
            return true;
        }
    }

    /// <summary>
    /// 托盘离开任务 任务号3
    /// </summary>
    [OPCDelectInfo(10, 11)]
    public class PalletOutTask : HBBaseTask
    {
        public PalletOutTask(int subIdx) : base(3)
        {
            TaskName = $"托盘：{subIdx + 1} {Properties.Resources.TrayDepartureTask}";
            SubIdx = subIdx;
        }

        public override async Task<bool> DoSth()
        {
            var pallet = CZPUtils.GetPalletByIndex(Context.Name, SubIdx);
            if (pallet == null || !pallet.IsEnable)
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.TrayDepartureTask}: {Properties.Resources.Tray}：{SubIdx + 1}{Properties.Resources.NotFound}"));
                return false;
            }

            if (!await Context.Mediator.Send(new PalletOutCommand(Context.Name, pallet.InsideNum)))
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.TrayDepartureTask}: {Properties.Resources.Tray}： {SubIdx + 1}，{pallet.InsideNum}上传失败"));
            }
            else 
            {
                pallet.ClearStage();
                RuntimeStateStore.Save();
            }
            
            return true;
        }
    }

    /// <summary>
    /// 物流线路径请求  8
    /// </summary>
    [OPCDelectInfo(10, 11)]
    public class CvRequestTask : HBBaseTask
    {
        public CvRequestTask(int subIdx) : base(8)
        {
            TaskName = $"托盘：{subIdx + 1} 调用CvRequest接口";
            SubIdx = subIdx;
        }

        public override async Task<bool> DoSth()
        {
            var pallet = CZPUtils.GetPalletByIndex(Context.Name, SubIdx + 1);
            if (pallet == null || !pallet.IsEnable)
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.Tray}：{SubIdx + 1}{Properties.Resources.NotFound}"));
                return false;
            }

            if (!await Context.Mediator.Send(new CvRequestCommand(Context.Name, pallet.InsideNum)))
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.TrayDepartureTask}: {Properties.Resources.Tray}： {SubIdx + 1}，{pallet.InsideNum}上传失败"));
                
            }
            pallet.ClearStage();
            RuntimeStateStore.Save();
            return true;
        }
    }


    /// <summary>
    /// 拆组结束
    /// </summary>
    [OPCDelectInfo(10, 11)]
    public class GrabActionExitTask : HBBaseTask
    {
        public GrabActionExitTask(int subIdx) : base(4)
        {
            TaskName = $"托盘：{subIdx + 1} 拆组盘结束";
            SubIdx = subIdx;
        }

        public override async Task<bool> DoSth()
        {
            var pallet = CZPUtils.GetPalletByIndex(Context.Name, SubIdx);
            if (pallet == null || !pallet.IsEnable)
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.TrayDisassemblyTaskStarted}: {Properties.Resources.Tray}{SubIdx + 1}{Properties.Resources.NotFound}"));
                return false;
            }
            Context.Logger.LogInformation("托盘：{idx} 上报拆组盘接口", SubIdx + 1);
            var res = await Context.Mediator.Send(new GrabActionCommand(Context.Name, pallet.InsideNum, false, false));
            return res;//&& await Context.Mediator.Send(new PalletArrivedCommand(Context.Name, pallet.InsideNum));

            //return res;

            return await base.DoSth();
        }
    }

}
