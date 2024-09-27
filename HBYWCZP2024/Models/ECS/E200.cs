using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS;
/// <summary>
/// 作业再开始
/// </summary>
public class E200 : IECSContent
{
    public new string Type => "E200";
    public OperationOption OperationOption { get; set; }

    public string GetContent()
    {
        return OperationOption.ToString();
    }

    public void Parse(string content)
    {
        if (content.Length != 1)
        {
            throw new ArgumentException("字符串长度必须为1!");
        }

        char option = content.ToUpper()[0];
        switch (option)
        {
            case 'E':
                OperationOption = OperationOption.ContinuousOperation;
                break;
            case 'P':
                OperationOption = OperationOption.RestartOperation;
                break;
            default:
                throw new ArgumentException($"未知的选项: {option}");
        }
    }
}

public enum OperationOption
{
    ContinuousOperation = 'E',
    RestartOperation = 'P'
}
