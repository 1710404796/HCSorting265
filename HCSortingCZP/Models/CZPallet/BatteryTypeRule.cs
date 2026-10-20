using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HCSortingCZP.Models.CZPallet
{
    public record BatteryTypeRule
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; } = string.Empty;
        public string BatteryRegex { get; set; } = string.Empty;
        public BatteryTypeRule(string name, string batteryRegex)
        {
            Name = name;
            BatteryRegex = batteryRegex;
        }
        /// <summary>
        /// 检测电池条码规则 可能只需前13位
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool CheckOk(string code)
        {
            return Regex.IsMatch(code, BatteryRegex);
        }

    }
}
