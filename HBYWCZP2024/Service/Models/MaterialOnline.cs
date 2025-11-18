using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HBYWCZP2024.Service.Models
{


    public class MaterialOnlineRequest(string name)
    {
        public Batterytraybindvolist[] batteryTrayBindVOList { get; set; } = [];
        public string siteCode => MainStorage.Saves.KeysSave[name].SiteCode;
        public string lineCode => MainStorage.Saves.KeysSave[name].LineCode;
        public string equipNum => MainStorage.Saves.KeysSave[name].DeviceCode;
        public string containerCode { get; set; } = "";
        public string actionType { get; set; } = "IN";
    }

    public class Batterytraybindvolist
    {
        public string? identification { get; set; }
        public string? locationBat { get; set; }
    }



    public class MaterialOnlineResponse : IMESResponse
    {
        public Row1[]? rows { get; set; } = [];
        public int total { get; set; }
        [JsonPropertyName("success")]
        public bool Success
        {
            get; set;
        }
        [JsonPropertyName("message")]
        public string? Message
        {
            get; set;
        }
    }

    public class Row1
    {
        public string? identification { get; set; }
        public string? qualityStatus { get; set; } // ok/ng
        public string? materialId { get; set; } // ok/ng

    }

}

namespace HBYWCZP2024
{
    public interface IMESResponse
    {
        bool Success { get; }
        string? Message { get; }
    }
}