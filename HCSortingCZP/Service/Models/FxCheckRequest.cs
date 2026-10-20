using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HCSortingCZP.Service.Models
{

    /// <summary>
    /// 分选检查请求，参数从KeysSave里取
    /// </summary>
    /// <param name="name"></param>
    public class FxCheckRequest(string name)
    {
        [JsonPropertyName("siteCode")]
        public string SiteCode => MainStorage.Saves.KeysSave[name].SiteCode;

        [JsonPropertyName("lineCode")]
        public string LineCode => MainStorage.Saves.KeysSave[name].LineCode;

        [JsonPropertyName("equipCode")]
        public string EquipNum => MainStorage.Saves.KeysSave[name].DeviceCode;
        public int qty{ get; set; }
        //[JsonPropertyName("materialCode")]
        //public string MaterialCode => MainStorage.Saves.KeysSave[name].MaterialCode;

        [JsonPropertyName("productType")]
        public string ProductType => MainStorage.Saves.KeysSave[name].ProductType;

        public string materialCode => "91029142";
        public string userName => MainStorage.Saves.KeysSave[name].UserName;

        [JsonPropertyName("containerCode")]
        public string ContainerCode
        {
            get; set;
        } = string.Empty;

        [JsonPropertyName("materiallotCodeList")]
        public required string[] MateriallotCodeList
        {
            get; set;
        }
    }



    

    public class FxCheckResponse : IMESResponse
    {
        public class Row
        {
            public string identification { get; set; }
            public object level { get; set; }
            public string rank { get; set; }
            public object passage { get; set; }
            public object message { get; set; }
            public object ngType { get; set; }
            public string engravingTime { get; set; }
        }
        [JsonPropertyName("rows")]
        public FxCheckResponse.Row[] Rows
        {
            get; set;
        } = [];

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

        [JsonPropertyName("total")]
        public int Total
        {
            get; set;
        }
    }

}
