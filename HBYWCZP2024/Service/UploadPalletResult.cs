using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HBYWCZP2024.Service
{

    public class UploadPalletResultRequest(string key)
    {
        public string siteCode => MainStorage.Saves.KeysSave[key].SiteCode;
        public string lineCode => MainStorage.Saves.KeysSave[key].LineCode;
        public string equipNum => MainStorage.Saves.KeysSave[key].EquipNum;
        public required string containerCode { get; set; }
        public string actionType { get; set; } = "IN";
        public required Celldatavolist[] cellDataVOList { get; set; } = [];
    }

    public class Celldatavolist
    {
        public required string identification { get; set; }
        public required string locationBat { get; set; }
    }


    public class UploadPalletResultResponse: IMESResponse
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
