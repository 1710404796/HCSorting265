using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.ECS
{
    public class S000 : IECSContent
    {
        public string Type => "S000";

        public string GetContent()
        {
            return string.Empty;
        }

        public void Parse(string content)
        {
        }
    }
}
