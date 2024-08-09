using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{
    public class E101 : IECSContent
    {
        public DeviceStatus2 DeviceStatus { get; set; }

        public string GetContent()
        {
            return $"{(int)DeviceStatus}";
        }

        public void Parse(string content)
        {
            if (content.Length != 1)
            {
                throw new ArgumentException("字符串长度不足!");
            }

            DeviceStatus = (DeviceStatus2)(content[0] - '0');
        }
    }


}
