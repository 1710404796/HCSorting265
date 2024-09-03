using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS;

public class E100 : IECSContent
{
    public ResetOption ResetOption { get; set; }

    public string Type => "E100";

    public string GetContent()
    {
        return $"{(int)ResetOption}";
    }

    public void Parse(string content)
    {
        if (content.Length != 1)
        {
            throw new ArgumentException("字符串长度必须为1!");
        }
        ResetOption = (ResetOption)(content[0] - '0');
    }
}

public enum ResetOption
{
    WaitForCommand = 0,
    StartWorkAfterReset = 1
}
