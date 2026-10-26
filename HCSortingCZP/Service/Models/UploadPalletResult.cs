using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HCSortingCZP.Service.Models
{

    public class UploadPalletResultRequest(string key)
    {
        public string siteCode => MainStorage.Saves.KeysSave[key].SiteCode;
        public string lineCode => MainStorage.Saves.KeysSave[key].LineCode;
        public string equipNum => MainStorage.Saves.KeysSave[key].EquipmentCode;
        public string containerCode { get; set; }
        public string actionType { get; set; } = "IN";
        public Celldatavolist[] cellDataVOList { get; set; } = [];
    }

    public class Celldatavolist
    {
        public  string identification { get; set; }
        public  string locationBat { get; set; }
    }


    public class UploadPalletResultResponse : IMESResponse
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
