using HBYWCZP2024.Models;
using HBYWCZP2024.Utils;
using LinxUniverse.OPCUA.Attributes;
using LinxUniverse.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.PLC.Tasks
{
    [OPCDelectInfo(600,601)]
    public class ApplyBatteryInfoTaskB: ApplyBatteryInfoTask
    {
        public override bool GetPLCData()
        {
            grabStation = Context.DB500B.GrabStation;
            return true;
        }

        protected override Task WriteDataToPlc()
        {

            HBYWCZP2024.Utils.Utils.WriteTagNode(Context.Plc, "WCS_to_JXS2.Data.to_grabStation", grabStation, Context.Name);

            HBYWCZP2024.Utils.Utils.WriteTagNode(Context.Plc, "WCS_to_JXS2.Data.to_channelStatus", _batteryLevels.Select(GetPlcCode).ToArray(), Context.Name);
            HBYWCZP2024.Utils.Utils.WriteTagNode(Context.Plc, "WCS_to_JXS2.Data.to_traytype", isFake ? (byte)2 : (byte)1, Context.Name);

   

            return base.WriteDataToPlc();
        }

        protected override Task<bool> CleanData()
        {
            var pallet = CZPUtils.GetPalletByInsideNum(Context.Name, grabStation);
            if (pallet == null) return Task.FromResult( true);
            HBYWCZP2024.Utils.Utils.WriteTagNode(Context.Plc, "WCS_to_JXS2.Data.to_grabStation", (ushort)0, Context.Name);
            HBYWCZP2024.Utils.Utils.WriteTagNode(Context.Plc, "WCS_to_JXS2.Data.to_channelStatus", new byte[pallet!.ChannelNum], Context.Name);
            HBYWCZP2024.Utils.Utils.WriteTagNode(Context.Plc, "WCS_to_JXS2.Data.to_traytype", (byte)0, Context.Name);


            grabStation = 0;
            _batteryLevels = [];
            isFake = false;
            return Task.FromResult(true);
        }
    }

    [OPCDelectInfo(600,601)]
    public class SinglePlacementCompletedTaskB : SinglePlacementCompletedTask
    {
        public override bool GetPLCData()
        {
            grabStation = Context.DB500B.GrabStation;
            putStaion = Context.DB500B.PutStation;

            grabChannel = Context.DB500B.GrabChannels;
            putChannel = Context.DB500B.PutChannels;

            claw = Context.DB500B.Claws;

            batteryCodes = Context.DB500B.BatteryCodes;//.Select(s => Encoding.UTF8.GetString(s.Code).Trim()).ToArray();

            return true;
        }
        protected override Task WriteDataToPlc()
        {
            Context.Plc.WriteTagNode("WCS_to_JXS2.Data.to_grabStation", grabStation, Context.Name);
            Context.Plc.WriteTagNode("WCS_to_JXS2.Data.to_putStation", putStaion, Context.Name);



            return Task.FromResult(true);
        }

        protected override Task<bool> CleanData()
        {

            Context.Plc.WriteTagNode("WCS_to_JXS2.Data.to_grabStation", (ushort)0, Context.Name);
            Context.Plc.WriteTagNode("WCS_to_JXS2.Data.to_putStation", (ushort)0, Context.Name);

            return Task.FromResult(true);
        }
    }

    [OPCDelectInfo(600, 601)]
    public class ChaiPanCompleted2TaskB : ChaiPanCompleted2Task;

    [OPCDelectInfo(20, 21)]
    public class PalletArrivedTaskB(int idx) : PalletArrivedTask(idx);

    [OPCDelectInfo(20, 21)]
    public class GrabActionTaskB(int idx) : GrabActionTask(idx);
    [OPCDelectInfo(20, 21)]
    public class PalletOutTaskB(int idx) : PalletOutTask(idx);

    [OPCDelectInfo(20, 21)]
    public class GrabActionExitTaskB(int idx) : GrabActionExitTask(idx);





    //[OPCDelectInfo(610,611)]
    //public class CheckLadaiTaskB: CheckLadaiTask;
    //[OPCDelectInfo(610,611)]
    //public class CheckLadaiTask2B: CheckLadaiTask2;
    //[OPCDelectInfo(610,611)]
    //public class CheckLadaiTask3B: CheckLadaiTask3;


}
