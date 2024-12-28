using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS;
/// <summary>
/// 抓取电池结果上报
/// 机械手每抓一次电池都要进行上报
/// </summary>
public class S105 : IECSContent
{
        public new string Type => "S105";
    public int BatteryCount { get; set; }
    public List<string> BatteryBarcodes { get; set; }
    public string TrayNumber { get; set; }
    public int TrayPosition { get; set; }
    public TrayType TrayType { get; set; }

    public string GetContent()
    {
        var barcodes = string.Join("", BatteryBarcodes.Select(b => b.PadRight(24)));
        return $"{TrayPosition}{(int)TrayType}{TrayNumber,-20}{BatteryCount,-3}{barcodes}";
    }

    public void Parse(string content)
    {
        if (content.Length < 24)
        {
            throw new ArgumentException("字符串长度不足!");
        }


        TrayPosition = 

        BatteryCount = int.Parse(content.Substring(0, 2).Trim());
        BatteryBarcodes = new List<string>();

        int barcodeLength = 24;
        for (int i = 0; i < BatteryCount; i++)
        {
            int startIndex = 2 + i * barcodeLength;
            if (startIndex + barcodeLength > content.Length - 20)
            {
                throw new ArgumentException("字符串长度不足，无法解析电芯条码!");
            }
            BatteryBarcodes.Add(content.Substring(startIndex, barcodeLength).Trim());
        }

        TrayNumber = content.Substring(content.Length - 20).Trim();
    }
}
