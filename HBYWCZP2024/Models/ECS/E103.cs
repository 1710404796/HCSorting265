using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{

    //public class E103B : IECSContent
    //{
    //    public string Type => "E103";
    //    public string BatteryStatus
    //    {
    //        get; set;
    //    } = string.Empty;
    //    public int BatteryCount { get; set; }

    //    public BatteryLevel[] GetBatteryLevels()
    //    {
    //        var levels = new BatteryLevel[BatteryCount];
    //        for (int i = 0; i < BatteryCount; i++)
    //        {
    //            levels[i] = BatteryStatus[i] switch
    //            {
    //                '0' => BatteryLevel.EMPTY,
    //                '1' => BatteryLevel.OK,
    //                '2' => BatteryLevel.NG,
    //                _ => BatteryLevel.Unknown
    //            };
    //        }
    //        return levels;
    //    }

    //    public string GetContent()
    //    {
    //        return $"{BatteryCount,-3}{BatteryStatus}";
    //    }

    //    public void Parse(string content)
    //    {
    //        BatteryCount = int.Parse(content[..3].Trim());
    //        BatteryStatus = content[3..];
    //    }
    //}
    // 筛选所用协议类
    public class E103 : IECSContent
    {
        public string Type => "E103";

        public int BatteryCount { get; set; }
        public string[] BatteryCodes { get; set; } = [];
        public string TrayNumber { get; set; } = string.Empty;

       

        public string GetContent()
        {
            return $"{TrayNumber}{BatteryCount,-3}{string.Concat(BatteryCodes.Select(s => s.PadRight(24)))}";
        }

        public void Parse(string content)
        {
            int n = int.Parse(content.Substring(20, 3).Trim());

            if (content.Length != n + 4)
            {
                throw new ArgumentException("字符串长度不足!");
            }

            BatteryCount = n;
            TrayNumber = content[..20].Trim();
            BatteryCodes = new string[n];
            for (int i = 0; i < n; i++)
            {
                BatteryCodes[i] = content.Substring(23 + i * 24, 24).Trim();
            }
        }
    }

}
