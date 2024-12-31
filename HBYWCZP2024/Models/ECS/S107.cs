using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS;
/// <summary>
/// 托盘离开
/// 当托盘离开前进行上报
/// </summary>
public class S107 : S102
{
    //public new string Type => "S107";
    public S107()
    {
        Type = "S107";
    }
    //public int TrayPosition { get; set; }
    //public TrayType TrayType { get; set; }
    //public string TrayNumber { get; set; }

    //public string GetContent()
    //{
    //    return $"{TrayPosition}{(int)TrayType}{TrayNumber.PadRight(20)}";
    //}

    //public void Parse(string content)
    //{
    //    if (content.Length != 22)
    //    {
    //        throw new ArgumentException("字符串长度不足!");
    //    }

    //    TrayPosition = int.Parse(content.Substring(0, 1));
    //    TrayType = (TrayType)(content[1] - '0');
    //    TrayNumber = content.Substring(2, 20).Trim();
    //}
}

public enum TrayType
{
    Engineering = 0,
    NG = 1,
    Other = 2
}
