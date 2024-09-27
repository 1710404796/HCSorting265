using CommunityToolkit.Mvvm.ComponentModel;
using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Models.CZPallet;
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
    [ObservableObject]
    public partial class WcsSaves() : SaveBase("HBYWCZP2024")
    {
        public string OpcIP
        {
            get; set;
        } = "127.0.0.1";

        //public string SelectStageName { get; set; } = string.Empty;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Stage))]
        [NotifyPropertyChangedFor(nameof(PLCPSetting))]
        [NotifyPropertyChangedFor(nameof(PalletTypeRules))]
        string _selectStageName = string.Empty;

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

        public OpcPLCSetting PLCPSetting => KeysSave[SelectStageName].PLCPSetting;
        public Dictionary<PowerEnum, Dictionary<RoleEnum, bool>> PowerTable { get; set; } = [];
        public string ECSIP { get; set; } = "127.0.0.1";
        public ushort ECSPort { get; set; } = 50005;
        [JsonIgnore]

        public string DeviceCode { get => KeysSave[SelectStageName].DeviceCode; set => KeysSave[SelectStageName].DeviceCode = value; }


        public Dictionary<string, StageMono> KeysSave { get; set; } = [];
        public Dictionary<string, string> OpcTable { get; set; } = [];
        public OKNGCnt OKNGCnt { get; internal set; }
    }

    public class StageMono
    {
        public string Name { get; set; } = "Default";
        public XYLStage Stage { get; set; } = new();
        public OpcPLCSetting PLCPSetting { get; set; } = new();

        public List<PalletTypeRule> PalletTypeRules { get; set; } = [];

        public string DeviceCode { get; set; } = string.Empty;

    }

    public class OKNGCnt
    {
        // 想办法通知一下
        public int Id { get; set; }

        public int OKCnt { get; set; }
        /// <summary>
        /// 扫码NG
        /// </summary>
        public int NG1Cnt { get; set; }

        /// <summary>
        /// 校验NG
        /// </summary>
        public int NG2Cnt { get; set; }

        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; }

        public void AddOk(int value)
        {
            OKCnt += value;
            //App.GetService<DataDisplayViewModel>().Update();
        }

        public void AddNG1(int value)
        {
            NG1Cnt += value;
            //App.GetService<DataDisplayViewModel>().Update();
        }

        public void AddNG2(int value)
        {
            NG2Cnt += value;
            //App.GetService<DataDisplayViewModel>().Update();
        }
    }
}
