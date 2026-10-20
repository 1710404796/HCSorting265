using HCSortingCZP.Mediator.Commands;
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
    public class AbnormalDischargeTask : HBBaseTask
    {
        private ushort grabStation;

        public AbnormalDischargeTask() : base(10)
        {
            TaskName = "异常排出";
            //TaskName = Properties.Resources.ExceptionEjection;
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
            pallet.ClearStage();
            return await base.DoSth();
        }


        protected override Task WriteDataToPlc()
        {
            //Context.Plc.WriteNode("DB501.DBW4", grabStation);
            //Context.Plc.WriteNode("WCS_to_JXS.Data.to_grabStation".GetOPCAddress(), grabStation);
            Context.Plc.WriteTagNode("WCS_to_JXS.Data.to_grabStation", grabStation, Context.Name);

            return base.WriteDataToPlc();
        }

        protected override Task<bool> CleanData()
        {
            //Context.Plc.WriteNode("WCS_to_JXS.Data.to_grabStation".GetOPCAddress(), 0);
            Context.Plc.WriteTagNode("WCS_to_JXS.Data.to_grabStation", (ushort)0, Context.Name);

            grabStation = 0;
            return base.CleanData();
        }
    }
}
