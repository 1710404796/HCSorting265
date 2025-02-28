using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.PLC.Models
{
    [OPCMember("PLC_TO_WCS_ScanTheCode.PLC")]
    public class DB510 : ITaskCodeDelectDB
    {
        public DB510() { }


        [OPCMember("action")]
        public byte Action { get; set; }
        //public byte Actions(int idx) => Action;

        [OPCMember("channelState")]
        public bool[] ChannelStates { get; set; } = new bool[16];
        //public bool[] ChannelStates { get; set; } = new bool[20];

        [OPCMember("batteryCode")]
        public string[] BatteryCodes { get; set; } = new string[16];
        //public string[] BatteryCodes { get; set; } = new string[20];
    }
}
