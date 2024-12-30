
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LinxUniverse.OPCUA.Attributes;
using LinxUniverse.OPCUA;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Utils;
using HBYWCZP2024.Mediator.Commands;

namespace HBYWCZP2024.PLC.Tasks
{
    [OPCDelectInfo(500, 501)]

    public class InstallationCompletedTask : HBBaseTask
    {
        private ushort putStation;

        public InstallationCompletedTask() : base(7)
        {
            TaskName = "组盘完成";
            //TaskName = Properties.Resources.AssemblyCompleted;
        }

        public override bool GetPLCData()
        {
            putStation = Context.DB500.PutStation;
            return base.GetPLCData();
        }

        public override async Task<bool> DoSth()
        {
            var pallet = CZPUtils.GetPalletByInsideNum(Context.Name, putStation);
            if (pallet == null)
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"未找到工位 {putStation}", SaveLog: false));

                return false;
            }

            var res = await Context.Mediator.Send(new GrabActionCommand(Context.Name, putStation, false));
            return res;
            //return await base.DoSth();
        }

        protected override Task WriteDataToPlc()
        {
            //Context.Plc.WriteNode("WCS_TO_PLC_ScanTheCode.WCS.to_putStation".GetOPCAddress(), putStation);
            Context.Plc.WriteTagNode("WCS_to_JXS.Data.to_putStation", putStation, Context.Name);

            return base.WriteDataToPlc();

        }

        protected override Task<bool> CleanData()
        {
            //Context.Plc.WriteNode("WCS_TO_PLC_ScanTheCode.WCS.to_putStation".GetOPCAddress(), 0);
            Context.Plc.WriteTagNode("WCS_to_JXS.Data.to_putStation", (ushort)0, Context.Name);

            putStation = 0;
            return base.CleanData();
        }
    }
}
