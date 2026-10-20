
using HCSortingCZP.Mediator.ECS;
using HCSortingCZP.Mediator.Queries;
using HCSortingCZP.Models;
using HCSortingCZP.Utils;
using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HCSortingCZP.PLC.Tasks
{

    #region
    //[TaskName("拉带入线校验2")]

    //[OPCDelectInfo(510, 511)]
    //public class CheckLadaiTask3 : HBBaseTask
    //{
    //    private bool[] channelState = [];
    //    private string[] batteryCode = [];
    //    private byte[] batteryStatus = [];

    //    public CheckLadaiTask3() : base(3)
    //    {
    //        //TaskName = "拉带入线校验";
    //        TaskName = Properties.Resources.PullBeltInLineVerification;
    //    }

    //    public override bool GetPLCData()
    //    {
    //        channelState = Context.DB510.ChannelStates;
    //        batteryCode = Context.DB510.BatteryCodes;//.Select(s => Encoding.UTF8.GetString(s.Code).Trim()).ToArray();
    //        return base.GetPLCData();
    //    }

    //    public override async Task<bool> DoSth()
    //    {
    //        //Context.Logger.LogInformation();
    //        int ng1 = 0, ng = 0, ok = 0;
    //        for (int i = 0; i < batteryCode.Length; i++)
    //        {
    //            if (!channelState[i]) continue;
    //            if (batteryCode[i].ToLower() == "noread" || string.IsNullOrWhiteSpace(batteryCode[i]))
    //            {
    //                batteryCode[i] = $"Noread{i}";
    //                ng1++;
    //                // 尝试剪一下
    //            }

    //            if (channelState[i])
    //            {
    //                Context.Logger.LogInformation("通道{idx}有料，料号：{code}", i + 1, batteryCode[i]);
    //            }

    //        }
    //        //var battery = batteryCode.Select((s, i) => new MaterialInfo(s, i)).Where(s=> channelState[s.Channel]).ToArray();
    //        var battery = batteryCode
    //            .Select((s, i) => new Channel(i, Some(new BatteryWithLevel(s, None))))
    //            .Zip(channelState)
    //            .Where(s => s.Item2)
    //            .Select(s => s.Item1)
    //            .ToImmutableArray();
    //        var res = await Context.Mediator.Send(new MaterialOnlineQuery(Context.Name, battery));
    //        if (res == null)
    //        {
    //            return false;
    //        }
    //        batteryStatus = Enumerable.Repeat(0, batteryCode.Length).Select(s => (byte)0).ToArray();

    //        foreach (var item in res)
    //        {
    //            BatteryLevel batteryLevel = item.BattleWithLevel.Bind(m => m.Level).Match(Some: s => s, None: () => BatteryLevel.NG);
    //            //batteryStatus[item.ChannelId] = (byte)batteryLevel;
    //            batteryStatus[item.ChannelId] = CZPUtils.GetPlcBatteryLevelCode(batteryLevel);

    //            if (batteryLevel == BatteryLevel.NG)
    //            {
    //                ng++;
    //            }
    //            else if (batteryLevel == BatteryLevel.OK)
    //            {
    //                ok++;
    //            }
    //        }
    //        // bind一下结果
    //        MainStorage.Saves.OKNGCnt.OKCnt += ok;
    //        MainStorage.Saves.OKNGCnt.NG2Cnt += ng - ng1;
    //        MainStorage.Saves.OKNGCnt.NG1Cnt += ng1;


    //        return await base.DoSth();
    //    }

    //    protected override async Task WriteDataToPlc()
    //    {

    //        Utils.Utils.WriteTagNode(Context.Plc, "WCS_TO_PLC_ScanTheCode.WCS.to_batteryStatus", batteryStatus, Context.Name);

    //        await base.WriteDataToPlc();
    //    }

    //    protected override Task<bool> CleanData()
    //    {
    //        channelState = [];
    //        batteryCode = [];
    //        batteryStatus = [];

    //        Utils.Utils.WriteTagNode(Context.Plc, "WCS_TO_PLC_ScanTheCode.WCS.to_batteryStatus", new byte[16], Context.Name);

    //        return base.CleanData();
    //    }
    //}

    //[OPCDelectInfo(510, 511)]
    //public class CheckLadaiTask2 : HBBaseTask
    //{
    //    private bool[] channelState = [];
    //    private string[] batteryCode = [];
    //    private byte[] batteryStatus = [];

    //    public CheckLadaiTask2() : base(2)
    //    {
    //        //TaskName = "拉带入线校验";
    //        TaskName = Properties.Resources.PullBeltInLineVerification;
    //    }

    //    public override bool GetPLCData()
    //    {
    //        channelState = Context.DB510.ChannelStates;
    //        batteryCode = Context.DB510.BatteryCodes;//.Select(s => Encoding.UTF8.GetString(s.Code).Trim()).ToArray();
    //        return base.GetPLCData();
    //    }

    //    public override async Task<bool> DoSth()
    //    {
    //        //Context.Logger.LogInformation();
    //        int ng1 = 0, ng = 0, ok = 0;
    //        for (int i = 0; i < batteryCode.Length; i++)
    //        {
    //            if (!channelState[i]) continue;
    //            if (batteryCode[i].ToLower() == "noread" || string.IsNullOrWhiteSpace(batteryCode[i]))
    //            {
    //                batteryCode[i] = $"Noread{i}";
    //                ng1++;
    //                // 尝试剪一下
    //            }

    //            if (channelState[i])
    //            {
    //                Context.Logger.LogInformation("通道{idx}有料，料号：{code}", i + 1, batteryCode[i]);
    //            }

    //        }
    //        //var battery = batteryCode.Select((s, i) => new MaterialInfo(s, i)).Where(s=> channelState[s.Channel]).ToArray();
    //        var battery = batteryCode
    //            .Select((s, i) => new Channel(i, Some(new BatteryWithLevel(s, None))))
    //            .Zip(channelState)
    //            .Where(s => s.Item2)
    //            .Select(s => s.Item1)
    //            .ToImmutableArray();
    //        var res = await Context.Mediator.Send(new MaterialOnlineQuery(Context.Name, battery));
    //        if (res == null)
    //        {
    //            return false;
    //        }
    //        batteryStatus = Enumerable.Repeat(0, batteryCode.Length).Select(s => (byte)0).ToArray();

    //        foreach (var item in res)
    //        {
    //            BatteryLevel batteryLevel = item.BattleWithLevel.Bind(m => m.Level).Match(Some: s => s, None: () => BatteryLevel.NG);
    //            //batteryStatus[item.ChannelId] = (byte)batteryLevel;
    //            batteryStatus[item.ChannelId] = CZPUtils.GetPlcBatteryLevelCode(batteryLevel);

    //            if (batteryLevel == BatteryLevel.NG)
    //            {
    //                ng++;
    //            }
    //            else if (batteryLevel == BatteryLevel.OK)
    //            {
    //                ok++;
    //            }
    //        }
    //        // bind一下结果
    //        MainStorage.Saves.OKNGCnt.OKCnt += ok;
    //        MainStorage.Saves.OKNGCnt.NG2Cnt += ng - ng1;
    //        MainStorage.Saves.OKNGCnt.NG1Cnt += ng1;


    //        return await base.DoSth();
    //    }

    //    protected override async Task WriteDataToPlc()
    //    {

    //        Utils.Utils.WriteTagNode(Context.Plc, "WCS_TO_PLC_ScanTheCode.WCS.to_batteryStatus", batteryStatus, Context.Name);

    //        await base.WriteDataToPlc();
    //    }

    //    protected override Task<bool> CleanData()
    //    {
    //        channelState = [];
    //        batteryCode = [];
    //        batteryStatus = [];

    //        Utils.Utils.WriteTagNode(Context.Plc, "WCS_TO_PLC_ScanTheCode.WCS.to_batteryStatus", new byte[16], Context.Name);

    //        return base.CleanData();
    //    }
    //}
    #endregion

    [TaskName("拉带入线校验")]

    [OPCDelectInfo(510, 511)]
    public class CheckLadaiTask : HBBaseTask
    {
        protected bool channelState = false;
        protected string batteryCode = string.Empty;
        protected byte batteryStatus = 0;
        protected bool _feedbackWritten = false;

        protected virtual string BatteryStatusNodePath => $"WCS_TO_PLC_ScanTheCode.WCS.to_batteryStatus.[{SubIdx}]";

        public CheckLadaiTask(int subIdx) : base(1)
        {
            //TaskName = "拉带入线校验";
            TaskName = Properties.Resources.PullBeltInLineVerification + subIdx;
            SubIdx = subIdx;
        }

        public override bool GetPLCData()
        {
            channelState = Context.DB510.ChannelStates[SubIdx];
            batteryCode = Context.DB510.BatteryCodes[SubIdx];//.Select(s => Encoding.UTF8.GetString(s.Code).Trim()).ToArray();
            return base.GetPLCData();
        }

        public override async Task<bool> DoSth()
        {
            //Context.Logger.LogInformation();
            int ng1 = 0, ng = 0, ok = 0;
            Console.WriteLine("batteryCode; {0}", batteryCode);
            //if (!channelState) continue;
            if (batteryCode.ToLower() == "noread" || string.IsNullOrWhiteSpace(batteryCode))
            {
                batteryCode = $"Noread{SubIdx}";
                ng1++;
                // 尝试剪一下
            }
            Console.WriteLine("batteryCode1; {0}", batteryCode);

            if (channelState)
            {
                Context.Logger.LogInformation("通道{idx}有料，料号：{code}", SubIdx + 1, batteryCode);
            }
            string[] batteryCodes = [batteryCode];
            //var battery = batteryCode.Select((s, i) => new MaterialInfo(s, i)).Where(s=> channelState[s.Channel]).ToArray();
            var battery = batteryCodes
                .Select((s, i) => new Channel(i + 1, Some(new BatteryWithLevel(s, None))))
                .ToImmutableArray();
            var res = await Context.Mediator.Send(new MaterialOnlineQuery(Context.Name, battery));
            if (res == null) { 
                return false;
            }
            //batteryStatus = Enumerable.Repeat(0, batteryCode.Length).Select(s => (byte)0).ToArray();

            foreach (var item in res)
            {
                BatteryLevel batteryLevel = item.BattleWithLevel.Bind(m => m.Level).Match(Some: s => s, None: () => BatteryLevel.NG);
                batteryStatus =  CZPUtils.GetPlcBatteryLevelCode(batteryLevel);

                if (batteryLevel == BatteryLevel.NG)
                {
                    ng++;
                }
                else if (batteryLevel == BatteryLevel.OK)
                {
                    ok++;
                }
            }
            // bind一下结果
            MainStorage.Saves.OKNGCnt.OKCnt += ok;
            MainStorage.Saves.OKNGCnt.NG2Cnt += ng - ng1;
            MainStorage.Saves.OKNGCnt.NG1Cnt += ng1;


            return await base.DoSth();
        }

        protected bool WriteBatteryStatusAndFeedbackToPlc(byte status, byte feedback)
        {
            _feedbackWritten = false;
            Exception? lastException = null;
            var statusNode = BatteryStatusNodePath.GetOPCAddress(Context.Name);
            var feedbackNode = TaskCodeFeedbackDB.GetFeedbacksOffset(SubIdx).GetOPCAddress(Context.Name);
            if (statusNode == default || feedbackNode == default)
            {
                Context.Logger?.LogWarning("没有此地址, {add}", BatteryStatusNodePath);
                return false;
            }

            for (var attempt = 1; attempt <= 2; attempt++)
            {
                try
                {
                    _feedbackWritten = Context.Plc.WriteNodesBatchedAsync(
                    [
                        new KeyValuePair<string, object>(statusNode, status),
                        new KeyValuePair<string, object>(feedbackNode, feedback)
                    ]).GetAwaiter().GetResult();
                    if (_feedbackWritten)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
               
            }

            // 第二次失败输出错误日志
            Context.Logger?.LogError(lastException, "下发PLC电芯信息失败, 任务: {TaskName}, 通道: {Channel}", TaskName, SubIdx + 1);
            return false;
        }

        protected override async Task WriteDataToPlc()
        {
            WriteBatteryStatusAndFeedbackToPlc(batteryStatus, TaskCode);

            await base.WriteDataToPlc();
        }

        protected override void FeedBackAction()
        {
            if (_feedbackWritten)
            {
                return;
            }
            base.FeedBackAction();
        }

        protected override Task<bool> CleanData()
        {
            //channelState = [];
            //batteryCode = [];
            //batteryStatus = [];

            channelState = false;
            batteryCode = string.Empty;
            batteryStatus = 0;

            WriteBatteryStatusAndFeedbackToPlc(0, 0);

            return base.CleanData();
        }

        protected override void ClearFeedBack()
        {
            if (_feedbackWritten)
            {
                return;
            }
            base.ClearFeedBack();
        }
    }
}
