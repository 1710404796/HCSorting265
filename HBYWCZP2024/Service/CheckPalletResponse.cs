using System.Text.Json.Serialization;

namespace HBYWCZP2024.Service;

public class CheckPalletResponse
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
