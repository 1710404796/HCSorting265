using HCSortingCZP.Models;

namespace HCSortingCZP.Models.CZPallet
{
    /// <summary>
    /// 托杯工位
    /// </summary>
    public class TrayCupStation : XYLStation
    {
        public TrayCupStation(int insideNum, string name) : base(insideNum, name)
        {
        }

        public TrayCupStation()
        {
        }
    }
}
