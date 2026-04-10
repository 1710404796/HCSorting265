using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media;

namespace HBYWCZP2024.Models
{
    /// <summary>
    /// 电池状态
    /// </summary>
    public enum BatteryLevel
    {
        None = 0,
        OK = 1,
        NG = 2,
        Fake = 3,
        REWORK = 4,
        EMPTY = 5,
        A = 7,
        B = 8,
        C = 9,
        D = 10,
        E = 11,
        F = 12,
        Mark = 13,
        V = 14,
        Asub = 15,
        Bsub = 16,
        待判定 = 17,
        复测2 = 待判定,
        待复测 = 18,
        复测1 = 待复测,
        Unknown = 999,
        V1 = 1000,
        NG2 = 1001,
    }
    public partial class StationChannel : ObservableObject
    {
        private static string V = "JJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJJ"[..24];

        /// <summary>
        /// 条码默认值 避免range出错 默认是30个空格
        /// </summary>
        public readonly static string DefaultCode = string.Empty;

        public string ContainerFrom { get; set; } = string.Empty;
        public int StationFrom { get; set; } = 0;
        public int ChannelFrom { get; set; } = 0;

        /// <summary>
        /// 电池等级
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BorderBrush))]
        BatteryLevel _batteryLevel = BatteryLevel.EMPTY;


        /// <summary>
        /// 通道条码
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(BorderBrush))]
        string _code = DefaultCode;

        /// <summary>
        /// 通道号
        /// </summary>
        [JsonIgnore]
        public int Location { get; set; }

        [JsonIgnore]
        public Brush BorderBrush => GetBrush();

        public string GetFakeCode()
        {
            return $"{Random.Shared.Next(10000000):00000000}{Random.Shared.Next(10000000):00000000}FAKEFAKE";
        }

        public byte GetPlcCode()
        {
            if (BatteryLevel == BatteryLevel.None || Code == DefaultCode) return (byte)BatteryLevel.EMPTY;

            return (byte)BatteryLevel;
        }

        /// <summary>
        /// 获取等级颜色
        /// </summary>
        /// <returns></returns>
        public int GetCellType()
        {
            return BatteryLevel switch
            {
                BatteryLevel.None => 0,
                BatteryLevel.EMPTY => 0,
                BatteryLevel.OK => 1,
                BatteryLevel.NG => 2,
                BatteryLevel.Fake => 3,
                _ => 1,
            };
        }
        public Brush GetBrush()
        {
            Brush colorDefault = (Brush)Application.Current.Resources["SystemControlPageTextBaseHighBrush"];
            if (Code == DefaultCode)
            {
                return colorDefault;
            }
            switch (BatteryLevel)
            {
                case BatteryLevel.None:
                    break;
                case BatteryLevel.OK:
                    return Brushes.LightGreen;
                case BatteryLevel.NG:
                    return Brushes.Red;
                case BatteryLevel.Fake:
                    return Brushes.Purple;
                case BatteryLevel.REWORK:
                    return Brushes.Blue;
                case BatteryLevel.EMPTY:
                    return colorDefault;

                default:
                    break;
            }
            return colorDefault;
        }
        public static string FakeCode => V;
        public void Reset(bool isFake)
        {
            if (isFake)
            {
                Code = FakeCode;
                BatteryLevel = BatteryLevel.Fake;
                ContainerFrom = string.Empty;
                StationFrom = 0;
                ChannelFrom = 0;
            }
            else
            {
                Reset();
            }
        }
        public void Reset()
        {
            Code = DefaultCode;
            BatteryLevel = BatteryLevel.EMPTY;
            ContainerFrom = string.Empty;
            StationFrom = 0;
            ChannelFrom = 0;
        }
    }
}