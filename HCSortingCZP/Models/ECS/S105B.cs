using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Models.ECS;
/// <summary>
/// 抓取电池结果上报
/// 机械手每抓一次电池都要进行上报
/// </summary>
//public class S105B : IECSContent
//{
//        public new string Type => "S105";
//    public int Position { get; set; }
//    public string TrayNumber { get; set; } = string.Empty;

//    public string GetContent()
//    {
//        return $"{Position,-3}{TrayNumber,-20}";
//    }

//    public void Parse(string content)
//    {
//        if (content.Length < 24)
//        {
//            throw new ArgumentException("字符串长度不足!");
//        }
//        Position = int.Parse(content.Substring(0, 3).Trim());


//        TrayNumber = content.Substring(3, 20).Trim();
//    }
//}
