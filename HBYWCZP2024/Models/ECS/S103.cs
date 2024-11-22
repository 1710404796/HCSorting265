using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{

    /// <summary>
    /// 请求NG位置信息 请求电池位置信息
    /// 作业开始前获取电芯位置
    /// </summary>'
    public class S103B : S103
    {
    
    }
    public class S103 : IECSContent
    {
        public string Type => "S103";
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
