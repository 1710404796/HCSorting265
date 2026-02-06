using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HBYWCZP2024.Service.Models
{
    /// <summary>
    /// 空托盘校验
    /// </summary>
    /// <param name="name"></param>
    public class CheckNullTrayRequest(string name)
    {
        public string containerCode { get; set; }

        public string containerTypeCode => MainStorage.Saves.KeysSave[name].ContainerTypeCode;
    }

    public class CheckNullTrayResponse : IMESResponse
    {
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
}
