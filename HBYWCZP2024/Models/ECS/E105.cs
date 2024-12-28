using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{
    public class E105 : E102 {
        public new string Type => "E105";
    }

    /// <summary>
    /// 作业结束
    /// </summary>
    public class E106 : E102 
    {
        public new string Type => "E106";
    }

    /// <summary>
    /// 托盘离开
    /// </summary>
    public class E107 : E102 {
        public new string Type => "E107";
    }
        
        
    //    : IECSContent
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
