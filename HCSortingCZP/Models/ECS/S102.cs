using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Models.ECS
{
    /// <summary>
    /// 托盘到达
    /// 当托盘到达设备后上报
    /// </summary>
    public class S102 : IECSContent
    {
        public string Type { get; set; }=  "S102";

        public required int TrayPosition { get; set; }
        public required TrayType TrayType { get; set; }
        public required string TrayNumber { get; set; }

        public virtual string GetContent()
        {
            return $"{TrayPosition}{(int)TrayType}{TrayNumber,-20}";
        }

        public void Parse(string content)
        {
            if (content.Length != 22)
            {
                throw new ArgumentException("字符串长度不足!");
            }

            TrayPosition = int.Parse(content.Substring(0, 1));
            TrayType = (TrayType)(content[1] - '0');
            TrayNumber = content.Substring(2, 20).Trim();
        }
    }

 
}
