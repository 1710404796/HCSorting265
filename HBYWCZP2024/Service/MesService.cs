using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HBYWCZP2024.Service;
public class MesService(HttpClient httpClient)
{
    public void CheckMaterialOnline(string name)
    {
        string _url = $"http://{MainStorage.Saves.KeysSave[name].MESIP}/core/api/public/pre/sorting/query";

    }



}

public class CheckMaterialRequest(string name)
{
    [JsonPropertyName("siteCode")]
    public string SiteCode => MainStorage.Saves.KeysSave[name].SiteCode;

    [JsonPropertyName("lineCode")]
    public string LineCode => MainStorage.Saves.KeysSave[name].LineCode;

    [JsonPropertyName("equipNum")]
    public string EquipNum => MainStorage.Saves.KeysSave[name].EquipNum;

    [JsonPropertyName("materialCode")]
    public string MaterialCode => MainStorage.Saves.KeysSave[name].MaterialCode;

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

public class ContainerCellInfo
{
    [JsonPropertyName("identification")]
    public string Identification
    {
        get; set;
    }

    [JsonPropertyName("location")]
    public string Location
    {
        get; set;
    }
}




public class CheckMaterialResponse
{
    [JsonPropertyName("rows")]
    public Row[] Rows
    {
        get; set;
    }

    [JsonPropertyName("success")]
    public bool Success
    {
        get; set;
    }

    [JsonPropertyName("total")]
    public int Total
    {
        get; set;
    }
}

public class Row
{
    [JsonPropertyName("identification")]
    public string Identification
    {
        get; set;
    }

    [JsonPropertyName("containerCode")]
    public string ContainerCode
    {
        get; set;
    }

    [JsonPropertyName("location")]
    public string Location
    {
        get; set;
    }

    [JsonPropertyName("equipNum")]
    public string EquipNum
    {
        get; set;
    }

    [JsonPropertyName("frFrequency")]
    public string FrFrequency
    {
        get; set;
    }

    [JsonPropertyName("grade")]
    public string Grade
    {
        get; set;
    }

    [JsonPropertyName("reason")]
    public string Reason
    {
        get; set;
    }
}