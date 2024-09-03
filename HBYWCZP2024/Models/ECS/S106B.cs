using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{
    public class S106B : IECSContent
    {
        public new string Type => "S106";
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

            TrayPosition = int.Parse(content.Substring(0, 1));
            TrayNumber = content.Substring(1, 20).Trim();
        }
    }

}
