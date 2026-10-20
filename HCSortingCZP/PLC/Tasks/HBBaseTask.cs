using HCSortingCZP.Service;
using LinxUniverse.OPCUA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.PLC.Tasks
{
    public class HBBaseTask : CoreTask<HBYWPLCContext>
    {
        //public required HBECSService HBECSService { get; init; }
        //public required string Context.Name { get; init; }
        public HBBaseTask(byte taskCode) : base(taskCode)
        {
        }
    }
}
