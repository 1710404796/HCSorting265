using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.PLC.Models
{
    [OPCMember("WCS_TO_PLC_ScanTheCode.WCS")]
    public class DB511 : ITaskCodeFeedbackDB
    {
        public DB511()
        {
        }

        [OPCMember("to_feedback")]
        public byte ToFeedback { get; set; }
        public byte ToFeedbacks(int idx) => ToFeedback;

        [OPCMember("to_batteryStatus")]
        public byte[] ToBatteryStatus { get; set; } = new byte[8];
    }
}
