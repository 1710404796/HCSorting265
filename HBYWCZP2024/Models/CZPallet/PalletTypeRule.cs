using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HBYWCZP2024.Models.CZPallet
{
    // 托盘类型及规则
    // 需要管理电池类
    public partial class PalletTypeRule : ObservableObject
    {
        public string PalletRule { get; set; }
        public System.Collections.Generic.HashSet<BatteryTypeRule> Rules { get; set; } = new();// 好像是前13位
        /// <summary>
        /// 规则名字
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public string FakeBatteryCode { get; set; } = string.Empty;
        /// <summary>
        /// 电池数量
        /// </summary>
        public int BatteryNum { get; set; } = 24;
        public PalletTypeRule(string name, string palletRule, string fakeBatteryCode, int channelNum)
        {
            Name = name;
            PalletRule = palletRule;
            FakeBatteryCode = fakeBatteryCode;
            BatteryNum = channelNum;
        }

        public PalletTypeRule()
        {
            //BatteryNum = channelNum;
        }
        public void AddRule(BatteryTypeRule rule)
        {
            Rules.Add(rule);
        }

        public bool CheckBatteryCodeRule(string code)
        {
            return Rules.Any(s => s.CheckOk(code));
        }

        public bool CheckOk(string code)
        {
            return Regex.IsMatch(code, PalletRule);
        }
        /// <summary>
        /// 通过条码获取托盘种类
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static PalletTypeRule? GetRuleFromCode(string code)
        {
            return MainStorage.Saves.PalletTypeRules.FirstOrDefault(s => s.CheckBatteryCodeRule(code));
            //return ;
        }
    }
}
