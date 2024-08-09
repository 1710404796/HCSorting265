using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Models.CZPallet;
using LinxUniverse.PLC;
using LinxUniverse.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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

        public string SelectStageName { get; set; } = string.Empty;

        public string Port
        {
            get; set;
        } = "50005";
        public Visibility BackGroundEnable
        {
            get;
            set;
        }
        [JsonIgnore]
        public XYLStage Stage => KeysSave[SelectStageName].Stage;
        public bool TwoOk { get; set; } = false;
        //public PLCCpuType PLCCpuType { get; set; } = PLCCpuType.小端;


        public int LogDeleteDay { get; set; } = 30;
        [JsonIgnore]

        public List<PalletTypeRule> PalletTypeRules => KeysSave[SelectStageName].PalletTypeRules;
        public bool TestMode { get; set; }
        [JsonIgnore]

        public PLCSetting PLCPSetting => KeysSave[SelectStageName].PLCPSetting;
        public Dictionary<PowerEnum, Dictionary<RoleEnum, bool>> PowerTable { get; set; } = [];
        public string ECSIP { get; set; } = "127.0.0.1";
        public ushort ECSPort { get; set; } = 50005;
        public string DeviceCode { get; set; }


        public Dictionary<string, StageMono> KeysSave { get; set; } = [];
    }

    public class StageMono
    {
        public XYLStage Stage { get; set; } = new();
        public PLCSetting PLCPSetting { get; set; } = new();

        public List<PalletTypeRule> PalletTypeRules { get; set; } = [];

    }
}
