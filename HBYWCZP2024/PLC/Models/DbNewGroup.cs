using LinxUniverse.OPCUA.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.PLC.Models
{
    [OPCMember("WCS_TO_PLC_Tray2")]
    public class DB11B : DB11;

    [OPCMember("PLC_TO_WCS_Tray2")]
    public class DB10B : DB10;


    [OPCMember("PLC_TO_WCS_ScanTheCode2.PLC")]
    public class DB510B : DB510;
    [OPCMember("WCS_TO_PLC_ScanTheCode2.WCS")]
    public class DB511B : DB511;


    [OPCMember("WCS_JXS2.Data")]
    public class DB500B : DB500;
    [OPCMember("WCS_to_JXS2.Data")]
    public class DB501B : DB501;
}
