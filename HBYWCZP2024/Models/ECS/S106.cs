using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{
    /// <summary>
    /// 设备作业结束(组盘)
    /// 当整托盘作业结束时上报
    /// </summary>
    public class S106 : S102
    {
        //public new string Type => "S106";
        public S106()
        {
            Type = "S106";
        }
        public required string OutSideStationNum { get; set; }

        public required PalletStatus PalletStatus { get; set; }

        public override string GetContent()
        {
            return $"{TrayPosition}{(int)PalletStatus}{(int)TrayType}{OutSideStationNum,-4}{TrayNumber,-20}";
            //return base.GetContent()/*;*/
        }
        //public int BatteryCount { get; set; }
        //public int TrayPosition { get; set; }
        //public List<string> BatteryBarcodes { get; set; } = [];
        //public string TrayNumber { get; set; } = string.Empty;

        //public string GetContent()
        //{
        //    var barcodes = string.Join("", BatteryBarcodes.Select(b => b.PadRight(24)));
        //    return $"{BatteryCount,-3}{TrayPosition,-1}{TrayNumber,-20}{barcodes}";
        //}

        //public void Parse(string content)
        //{
        //    if (content.Length < 28)
        //    {
        //        throw new ArgumentException("字符串长度不足!");
        //    }

        //    BatteryCount = int.Parse(content.Substring(0, 3).Trim());
        //    TrayPosition = int.Parse(content.Substring(3, 1).Trim());

        //    TrayNumber = content.Substring(4, 20).Trim();
        //    BatteryBarcodes = [];

        //    int barcodeLength = 24;
        //    for (int i = 0; i < BatteryCount; i++)
        //    {
        //        int startIndex = 24 + i * barcodeLength;
        //        if (startIndex + barcodeLength > content.Length)
        //        {
        //            throw new ArgumentException("字符串长度不足，无法解析电芯条码!");
        //        }
        //        BatteryBarcodes.Add(content.Substring(startIndex, barcodeLength).Trim());
        //    }


        //}
    }

}

namespace HBYWCZP2024
{
    public enum PalletStatus
    {
        不满 = 0,
        满盘 = 1,
    }
}