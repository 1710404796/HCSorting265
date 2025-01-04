using System.Text.Json.Serialization;

namespace HBYWCZP2024.Service.Models;

public class CheckPalletMaterialRequest(string name)
{
    [JsonPropertyName("siteCode")]
    public string SiteCode => MainStorage.Saves.KeysSave[name].SiteCode;

    [JsonPropertyName("lineCode")]
    public string LineCode => MainStorage.Saves.KeysSave[name].LineCode;

    [JsonPropertyName("equipNum")]
    public string EquipNum => MainStorage.Saves.KeysSave[name].EquipNum;

    //[JsonPropertyName("materialCode")]
    //public string MaterialCode => MainStorage.Saves.KeysSave[name].MaterialCode;

    [JsonPropertyName("productType")]
    public string ProductType => MainStorage.Saves.KeysSave[name].ProductType;

    [JsonPropertyName("containerCode")]
    public string ContainerCode
    {
        get; set;
    }

    [JsonPropertyName("containerCellInfos")]
    public required ContainerCellInfo[] ContainerCellInfos
    {
        get; set;
    }
}
