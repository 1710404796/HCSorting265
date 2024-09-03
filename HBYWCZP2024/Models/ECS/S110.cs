using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{
    public class S110 : IECSContent
    {
        public new string Type => "S110";
        public string ErrorCode { get; set; } = string.Empty;

        public string GetContent()
        {
            return $"{ErrorCode}";
        }

        public void Parse(string content)
        {
            if (content.Length != 4)
            {
                throw new ArgumentException("字符串长度必须为4!");
            }
            ErrorCode = content.Substring(0, 4);
        }
    }
}
