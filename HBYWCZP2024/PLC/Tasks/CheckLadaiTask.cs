
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Mediator.Queries;
using HBYWCZP2024.Models;
using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HBYWCZP2024.PLC.Tasks
{
    [TaskName("拉带入线校验")]

    [OPCDelectInfo(510, 511)]
    public class CheckLadaiTask : HBBaseTask
    {
        private bool[] channelState = [];
        private string[] batteryCode = [];
        private byte[] batteryStatus = [];

        public CheckLadaiTask() : base(1)
        {
            //TaskName = "拉带入线校验";
            TaskName = Properties.Resources.PullBeltInLineVerification;
        }

        public override bool GetPLCData()
        {
            channelState = Context.DB510.ChannelStates;
            batteryCode = Context.DB510.BatteryCodes;//.Select(s => Encoding.UTF8.GetString(s.Code).Trim()).ToArray();
            return base.GetPLCData();
        }

        public override async Task<bool> DoSth()
        {
            //Context.Logger.LogInformation();
            int ng1 = 0, ng = 0, ok = 0;
            for (int i = 0; i < batteryCode.Length; i++)
            {
                if (batteryCode[i].ToLower() == "noread")
                {
                    batteryCode[i] = $"Noread{i}";
                    ng1++;
                    // 尝试剪一下
                }

                if (channelState[i])
                {
                    Context.Logger.LogInformation("通道{idx}有料，料号：{code}", i + 1, batteryCode[i]);
                }

            }
            //var battery = batteryCode.Select((s, i) => new MaterialInfo(s, i)).Where(s=> channelState[s.Channel]).ToArray();
            var battery = batteryCode.Select((s, i) => new Channel(i, Some(new BatteryWithLevel(s, None)))).ToImmutableArray();
            var res = await Context.Mediator.Send(new MaterialOnlineQuery(Context.Name, battery));
            if (res == null) { 
                return false;
            }
            batteryStatus = Enumerable.Repeat(0, batteryCode.Length).Select(s => (byte)0).ToArray();

            foreach (var item in res)
            {
                BatteryLevel batteryLevel = item.BattleWithLevel.Bind(m => m.Level).Match(Some: s => s, None: () => BatteryLevel.NG);
                batteryStatus[item.ChannelId] = (byte)batteryLevel;

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

        protected override async Task WriteDataToPlc()
        {

            Utils.Utils.WriteTagNode(Context.Plc, "WCS_TO_PLC_ScanTheCode.WCS.to_batteryStatus", batteryStatus, Context.Name);

            await base.WriteDataToPlc();
        }

        protected override Task<bool> CleanData()
        {
            channelState = [];
            batteryCode = [];
            batteryStatus = [];

            Utils.Utils.WriteTagNode(Context.Plc, "WCS_TO_PLC_ScanTheCode.WCS.to_batteryStatus", new byte[16], Context.Name);

            return base.CleanData();
        }
    }
}
