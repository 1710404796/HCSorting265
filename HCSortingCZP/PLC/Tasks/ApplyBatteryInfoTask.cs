
using HCSortingCZP.Mediator.Commands;
using HCSortingCZP.Mediator.ECS;
using HCSortingCZP.Models;
using HCSortingCZP.PLC.Tasks;
using HCSortingCZP.Utils;
using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.PLC.Tasks
{
    [OPCDelectInfo(500, 501)]

    public class ApplyBatteryInfoTask : HBBaseTask
    {
        protected ushort grabStation;
        protected BatteryLevel[] _batteryLevels;
        protected bool isFake;


        public ApplyBatteryInfoTask(): base(1)
        {
            TaskName = "申请组盘信息";
            //TaskName = Properties.Resources.RequestAssemblyInformation;
        }

        public override bool GetPLCData()
        {
            grabStation = Context.DB500.GrabStation;
            return base.GetPLCData();
        }

        public override async Task<bool> DoSth()
        {
            var pallet = CZPUtils.GetPalletByInsideNum(Context.Name, grabStation);
            if (pallet == null)
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"未找到工位 {grabStation}", SaveLog: false));

                return false;
            }

            var res = await Context.Mediator.Send(new ApplyBatteryInfoCommand(Context.Name, pallet.OutsideStationNum));

            if (res == null)
            {
                return false;
            }

            

            _batteryLevels = res;


            return await base.DoSth();
        }

        protected override Task WriteDataToPlc()
        {

            //Context.Plc.Write("DB501.DBW4", grabStation);
            //Context.Plc.WriteNode("WCS_to_JXS.Data.to_grabStation".GetOPCAddress(), grabStation);
            HCSortingCZP.Utils.Utils.WriteTagNode(Context.Plc, "WCS_to_JXS.Data.to_grabStation", grabStation, Context.Name);

            //Context.Plc.WriteNode("WCS_to_JXS.Data.to_channelStatus".GetOPCAddress(), _batteryLevels.Select(GetPlcCode).ToArray());
            HCSortingCZP.Utils.Utils.WriteTagNode(Context.Plc, "WCS_to_JXS.Data.to_channelStatus", _batteryLevels.Select(GetPlcCode).ToArray(), Context.Name);
            HCSortingCZP.Utils.Utils.WriteTagNode(Context.Plc, "WCS_to_JXS.Data.to_traytype", isFake ? (byte)2 : (byte)1, Context.Name);

            //Context.Plc.WriteBytes(S7.Net.DataType.DataBlock, 501, 10, 
            //    _batteryLevels.Select(GetPlcCode).ToArray()
            //    );

            return base.WriteDataToPlc();
        }

        protected override Task<bool> CleanData()
        {
            var pallet = CZPUtils.GetPalletByInsideNum(Context.Name, grabStation);
            if (pallet == null) return base.CleanData();
            //Context.Plc.Write("DB501.DBW4", 0);
            //Context.Plc.WriteBytes(S7.Net.DataType.DataBlock, 501, 10, Enumerable.Repeat((byte)0, 256).ToArray());
            //Context.Plc.WriteNode("WCS_to_JXS.Data.to_grabStation".GetOPCAddress(), 0);
            HCSortingCZP.Utils.Utils.WriteTagNode(Context.Plc, "WCS_to_JXS.Data.to_grabStation", (ushort)0, Context.Name);
            HCSortingCZP.Utils.Utils.WriteTagNode(Context.Plc, "WCS_to_JXS.Data.to_channelStatus", new byte[pallet!.ChannelNum], Context.Name);
            HCSortingCZP.Utils.Utils.WriteTagNode(Context.Plc, "WCS_to_JXS.Data.to_traytype", (byte)0, Context.Name);

            //Context.Plc.WriteNode("WCS_to_JXS.Data.to_channelStatus".GetOPCAddress(), Enumerable.Repeat((byte)0, 48).ToArray());

            grabStation = 0;
            _batteryLevels = [];
            isFake = false;
            return base.CleanData();
        }
        public byte GetPlcCode(BatteryLevel batteryLevel)
        {
            return CZPUtils.GetPlcBatteryLevelCode(batteryLevel);
            if (batteryLevel == BatteryLevel.None) return (byte)BatteryLevel.EMPTY;
            if (batteryLevel == BatteryLevel.Fake) return 11;

            return (byte)batteryLevel;
        }
    }
}
