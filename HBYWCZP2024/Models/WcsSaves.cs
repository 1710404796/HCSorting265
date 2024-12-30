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
        public string OpcIP => KeysSave[SelectStageName].OpcIP;

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
        public bool TestMode => KeysSave[SelectStageName].TestMode;


        [JsonIgnore]
        public List<PalletTypeRule> PalletTypeRules => KeysSave[SelectStageName].PalletTypeRules;


        [JsonIgnore]
        public string ECSIP
        {
            get => KeysSave[SelectStageName].ECSIP;
            set => KeysSave[SelectStageName].ECSIP = value;
        }

        [JsonIgnore]
        public ushort ECSPort
        {
            get => KeysSave[SelectStageName].ECSPort;
            set => KeysSave[SelectStageName].ECSPort = value;
        }

        [JsonIgnore]
        public string DeviceCode
        {
            get => KeysSave[SelectStageName].DeviceCode;
            set => KeysSave[SelectStageName].DeviceCode = value;
        }


        [JsonIgnore]
        public OpcPLCSetting PLCPSetting => KeysSave[SelectStageName].PLCPSetting;
        public Dictionary<PowerEnum, Dictionary<RoleEnum, bool>> PowerTable { get; set; } = [];
        [JsonIgnore]

        public Dictionary<string, string> OpcTable
        {
            get
            {
                return KeysSave[SelectStageName].OpcTable;
            }
            set
            {
                KeysSave[SelectStageName].OpcTable = value;
            }
        }

        public Dictionary<string, StageMono> KeysSave { get; set; } = [];
        [JsonIgnore]
        public OKNGCnt OKNGCnt
        {
            get => KeysSave[SelectStageName].OKNGCnt;
            set => KeysSave[SelectStageName].OKNGCnt = value;
        }

        [JsonIgnore]
        public string MESIP
        {
            get => KeysSave[SelectStageName].MESIP;
            set => KeysSave[SelectStageName].MESIP = value;
        }

        [JsonIgnore]
        public string SiteCode
        {
            get => KeysSave[SelectStageName].SiteCode;
            set => KeysSave[SelectStageName].SiteCode = value;
        }

        [JsonIgnore]
        public string LineCode
        {
            get => KeysSave[SelectStageName].LineCode;
            set => KeysSave[SelectStageName].LineCode = value;
        }

        [JsonIgnore]
        public string EquipNum
        {
            get => KeysSave[SelectStageName].EquipNum;
            set => KeysSave[SelectStageName].EquipNum = value;
        }

        [JsonIgnore]
        public string MaterialCode
        {
            get => KeysSave[SelectStageName].MaterialCode;
            set => KeysSave[SelectStageName].MaterialCode = value;
        }

        [JsonIgnore]
        public string ProductType
        {
            get => KeysSave[SelectStageName].ProductType;
            set => KeysSave[SelectStageName].ProductType = value;
        }

        [JsonIgnore]
        public ApiEnable ApiEnableTable
        {
            get => KeysSave[SelectStageName].ApiEnableTable;
            set => KeysSave[SelectStageName].ApiEnableTable = value;
        }
        [JsonIgnore]
        public bool ResetTable
        {
            get => KeysSave[SelectStageName].ResetTable;
            set => KeysSave[SelectStageName].ResetTable = value;
        }
    }

    public class ApiEnable
    {
        public bool CheckPalletMaterialEnable
        {
            get; set;
        } = true;
        public bool GrabActionInsertEnable
        {
            get; set;
        } = true;

        public bool GrabActionRemoveEnable
        {
            get; set;
        } = true;

        public bool ApplyMaterialEnable
        {
            get; set;
        } = true;
        public bool PalletArrivedEnable { get; set; }
        public bool PalletOutEnable { get; set; }
        public bool CheckMaterialEnable { get;  set; }
    }

    public class StageMono
    {
        public OKNGCnt OKNGCnt { get; set; } = new();
        public Dictionary<PowerEnum, Dictionary<RoleEnum, bool>> PowerTable { get; set; } = [];
        public Dictionary<string, string> OpcTable { get; set; } = [];

        public string ECSIP { get; set; } = "127.0.0.1";
        public ushort ECSPort { get; set; } = 50005;
        public string Name { get; set; } = "Default";
        public XYLStage Stage { get; set; } = new();
        public OpcPLCSetting PLCPSetting { get; set; } = new();

        public List<PalletTypeRule> PalletTypeRules { get; set; } = [];

        public string DeviceCode { get; set; } = string.Empty;
        public ApiEnable ApiEnableTable { get; set; } = new();
        public bool TestMode
        {
            get; set;
        }

        public string OpcIP
        {
            get; set;
        } = "127.0.0.1";

        public string MESIP
        {
            get;
            set;
        } = "127.0.0.1";

        public string SiteCode
        {
            get;
            set;
        } = string.Empty;
        public string LineCode
        {
            get;
            set;
        } = string.Empty;
        public string EquipNum
        {
            get;
            set;
        } = string.Empty;
        public string MaterialCode
        {
            get;
            set;
        } = string.Empty;
        public string ProductType
        {
            get;
            set;
        } = string.Empty;
        public bool ResetTable { get; set; }
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
