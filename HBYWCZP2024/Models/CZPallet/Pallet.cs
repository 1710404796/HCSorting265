
using static Cognex.NET.CognexBCR;
using Cognex.NET;
using BCR.Common.Fs;
using System.Text.Json.Serialization;
using HBYWCZP2024.Models.ECS;

namespace HBYWCZP2024.Models.CZPallet
{
    public class Pallet : XYLStation
    {
        public Pallet(int insideNum, string name) : base(insideNum, name)
        {
            ChannelNum = 24;
        }
        [JsonIgnore]
        public Option<Reader> Reader { get; set; } = None;
        public int PalletId { get; set; }
        public PalletType Type { get; set; }
        public TrayType OKNGType { get; set; }

        public ECSTrayType ECSCZTrayType { get; set; }

        public bool CheckCodeRegular(string? code = null)
        {
            code ??= PalletCode;
            
            if (code.Length < 2) return false;
            return MainStorage.Saves.PalletTypeRules.Any(s => s.CheckOk(code));
        }


    }

    //public enum OKNGPalletType { 
    //    OK,
    //    NG,
    //    其他
    //}
    public enum PalletType
    {
        拆盘,
        组盘,
        筛选,
        异常口,
        假电芯盘,
    }
}