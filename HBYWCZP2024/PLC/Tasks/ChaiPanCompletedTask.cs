using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Utils;
using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.PLC.Tasks
{
    [OPCDelectInfo(500, 501)]

    public class ChaiPanCompletedTask : HBBaseTask
    {
        private ushort grabStation;

        public ChaiPanCompletedTask() : base(6)
        {
            TaskName = "拆盘完成";
            //TaskName = Properties.Resources.DisassemblyCompleted;
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
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"未找到工位: {grabStation}", SaveLog: false));

                return false;
            }

            var res = await Context.Mediator.Send(new GrabActionCommand(Context.Name, grabStation, false));
            return res;
            //return await base.DoSth();
        }

        protected override Task WriteDataToPlc()
        {
            //Context.Plc.Write("DB501.DBW4", grabStation);
            //Context.Plc.WriteNode("WCS_to_JXS.Data.to_grabStation".GetOPCAddress(), grabStation);
            Context.Plc.WriteTagNode("WCS_to_JXS.Data.to_grabStation", grabStation, Context.Name);

            return base.WriteDataToPlc();
        }

        protected override Task<bool> CleanData()
        {
            //Context.Plc.Write("DB501.DBW4", 0);
            //Context.Plc.WriteNode("WCS_to_JXS.Data.to_grabStation".GetOPCAddress(), 0);
            Context.Plc.WriteTagNode("WCS_to_JXS.Data.to_grabStation", (ushort)0, Context.Name);

            grabStation = 0;
            return base.CleanData();
        }
    }
}
