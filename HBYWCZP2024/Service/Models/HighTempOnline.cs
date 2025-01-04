using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HBYWCZP2024.Service.Models
{

    public class HighTempOnlineRequest(string key)
    {
        public string siteCode => MainStorage.Saves.KeysSave[key].SiteCode;
        public string lineCode => MainStorage.Saves.KeysSave[key].LineCode;
        public string equipNum => MainStorage.Saves.KeysSave[key].EquipNum;
        public string materialCode => MainStorage.Saves.KeysSave[key].MaterialCode;
        public string userName => MainStorage.Saves.KeysSave[key].UserName;
        public required string identification { get; set; }
        public string productType => "OK";

    }


    public class HighTempOnlineResponse: IMESResponse
    {
        public string code { get; set; }
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
        public string? category { get; set; }
    }


}
