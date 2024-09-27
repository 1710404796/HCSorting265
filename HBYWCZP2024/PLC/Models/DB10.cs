
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;
namespace HBYWCZP2024.PLC.Models
{
    [OPCMember("PLC_TO_WCS_Tray")]
    public class DB10 : ITaskCodeDelectDB
    {
        public DB10()
        {

        }
        [OPCMember("Tray_action")]

        public byte[] PalletArrvied { get; set; } = new byte[16];

        public byte Action => throw new NotImplementedException();

        public byte Actions(int idx)
        {
            return PalletArrvied[idx];
        }


    }
}
