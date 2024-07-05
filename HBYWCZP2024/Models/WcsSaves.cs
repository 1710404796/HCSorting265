using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Models.CZPallet;
using LinxUniverse.PLC;
using LinxUniverse.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HBYWCZP2024.Models
{
    public class WcsSaves() : SaveBase("HBYWCZP2024")
    {
        public string IP
        {
            get; set;
        } = "127.0.0.1";

        public string Port
        {
            get; set;
        } = "50005";
        public Visibility BackGroundEnable
        {
            get;
            set;
        }

        public XYLStage Stage { get; set; } = new();
        public bool TwoOk { get; set; } = false;
        //public PLCCpuType PLCCpuType { get; set; } = PLCCpuType.小端;


        public int LogDeleteDay { get; set; } = 30;

        public List<PalletTypeRule> PalletTypeRules { get; set; } = [];
        public bool TestMode { get; set; }

        public List<PLCSetting> PLCPSetting { get; set; } = [];
        public Dictionary<PowerEnum, Dictionary<RoleEnum, bool>> PowerTable { get; set; } = [];
    }
}
