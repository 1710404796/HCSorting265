using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{
    public class S103 : IECSContent
    {
        public new string Type => "S103";
        public string TrayNumber { get; set; }

        public string GetContent()
        {
            return $"{TrayNumber.PadRight(20)}";
        }

        public void Parse(string content)
        {
            if (content.Length != 20)
            {
                throw new ArgumentException("字符串长度不足!");
            }

            TrayNumber = content.Trim();
        }
    }

}
