using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HBYWCZP2024.Service;
public class MesService(HttpClient httpClient, string name, ILogger<MesService> logger)
{
    public HttpClient HttpClient { get; } = httpClient;
    public string Name { get; } = name;
    public async Task<CheckMaterialResponse?> CheckMaterialLevel(CheckMaterialRequest checkMaterialRequest)
    {
        string _url = $"http://{MainStorage.Saves.KeysSave[Name].MESIP}/core/api/public/pre/sorting/query";
        logger.LogInformation("请求MES地址: {url}", _url);
        string data = JsonSerializer.Serialize(checkMaterialRequest);
        logger.LogInformation("请求MES数据: {data}", data);
        var res = await httpClient.PostAsJsonAsync(_url, checkMaterialRequest);
        if (res.IsSuccessStatusCode)
        {
            logger.LogInformation("请求MES成功");
            var resdata = await res.Content.ReadAsStringAsync();

            logger.LogInformation("MES返回数据: {resdata}", resdata);
            var response = JsonSerializer.Deserialize<CheckMaterialResponse>(resdata);

            return response;
        }
        else
        {
            return null;
        }

        // 实现接口

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