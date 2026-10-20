using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Models.ECS
{

    public class E103 : IECSContentRes
    {
        public string Type => "E103";

        public int BatteryCount { get; set; }
        public string[] BatteryCodes { get; set; } = [];
        public string TrayNumber { get; set; } = string.Empty;

        public string ErrorCode => "0000";

        public string GetContent()
        {
            return $"{TrayNumber}{BatteryCount,-3}{string.Concat(BatteryCodes.Select(s => s.PadRight(24)))}";
        }

        public void Parse(string content)
        {
            int n = int.Parse(content.Substring(20, 3).Trim());

            //if (content.Length != n + 4)
            //{
            //    throw new ArgumentException("字符串长度不足!");
            //}

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
