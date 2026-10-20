using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.PLC.Models
{
    [OPCMember("WCS_TO_PLC_ScanTheCode.WCS")]
    public class DB511 : ITaskCodeFeedbackDB
    {
        public DB511()
        {
        }
        [OPCMember("to_feedback")]

        public byte[] LaidaiFeedack{ get; set; } = new byte[8];


        public byte ToFeedback { get; set; }
        public byte ToFeedbacks(int idx) => LaidaiFeedack[idx];

        [OPCMember("to_batteryStatus")]
        public byte[] ToBatteryStatus { get; set; } = new byte[8];

        string ITaskCodeFeedbackDB.GetFeedbacksOffset(int idx)
        {
            var baseName = GetType().GetCustomAttribute<OPCMemberAttribute>()?.Name;
            if (baseName is null)
            {
                throw new Exception("not found OPCMemberAttributes");
            }
            var mName = "to_feedback";
            // 先这样 等下做缓存
            return $"{baseName}.{mName}.[{idx}]";
        }
    }
}
