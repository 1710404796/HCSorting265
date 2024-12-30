using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Service
{


    public class MaterialOnlineRequest(string name)
    {
        public Batterytraybindvolist[] batteryTrayBindVOList { get; set; } = [];
        public string siteCode => MainStorage.Saves.KeysSave[name].SiteCode;
        public string lineCode => MainStorage.Saves.KeysSave[name].LineCode;
        public string equipNum => MainStorage.Saves.KeysSave[name].EquipNum;
        public string containerCode { get; set; } = "";
        public string actionType { get; set; } = "IN";
    }

    public class Batterytraybindvolist
    {
        public string? identification { get; set; }
        public string? locationBat { get; set; }
    }



    public class MaterialOnlineResponse
    {
        public Row1[]? rows { get; set; } = [];
        public bool success { get; set; }
        public int total { get; set; }
        public string message { get; set; } = string.Empty;
    }

    public class Row1
    {
        public string? identification { get; set; }
        public string? qualityStatus { get; set; } // ok/ng
      
    }

}
