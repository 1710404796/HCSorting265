using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Data.Models

{
    public class WcsLog
    {
        public int Id { get; set; }

        public string ApiName { get; set; } = string.Empty;
        public DateTime RequestTime { get; set; } = DateTime.Now;
        public DateTime ResponseTime { get; set; }

        public string Request { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        [DefaultValue(true)]
        public bool IsSuccess { get; set; } = true;
    }
}
