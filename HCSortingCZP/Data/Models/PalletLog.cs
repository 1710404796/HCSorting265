using HCSortingCZP.Models;
using HCSortingCZP.Models.CZPallet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HCSortingCZP.Data.Models
{
    //public class ChaiPalletLog
    //{
    //    public int Id
    //    {
    //        get; set;
    //    }

    //    [Column(TypeName = "nvarchar(255)")]
    //    public string PalletCode { get; set; } = string.Empty;
    //    public DateTime ChaiPanTime { get; set; } = DateTime.Now;
    //}
    public class PalletLog
    {
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        public string PalletCode { get; set; } = string.Empty;

        public List<BatteryInfo> BatteryInfo { get; set; } = [];
        [NotMapped]
        public int ChannelCount => BatteryInfo.Count;

        public DateTime ZuPanTime { get; set; } = DateTime.Now;
        public int Column { get; set; }
        public PalletType PalletType
        {
            get; set;
        } = PalletType.组盘;
    }

    public class BatteryInfo
    {
        public BatteryLevel BatteryLevel { get; set; }
        public string BatteryCode { get; set; } = string.Empty;
    }
}
