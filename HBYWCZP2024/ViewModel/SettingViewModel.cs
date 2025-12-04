using CommunityToolkit.Mvvm.ComponentModel;
using HBYWCZP2024.Models;
using HBYWCZP2024.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HBYWCZP2024.ViewModel
{
    public partial class SettingViewModel(MainViewModel mainViewModel) : ObservableRecipient
    {
        public WcsSaves Saves => MainStorage.Saves;

        public List<string> Stations => MainStorage.Saves.KeysSave.Values.Select(s => s.Name).ToList();
        
        [ObservableProperty]
        bool _canEditing = true;
        
        public bool IsBackGround
        {
            get => Saves.BackGroundEnable == System.Windows.Visibility.Visible;
            set
            {
                Saves.BackGroundEnable = value ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                mainViewModel.HasBackGround = Saves.BackGroundEnable;
            }
        }

        /// <summary>
        /// 设备类型列表，用于ComboBox绑定
        /// </summary>
        public List<KeyValuePair<EquipmentType, string>> EquipmentTypes { get; } = new()
        {
            //new(EquipmentType.OCV, "OCV - 开路电压测试"),
            //new(EquipmentType.FMT, "FMT - 化成"),
            //new(EquipmentType.DCIR, "DCIR - 直流内阻测试"),
            new(EquipmentType.SEL, "SEL - 筛选"),
            //new(EquipmentType.CV, "CV - 物流线"),
            //new(EquipmentType.STACKER, "STACKER - 堆垛机"),
            //new(EquipmentType.BCR, "BCR - 扫码轮"),
            new(EquipmentType.AEB, "AEB - 组盘机"),
            new(EquipmentType.DAEB, "DAEB - 拆盘机"),
            //new(EquipmentType.LIQUID, "LIQUID - 注液机"),
            //new(EquipmentType.LAMINAT, "LAMINAT - 包膜机"),
            //new(EquipmentType.NAIL, "NAIL - 充气打扣机"),
            //new(EquipmentType.CLEAR, "CLEAR - 清洗机"),
            //new(EquipmentType.HELIUM, "HELIUM - 后气检机"),
            //new(EquipmentType.SEAL, "SEAL - 密封打扣机"),
            new(EquipmentType.CEB, "CEB - 换盘机"),
            new(EquipmentType.OTHER,"OTHER - 分档机")
        };
    }
}
