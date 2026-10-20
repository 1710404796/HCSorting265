using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Models.ECS
{
    public class S204 : IECSContent
    {
        public new string Type => "S204";
        public ErrorStatus DeviceStatus { get; set; }

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

            DeviceStatus = (ErrorStatus)(content[0] - '0');
        }
    }


}
