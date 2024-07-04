using HBYWCZP2024.Models;
using HBYWCZP2024.Models.CZPallet;
using HBYWCZP2024.ViewModel;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Threading.Tasks;


namespace HBYWCZP2024.Utils
{
    public static class CZPUtils
    {
        public static Pallet? GetPalletByIndex(int idx)
        {
            if (idx < 0) return null;
            return MainStorage.Saves.Stage.Pallets.FirstOrDefault(p => p.PalletId == idx && p.IsEnable);
        }

        public static Pallet? GetPalletByInsideNum(int insideNum)
        {
            if (insideNum <= 0) return null;
            return MainStorage.Saves.Stage.Pallets.FirstOrDefault(p => p.InsideNum == insideNum && p.IsEnable);
        }

        public static Pallet? GetPalletByOutsideNum(int outsideNum)
        {
            if (outsideNum <= 0) return null;

            return MainStorage.Saves.Stage.Pallets.FirstOrDefault(p => p.OutsideStationNum == outsideNum && p.IsEnable);
        }


        public static XYLStation? GetXYLStationByInsideNum(int insideNum)
        {

            if (insideNum <= 0) return null;
            return MainStorage.Saves.Stage.Stations.FirstOrDefault(p => p.InsideNum == insideNum && p.IsEnable);
        }

        public static XYLStation? GetXYLStationByOutsideNum(int outsideNum)
        {
            if (outsideNum <= 0) return null;
            return MainStorage.Saves.Stage.Stations.FirstOrDefault(p => p.OutsideStationNum == outsideNum && p.IsEnable);
        }

        [JsonIgnore]
        static Lazy<ILogger> _lazyLogger= new Lazy<ILogger>(App.GetService<ILogger<MainViewModel>>);
        [JsonIgnore]

        static ILogger _logger => _lazyLogger.Value;

        public static bool MoveTo(XYLStation stationFrom, XYLStation stationTo, int channelFrom, int channelTo, string? checkCode = null)
        {
            if (stationFrom is null)
            {
                throw new ArgumentNullException(nameof(stationFrom));
            }

            if (stationTo is null)
            {
                throw new ArgumentNullException(nameof(stationTo));
            }
            //if (stationFrom == null)
            if (checkCode != null && checkCode != stationFrom.Channels[channelFrom].Code)
            {
                _logger.LogDebug($"电芯条码与本地对比！本地: {stationFrom.Channels[channelFrom].Code}; 传入: {checkCode}");
            }

            // 如果plc不上报条码 则以缓存为准
            if (string.IsNullOrWhiteSpace(checkCode)) { checkCode = stationFrom.Channels[channelFrom].Code; }

            // Todo: move这里 可能还需要检查规则
            //stationTo.Channels[channelTo].Code = stationFrom.Channels[channelTo].Code;
            stationTo.Channels[channelTo].Code = checkCode;
            stationTo.Channels[channelTo].BatteryLevel = stationFrom.Channels[channelFrom].BatteryLevel;
            if (stationTo.Name == "机器人夹爪")
            {
                stationTo.Channels[channelTo].ContainerFrom = stationFrom.PalletCode;
                stationTo.Channels[channelTo].StationFrom = stationFrom.OutsideStationNum;
                stationTo.Channels[channelTo].ChannelFrom = channelTo + 1;

            }
            if (checkCode != string.Empty && checkCode != null &&
                (stationTo.Channels[channelTo].BatteryLevel == BatteryLevel.None || stationTo.Channels[channelTo].BatteryLevel == BatteryLevel.EMPTY))
            {
                stationTo.Channels[channelTo].BatteryLevel = BatteryLevel.OK;
            }
            stationFrom.Channels[channelFrom].Reset();

            return true;
        }
    }

}
