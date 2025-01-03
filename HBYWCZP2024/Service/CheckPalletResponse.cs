using System.Text.Json.Serialization;

namespace HBYWCZP2024.Service;

public class CheckPalletResponse: IMESResponse
{
    [JsonPropertyName("rows")]
    public Row[] Rows
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
