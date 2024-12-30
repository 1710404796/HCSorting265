using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Service
{


    public class MaterialOnlineRequest
    {
        public Batterytraybindvolist[] batteryTrayBindVOList { get; set; }
        public string siteCode { get; set; }
        public string lineCode { get; set; }
        public string equipNum { get; set; }
        public string containerCode { get; set; }
        public string logisticsOperationCode { get; set; }
        public string actionType { get; set; }
    }

    public class Batterytraybindvolist
    {
        public string identification { get; set; }
        public string locationBat { get; set; }
    }

}
