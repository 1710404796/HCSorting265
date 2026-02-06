using CommunityToolkit.Mvvm.ComponentModel;
using HBYWCZP2024.Models.CZPallet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HBYWCZP2024.Models
{
    public partial class XYLStage : ObservableObject
    {
        [ObservableProperty] // 可以考虑加入特性
        XYLStation _incomingBelt = new(100, "来料拉带");
        /// <summary>
        /// 去料拉带    
        /// </summary>
        [ObservableProperty]
        XYLStation _strippingBelt = new(200, "去料拉带");

        /// <summary>
        /// 机器人夹爪
        /// </summary>
        [ObservableProperty]
        XYLStation _robotGripper = new(300, "机器人夹爪");

        /// <summary>
        /// OK配对平台
        /// </summary>
        [ObservableProperty]
        XYLStation _okPairingPlatform = new(400, "OK配对平台");
        /// <summary>
        /// REWORK配对平台
        /// </summary>
        [ObservableProperty]
        XYLStation _reworkPairingPlatform = new(500, "REWORK配对平台");
        /// <summary>
        /// NG拉带
        /// </summary>
        [ObservableProperty]
        XYLStation _ngBelt = new(600, "NG拉带");
        /// <summary>
        /// 异常口
        /// </summary>
        [ObservableProperty]
        Pallet _abnormalStation = new(700, "异常口");

        [ObservableProperty]
        XYLStation _okPairingPlatform2 = new(800, "OK配对平台2");

        /// <summary>
        /// 托杯工位
        /// </summary>
        [ObservableProperty]
        TrayCupStation _trayCupStation = new(900, "托杯工位");

        [ObservableProperty]
        ObservableCollection<Pallet> _pallets = new();

        [JsonIgnore]
        public XYLStation[] Stations => [IncomingBelt, StrippingBelt, RobotGripper, OkPairingPlatform, ReworkPairingPlatform, NgBelt, AbnormalStation, OkPairingPlatform2, TrayCupStation, .. Pallets];

        public XYLStage()
        {
            for (int i = 1; i <= 16; i++)
            {
                Pallets.Add(new Pallet(i * 100 + 2000, $"托盘{i}")
                {
                    PalletId = i - 1,

                });
            }
            //Stations = 

            //[ IncomingBelt, StrippingBelt, RobotGripper, OkPairingPlatform, ReworkPairingPlatform, NgBelt, AbnormalStation, OkPairingPlatform2, .._pallets ];

        }

    }
}
