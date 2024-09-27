using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{
    public class E103 : IECSContent
    {
        public string Type => "E103";

        public int BatteryCount { get; set; }
        public string BatteryStatuses { get; set; }
        public int TrayPosition { get; set; }

        public BatteryLevel[] GetBatteryLevels()
        {
            var levels = new BatteryLevel[BatteryCount];
            for (int i = 0; i < BatteryCount; i++)
            {
                levels[i] = BatteryStatuses[i] switch
                {
                    '0' => BatteryLevel.EMPTY,
                    '1' => BatteryLevel.OK,
                    '2' => BatteryLevel.NG,
                    _ => BatteryLevel.Unknown
                };
            }
            return levels;
        }

        public string GetContent()
        {
            return $"{BatteryCount,-3}{BatteryStatuses}{TrayPosition}";
        }

        public void Parse(string content)
        {
            int n = int.Parse(content.Substring(0, 3).Trim());

            if (content.Length != n + 4)
            {
                throw new ArgumentException("字符串长度不足!");
            }

            BatteryCount = n;
            BatteryStatuses = content.Substring(3, n);
            TrayPosition = int.Parse(content.Substring(3 + n, 1));
        }
    }

}
