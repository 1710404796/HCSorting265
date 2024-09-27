
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;

namespace HBYWCZP2024.PLC.Models
{
    [OPCMember("WCS_JXS.Data")]

    public class DB500 : ITaskCodeDelectDB
    {
        public DB500()
        {

        }

        [OPCMember("heartbeat")]
        public byte Heartbeat { get; set; }

        [OPCMember("mode")]

        public byte Mode { get; set; }


        [OPCMember("action")]


        // 思考检测特性 而不是用接口
        public byte Action { get; set; }


        [OPCMember("error")]

        public ushort ErrorCode { get; set; }
        [OPCMember("grabStation")]

        public ushort GrabStation { get; set; }

        [OPCMember("putStation")]

        public ushort PutStation { get; set; }


        [OPCMember("claw")]

        public bool[] Claws { get; set; } = new bool[16];

        [OPCMember("grabChannel")]

        public ushort[] GrabChannels { get; set; } = new ushort[16];

        [OPCMember("putChannel")]

        public ushort[] PutChannels { get; set; } = new ushort[16];



        [OPCMember("battery")]

        public string[] BatteryCodes { get; set; } = new string[16];


        [OPCMember("applyStation")]

        public ushort ApplyStation { get; set; }
        [OPCMember("quantity")]

        public ushort[] Quantity { get; set; } = new ushort[3];

        //public byte Actions(int idx) => Action;
    }

    [OPCMember]

    public class Codes
    {
        [OPCMember("code")]

        public byte[] Code { get; set; } = new byte[32];
    }
}