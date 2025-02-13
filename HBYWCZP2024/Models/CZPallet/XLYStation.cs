using CommunityToolkit.Mvvm.ComponentModel;
using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models
{
    /// <summary>
    /// 工位
    /// </summary>
    public partial class XYLStation : ObservableObject
    {
        public string ReaderIpAddr { get; set; } = string.Empty;


        public bool GetFromServer { get; set; } = false;
        public string ServerIp { get; set; } = "127.0.0.1:50005";


        public XYLStation(int insideNum, string name)
        {
            //_mediator = App.GetService<IMediator>();
            // 想想会不会被覆盖
            OutsideStationNum = InsideNum = insideNum;
            Name = name;
            int idx = 0;

            foreach (var channel in Channels)
            {
                channel.Location = idx++;
            }
        }
        public XYLStation()
        {
            int idx = 0;
            foreach (var channel in Channels)
            {
                channel.Location = idx++;
            }
        }

        /// <summary>
        /// 屏蔽扫码器
        /// </summary>
        [JsonIgnore]

        public bool BanBarcodeReader { get; set; } = false;
        /// <summary> 
        /// 内部调度号
        /// </summary>
        public int InsideNum { get; set; }

        /// <summary>
        /// 托盘通道数
        /// </summary>
        public int ChannelNum { get; set; } = 2;
        /// <summary>
        /// 托盘数量
        /// </summary>
        public int Column { get; set; } = 1;


        public bool IsFake => OutsideStationNum >= 10000 && OutsideStationNum < 11000;

        public ObservableCollection<StationChannel> Channels { get; set; } = new ObservableCollection<StationChannel>(
            Enumerable.Range(0, 256).Select(s => new StationChannel() { Location = s })
            );

        [ObservableProperty]
        bool _isEnable;

        /// <summary>
        /// 工位号
        /// </summary>

        [ObservableProperty]
        int _outsideStationNum;

        [ObservableProperty]
        string _name = string.Empty;

        /// <summary>
        /// 托盘条码
        /// </summary>
        [ObservableProperty]
        string _palletCode = "";
        [JsonIgnore]
        Lazy<IMediator> _lazyMediator = new Lazy<IMediator>(App.GetService<IMediator>);
        [JsonIgnore]

        IMediator _mediator => _lazyMediator.Value;
        private bool CheckCodeLength(string code)
        {
            if (code.Length != 24)
            {

                //OnWarnning?.Invoke(this, $"电池条码长度不为24！长度为{code.Length}");
                return false;
            }
            return true;
        }
        public async ValueTask<bool> BindBattery(int id, string code, bool force = false, BatteryLevel batteryLevel = BatteryLevel.OK)
        {
            // Todo: 确认这一步需不需要请求fts， 或是在其他位置请求
            if (!CheckCodeLength(code) && !force) return false;
            if (Channels[id].Code != StationChannel.DefaultCode && !force)
            {
                //OnNG?.Invoke(this, "该位置已经被占用！绑定失败");
                // 不为空 不可绑定

                return false;
            }
            else
            {
                return await SetChannelCode(id, code, force, batteryLevel);
            }
        }

        private async ValueTask<bool> SetChannelCode(int id, string code, bool force = false, BatteryLevel batteryLevel = BatteryLevel.OK)
        {
            if (CheckBatteryUnique(code) || force)
            {
                // 目前只需校验当前的重复性 无需管理其他托盘
                if (CheckBatterySerial(code) || force)
                {
                    await SetChanelStage(id, code, batteryLevel);
                    //OnSuccess?.Invoke(this, "绑定成功!");
                    return true;
                }
                else
                {
                    //OnNG?.Invoke(this, "电芯码前13位不一样!");
                    await _mediator.Send(new WarningBoxCommand("电芯码前13位不一样"));
                    return false;
                }
            }
            else
            {
                //OnNG?.Invoke(this, "有相同的电池");
                await _mediator.Send(new WarningBoxCommand("有相同的电池"));

                return false;
            }
        }
        private async ValueTask SetChanelStage(int id, string code, BatteryLevel batteryLevel)
        {
            if (!string.IsNullOrWhiteSpace(ServerIp) && GetFromServer)
            {
                await _mediator.Send(new PushDataToStationCommand(OutsideStationNum, [new Channel(id + 1, new BatteryWithLevel(code, batteryLevel))], true));
            }
            else
            {
                Channels[id].Code = code;
                Channels[id].BatteryLevel = batteryLevel;
                if (code == StationChannel.DefaultCode || string.IsNullOrWhiteSpace(code))
                {
                    Channels[id].BatteryLevel = BatteryLevel.EMPTY;
                }
            }

            
        }

        /// <summary>
        /// 手动更新电芯
        /// </summary>
        /// <param name="id"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public async ValueTask<bool> UpdateBattery(int id, string code, bool force = false, BatteryLevel batteryLevel = BatteryLevel.OK)
        {
            if (code.Length == 0)
            {
                UnbindBattery(id);
                return true;
            }
            if (!CheckCodeLength(code) && !force) return false;

            return await SetChannelCode(id, code, force, batteryLevel);

            //if (CheckBatteryUnique(code))
            //{
            //    // 目前只需校验当前的重复性 无需管理其他托盘
            //    OnSuccess?.Invoke(this, "绑定成功!");
            //    return true;
            //}
            //else
            //{
            //    OnNG?.Invoke(this, "有相同的电池");
            //    return false;
            //}
        }

        public bool UnbindBattery(int id)
        {
            if (Channels[id].Code != StationChannel.DefaultCode)
            {
                if (!string.IsNullOrWhiteSpace(ServerIp) && GetFromServer)
                {
                     _mediator.Send(new PushDataToStationCommand(OutsideStationNum, [new Channel(id + 1, None)], false));
                    return true;
                }
                else
                {
                    Channels[id].Reset(IsFake);
                    return true;
                }
                
            }
            else
            {
                //OnNG?.Invoke(this, "该位置已经啥也没有！解绑失败");
                return false;
            }
        }
        /// <summary>
        /// 检查是否电池是否是一个系列的 (前13位)
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool CheckBatterySerial(string code)
        {
            // log 出现不唯一
            var activeChannel = Channels.FirstOrDefault(s => s.Code != StationChannel.DefaultCode);
            // 如果是第一个 则必定通过
            if (activeChannel == null) return true;

            // 否则检查前13位是否相同
            return code.Length >= 13 && activeChannel.Code.Length >= 13 && activeChannel.Code[..13] == code[..13];

        }
        /// <summary>
        /// 检查是否电池不唯一 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool CheckBatteryUnique(string code)
        {
            // log 出现不唯一
            string checkcode = code;
            return !Channels.Any(s => s.Code == checkcode);

        }
        /// <summary>
        /// 返回有条码的电芯数
        /// </summary>
        /// <returns></returns>
        public int GetActivecnt()
        {
            return Channels.Count(s => s.Code != StationChannel.DefaultCode);
        }
        public void ClearStage()
        {
            PalletCode = string.Empty;
            foreach (var channel in Channels)
            {
                channel.Reset(IsFake);
            }
        }

    }
}
