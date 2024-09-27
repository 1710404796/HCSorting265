using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;

namespace HBYWCZP2024.Models.ECS;

//    4	内容	3	"1     R:Running  I:Idle  T:Trouble  P:Power on                   
//       O:Power off （设备状态）
//1     0:手动模式    1:自动模式（设备模式）
//1     0:无作业对象 1:有作业对象（有无托盘或  
//       电芯）"

/// <summary>
/// 向ECS上报设备状态
/// </summary>
public class S101 : IECSContent
{
    public new string Type => "S101";

    public DeviceStatus DeviceStatus { get; set; }
    public DeviceMode DeviceMode { get; set; }

    public JobObjectStatus JobObjectStatus { get; set; }
    public string GetContent()
    {
        return $"{DeviceStatus.Humanize()}{(int)DeviceMode}{(int)JobObjectStatus}";
    }

    public void Parse(string content)
    {
        if (content.Length > 3)
        {
            throw new ArgumentException("字符串长度不足!");
        }
        DeviceStatus = content[0].ToString().DehumanizeTo<DeviceStatus>();
        DeviceMode = (DeviceMode)(content[1] - '0');
        JobObjectStatus = (JobObjectStatus)(content[2] - '0');
    }
}

public enum DeviceStatus
{
    [Description("R")]
    Running,
    [Description("I")]
    Idle,
    [Description("T")]
    Trouble,
    [Description("P")]
    PowerOn,
    [Description("O")]
    PowerOff
}

public enum DeviceMode
{
    Manual = 0,
    Auto = 1
}

public enum JobObjectStatus
{
    NoJobObject = 0,
    HasJobObject = 1
}
