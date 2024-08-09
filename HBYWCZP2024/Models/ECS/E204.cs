using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{
    public class E204 : IECSContent
    {
        public WorkStatus WorkStatus { get; set; }

        public string GetContent()
        {
            return WorkStatus.ToString();
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
                    WorkStatus = WorkStatus.Ended;
                    break;
                case 'P':
                    WorkStatus = WorkStatus.Paused;
                    break;
                default:
                    throw new ArgumentException($"未知的选项: {option}");
            }
        }
    }

    public enum WorkStatus
    {
        Ended = 'E',
        Paused = 'P'
    }
}
