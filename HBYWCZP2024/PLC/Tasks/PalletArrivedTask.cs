using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Utils;
using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.PLC.Tasks
{
    /// <summary>
    /// 托盘到位任务 任务号1
    /// </summary>
    [OPCDelectInfo(10, 11)]
    public sealed class PalletArrivedTask : HBBaseTask
    {
        public PalletArrivedTask(int subIdx) : base(1)
        {
            TaskName = $"{subIdx + 1} 托盘到位";
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
            Context.Logger.LogInformation("准备获取托盘{idx}条码", SubIdx);
            pallet.PalletCode = string.Empty;
            pallet.PalletCode = (await Context.Mediator.Send(new ScanPalletCodeCommand(Context.Name, SubIdx)))?.Trim() ?? string.Empty;
            // 条码校验
            while (true)
            {
                if (string.IsNullOrEmpty(pallet.PalletCode))
                {
                    var res = await Context.Mediator.Send(new WarningBoxCommand($"{Properties.Resources.Tray}{SubIdx + 1} {Properties.Resources.Workstation}{pallet.OutsideStationNum} {Properties.Resources.TrayBarcodeNotScanned}\n{Properties.Resources.RescanOrManualEntry}", Button: System.Windows.MessageBoxButton.OKCancel));
                    if (res == System.Windows.MessageBoxResult.Cancel)
                    {
                        return false;
                    }
                }
                else if (!pallet.CheckCodeRegular())
                {

                    var res = await Context.Mediator.Send(new WarningBoxCommand($"{Properties.Resources.Tray}{SubIdx + 1} {Properties.Resources.Workstation}{pallet.OutsideStationNum} {Properties.Resources.UpperMachineErrorOrInvalidTrayType}\n{Properties.Resources.RescanOrManualEntry}", Button: System.Windows.MessageBoxButton.OKCancel));
                    if (res == System.Windows.MessageBoxResult.Cancel)
                    {
                        return false;
                    }
                }
                else
                {
                    break;
                }

            }

            return  await Context.Mediator.Send(new PalletArrivedCommand(Context.Name, pallet.InsideNum));

            //return await base.DoSth();


        }


        protected override Task WriteDataToPlc()
        {
            var pallet = CZPUtils.GetPalletByIndex(Context.Name, SubIdx);

            byte[] data = new byte[32];
            // 可能要把条码写回去
            //LinxUniverse.PLC.Common.Utils.SetStringCharArray(data, 0, pallet.PalletCode);
            //Encoding.UTF8.GetBytes(pallet.PalletCode).CopyTo(data, 0);
            //Context.Plc.WriteBytes(S7.Net.DataType.DataBlock, 11, 16 + 34 * SubIdx, data);
            Context.Plc.WriteTagNode($"WCS_TO_PLC_Tray.Tray.[{SubIdx}]", pallet!.PalletCode, Context.Name);

            //Context.Plc.WriteNode()
            return base.WriteDataToPlc();
        }

        protected override Task<bool> CleanData()
        {
            byte[] data = new byte[34];

            Context.Plc.WriteTagNode($"WCS_TO_PLC_Tray.Tray.[{SubIdx}]", string.Empty, Context.Name);
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
            TaskName = $"{subIdx + 1} 拆组盘开始";
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
            Context.Logger.LogInformation("托盘{idx} 上报拆组盘接口", SubIdx);
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
    /// 托盘离开任务 任务号3
    /// </summary>

    [OPCDelectInfo(10, 11)]
    public class PalletOutTask : HBBaseTask
    {
        public PalletOutTask(int subIdx) : base(3)
        {
            TaskName = $"{subIdx} {Properties.Resources.TrayDepartureTask}";
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

            pallet.ClearStage();
            await Context.Mediator.Send(new PalletOutCommand(Context.Name, pallet.InsideNum));
            return true;
        }
    }
}
