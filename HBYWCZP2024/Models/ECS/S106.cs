using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{
    public class S106 : IECSContent
    {
        public new string Type => "S106";
        public int BatteryCount { get; set; }
        public int TrayPosition { get; set; }
        public List<string> BatteryBarcodes { get; set; }
        public string TrayNumber { get; set; }

        public string GetContent()
        {
            var barcodes = string.Join("", BatteryBarcodes.Select(b => b.PadRight(24)));
            return $"{BatteryCount.ToString().PadRight(3)}{TrayPosition.ToString().PadRight(1)}{barcodes}{TrayNumber.PadRight(20)}";
        }

        public void Parse(string content)
        {
            if (content.Length < 28)
            {
                throw new ArgumentException("字符串长度不足!");
            }

            BatteryCount = int.Parse(content.Substring(0, 3).Trim());
            TrayPosition = int.Parse(content.Substring(3, 1).Trim());
            BatteryBarcodes = new List<string>();

            int barcodeLength = 24;
            for (int i = 0; i < BatteryCount; i++)
            {
                int startIndex = 4 + i * barcodeLength;
                if (startIndex + barcodeLength > content.Length - 20)
                {
                    throw new ArgumentException("字符串长度不足，无法解析电芯条码!");
                }
                BatteryBarcodes.Add(content.Substring(startIndex, barcodeLength).Trim());
            }

            TrayNumber = content.Substring(content.Length - 20).Trim();
        }
    }

}
