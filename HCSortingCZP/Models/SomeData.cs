using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCSortingCZP.Models
{
    public record CupBatteryModel(
        string CupCode,
        string BatteryCode,
        int Channel
        );

    public record CupTray(string TrayCode,
        List<CupBatteryModel> CupBatteryModels
        );

    internal class SomeData
    {
        public static List<(string, string)> TestData { get; set; } = [

            ("431905143",   "0ZZCE11431A00SG230000162"),
("431130204",   "0ZZCE11431B00SG1X0000162"),
("431902157",   "0ZZCE11431B00SG1X0000292"),
("431901766",   "0ZZCE11431B00SG1W0001137"),
("431901201",   "0ZZCE11431B00SG1X0000257"),
("431902353",   "0ZZCE11431A00SG1X0000323"),
("431910309",   "0ZZCE11431B00SG1W0000624"),
("431903674",   "0ZZCE11431A00SG230000155"),
("431908781",   "0ZZCE11431B00SG1W0001034"),
("431131081",   "0ZZCE11431B00SG1W0001141"),
("431901650",   "0ZZCE11431A00SG230000091"),
("431902167",   "0ZZCE11431B00SG1X0000024"),
("431901761",   "0ZZCE11431B00SG1X0000214"),
("431901221",   "0ZZCE11431A00SG230000167"),
("431901212",   "0ZZCE11431B00SG1X0000324"),];

        public static List<CupBatteryModel> CupBatteries { get; set; } = TestData.Select((x, i) => new CupBatteryModel(x.Item1, x.Item2, i % 196)).ToList();

        public static CupTray TestCupTray = new("43S100011", CupBatteries.Take(196).ToList());


    
    }
}
