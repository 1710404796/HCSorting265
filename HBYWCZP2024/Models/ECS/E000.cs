using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{
    internal class E000 : IECSContentRes
    {
        public string ErrorCode => "0000";

        public string Type => "E000";

        public string GetContent()
        {
            return string.Empty;
        }

        public void Parse(string content)
        {
        }
    }
}
