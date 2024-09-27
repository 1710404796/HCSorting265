using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{
    /// <summary>
    /// 设备作业开始
    /// 当整托盘作业开始时上报
    /// </summary>
    public class S104 : IECSContent
    {
        public new string Type => "S104";
        public int TrayPosition { get; set; }
        public string TrayNumber { get; set; }

        public string GetContent()
        {
            return $"{TrayPosition}{TrayNumber.PadRight(20)}";
        }

        public void Parse(string content)
        {
            if (content.Length != 21)
            {
                throw new ArgumentException("字符串长度不足!");
            }

            TrayPosition = content[0] - '0';
            TrayNumber = content[1..].Trim();
        }
    }

}
