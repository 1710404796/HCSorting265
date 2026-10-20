using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Models.ECS
{
    public class E104 : E102
    {
        public E104()
        {
            Type = "E104";
        }

    }

    //: IECSContent
    //{
    //    public DeviceStatus2 DeviceStatus { get; set; }
    //    public string ErrorCode { get; set; }

    //    public string GetContent()
    //    {
    //        return $"{(int)DeviceStatus}{ErrorCode.PadLeft(4, '0')}";
    //    }

    //    public void Parse(string content)
    //    {
    //        if (content.Length != 5)
    //        {
    //            throw new ArgumentException("字符串长度不足!");
    //        }

    //        DeviceStatus = (DeviceStatus2)(content[0] - '0');
    //        ErrorCode = content.Substring(1, 4);
    //    }
    //}


}
