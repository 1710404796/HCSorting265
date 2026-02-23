using CommunityToolkit.Mvvm.ComponentModel;
using HBYWCZP2024.Attritubes;
using HBYWCZP2024.Models.CZPallet;
using HBYWCZP2024.Service;
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
        [NotifyPropertyChangedFor(nameof(TestMesContainerCode))]
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

        public List<string[]> BatteryLevelList { get; set; } = Enumerable.Range(0,7).Select(s => new string[0]).ToList();
        public  string ExDbConnectionString { get; set; } = string.Empty;
        public  string ExDbTableName { get; set; } = string.Empty;

        public int LogDeleteDay { get; set; } =30;
        public bool TestMode => KeysSave[SelectStageName].TestMode;


        [JsonIgnore]
        public List<PalletTypeRule> PalletTypeRules => KeysSave[SelectStageName].PalletTypeRules;


        [JsonIgnore]
        public string ECSUrl
        {
            get => KeysSave[SelectStageName].ECSUrl;
            set => KeysSave[SelectStageName].ECSUrl = value;
        }


        [JsonIgnore]
        public string FMSUrl
        {
            get => KeysSave[SelectStageName].FMSUrl;
            set => KeysSave[SelectStageName].FMSUrl = value;
        }

        //[JsonIgnore]
        //public ushort ECSPort
        //{
        //    get => KeysSave[SelectStageName].ECSPort;
        //    set => KeysSave[SelectStageName].ECSPort = value;
        //}

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
        public string StepCode
        {
            get => KeysSave[SelectStageName].StepCode;
            set => KeysSave[SelectStageName].StepCode = value;
        }

        [JsonIgnore]
        public string UserName
        {
            get => KeysSave[SelectStageName].UserName;
            set => KeysSave[SelectStageName].UserName = value;
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
        [JsonIgnore]

        public string ProductModelCode
        {
            get => KeysSave[SelectStageName].ProductModelCode;
            set => KeysSave[SelectStageName].ProductModelCode = value;
        }
        [JsonIgnore]

        public string FilterSpecialNum
        {
            get => KeysSave[SelectStageName].FilterSpecialNum;
            set => KeysSave[SelectStageName].FilterSpecialNum = value;
        }

        [JsonIgnore]
        public EquipmentType EquipType
        {
            get => KeysSave[SelectStageName].EquipType;
            set => KeysSave[SelectStageName].EquipType = value;
        }
        [JsonIgnore]
        public bool ExDb
        {
            get => KeysSave[SelectStageName].ExDb;
            set => KeysSave[SelectStageName].ExDb = value;
        }
        [JsonIgnore]
        public string ContainerTypeCode
        {
            get => KeysSave[SelectStageName].ContainerTypeCode;
            set => KeysSave[SelectStageName].ContainerTypeCode = value;
        }

        [JsonIgnore]
        public string TestMesContainerCode
        {
            get => KeysSave[SelectStageName].TestMesContainerCode;
            set => KeysSave[SelectStageName].TestMesContainerCode = value;
        }
        public int HeartbeartDuring { get; set; } =5000;
        public byte MarkCode { get; set; } =1;
        public byte NgCode { get; set; } =2;

        //[JsonIgnore]
        public List<StationLevelMapItem> StationLevelMap { get; set; } = new();
        //{
        //    get => KeysSave[SelectStageName].StationLevelMap;
        //    set => KeysSave[SelectStageName].StationLevelMap = value;
        //}
    }

    public class ApiEnable
    {
        public bool CheckPalletMaterialEnable
        {
            get; set;
        } = true;

        /// <summary>
        /// 托杯绑定关系上报外部数据库使能
        /// </summary>
        public bool UploadCupBindingToDbEnable { get; set; } = true;

        /// <summary>
        /// 空托盘校验使能(MES: CheckNullTray)。用于组盘托盘到位时的空托盘验证。
        /// </summary>
        public bool CheckNullTrayEnable { get; set; } = true;
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

        /// <summary>
        /// 托盘手动查询接口(equipment/assembly/container/manual)使能(筛选)
        /// </summary>
        public bool ContainerManualEnable { get; set; }
        public bool PalletArrivedEnable { get; set; }
        public bool PalletOutEnable { get; set; }
        public bool CheckMaterialEnable { get; set; }
        public bool UploadFmsDataEnable { get; set; }
        public bool UploadMesDataEnable { get; set; }

        /// <summary>
        /// 组盘结束时，上报整盘电芯信息到MES(equipment/assembly/uploadPalletResult)
        /// </summary>
        public bool UploadMesMpEndEnable { get; set; }
        public bool HighTempEnable { get; set; }
        public bool HCInStationEnable { get; set; }
        /// <summary>
        /// 分选挡位查询接口
        /// </summary>
        public bool FXQueryEnable { get; set; }
        public bool GRegionFXQueryEnable { get; set; }
        public bool AllLevelUploadEnable { get; set; }
        public bool HHFilterQueryEnable { get; set; }
        /// <summary>
        /// 分选档位上传(分选)
        /// </summary>
        public bool TrayLevelUpdate { get; set; }
        public bool UploadFMSChaiHuanpanEnable { get; set; }
        public bool Mark2Enable { get; set; }
        /// <summary>
        /// 特殊筛选工位使能(筛选)
        /// </summary>
        public bool FilterSpecialEnable { get; set; }

        /// <summary>
        /// G区预分选特殊档位接口
        /// </summary>
        public bool GRegionCvRequestEnable { get; set; }

        /// <summary>
        /// NG站台上报FMS
        /// </summary>
        public bool NGStationUpload { get; set; }
    }
    public class LevelName
    {
        public string Name { get; set; }
        = string.Empty;
        public BatteryLevel BatteryLevel { get; set; }
    }
    public class StationLevelMapItem
    {
        public int Station { get; set; }
        public int Level { get; set; }
    }

    public class StageMono
    {
        public OKNGCnt OKNGCnt { get; set; } = new();
        public Dictionary<PowerEnum, Dictionary<RoleEnum, bool>> PowerTable { get; set; } = [];
        public Dictionary<string, string> OpcTable { get; set; } = [];


        // 读取EXDB的任务
        public bool ExDb { get; set; } = false;


        public string ECSUrl { get; set; } = "127.0.0.1";
        //public ushort ECSPort { get; set; } = 50005;

        public string FMSUrl { get; set; } = "127.0.0.1";
        //public ushort FMSPort { get; set; } = 50005;


        public string Name { get; set; } = "Default";
        public XYLStage Stage { get; set; } = new();
        public OpcPLCSetting PLCPSetting { get; set; } = new();

        public List<PalletTypeRule> PalletTypeRules { get; set; } = [];

        public string DeviceCode { get; set; } = string.Empty;

        public string ContainerTypeCode { get; set; } = string.Empty;

        /// <summary>
        /// SettingView里“测试MES托盘信息获取”使用的托盘号(测试用)。
        /// </summary>
        public string TestMesContainerCode { get; set; } = "43S100001";

        public ApiEnable ApiEnableTable { get; set; } = new();
        public bool TestMode
        {
            get; set;
        }

        public string OpcIP
        {
            get; set;
        } = "127.0.0.1";

        public string UserName
        {
            get; set;
        } = "test_hc";

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
        public EquipmentType EquipType
        {
            get;
            set;
        } = EquipmentType.AEB;
        public string StepCode
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
        public string ProductModelCode { get; set; } = string.Empty;

        public string FilterSpecialNum
        {
            get;
            set;
        } = string.Empty;

        public bool ResetTable { get; set; }

        public List<StationLevelMapItem> StationLevelMap { get; set; } = new();
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
