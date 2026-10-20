using System.Text.Json.Serialization;

namespace HCSortingCZP.Service.Models;

public class ContainerManualResponse : IMESResponse
{
    [JsonPropertyName("rows")]
    public ContainerManualRow[] Rows { get; set; } = [];

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class ContainerManualRow
{
    [JsonPropertyName("containerCode")]
    public string? ContainerCode { get; set; }

    [JsonPropertyName("containerName")]
    public string? ContainerName { get; set; }

    [JsonPropertyName("operationName")]
    public string? OperationName { get; set; }

    [JsonPropertyName("operationDesc")]
    public string? OperationDesc { get; set; }

    [JsonPropertyName("materialCode")]
    public string? MaterialCode { get; set; }

    [JsonPropertyName("cellDataList")]
    public ContainerManualCellData[] CellDataList { get; set; } = [];
}

public class ContainerManualCellData
{
    [JsonPropertyName("identification")]
    public string? Identification { get; set; }

    [JsonPropertyName("locationBat")]
    public string? LocationBat { get; set; }

    [JsonPropertyName("qualityStatus")]
    public string? QualityStatus { get; set; }

    [JsonPropertyName("operationId")]
    public string? OperationId { get; set; }

    [JsonPropertyName("eoId")]
    public string? EoId { get; set; }
}
