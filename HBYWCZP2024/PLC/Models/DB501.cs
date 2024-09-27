using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.PLC.Models
{
    [OPCMember("WCS_to_JXS.Data")]
    public class DB501 : ITaskCodeFeedbackDB
    {
        public DB501()
        {

        }

        [OPCMember("to_heartbeat")]
        public byte ToHeartbeat { get; set; }

        [OPCMember("to_traytype")]
        public byte ToTrayType { get; set; }
        [OPCMember("to_feedback")]

        public byte ToFeedback { get; set; }


        [OPCMember("to_error")]
        public byte ToErrorCode { get; set; }

        [OPCMember("to_grabStation")]
        public ushort ToGrabStation { get; set; }

        [OPCMember("to_putStation")]
        public ushort ToPutStation { get; set; }


        [OPCMember("to_Quantity")]
        public ushort ToQuantity { get; set; }

        [OPCMember("to_channelStatus")]
        public byte[] ToChannelStatus { get; set; } = new byte[48];


        /// <summary>
        /// 如果为1 就是假电芯
        /// </summary>
        [OPCMember("to_fake")]
        public byte ToFake { get; set; }


        //[OPCMember("to_applyStation")]
        //public ushort ToApplyStation { get; set; }

        //[OPCMember("to_applyStatus")]
        //public byte[] ToApplyStatus { get; set; } = new byte[256];

        public byte ToFeedbacks(int idx) => ToFeedback;
    }
}
