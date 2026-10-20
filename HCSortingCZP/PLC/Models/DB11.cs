
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;

namespace HCSortingCZP.PLC.Models
{
   

    [OPCMember("WCS_TO_PLC_Tray")]
    public class DB11 : ITaskCodeFeedbackDB
    {
        public DB11()
        {
        }
        [OPCMember("Tray_Feedback")]
        public byte[] PalletCheck { get; set; } = new byte[16];



        [OPCMember("Tray")]

        public string[] PalletCode { get; set; } = new string[16];

        public byte ToFeedback => throw new NotImplementedException();

        public byte ToFeedbacks(int idx) => PalletCheck[idx];

        //public int GetFeedbacksOffset(int idx) => idx;

        string ITaskCodeFeedbackDB.GetFeedbacksOffset(int idx)
        {
            var baseName = GetType().GetCustomAttribute<OPCMemberAttribute>()?.Name;
            if (baseName is null)
            {
                throw new Exception("not found OPCMemberAttributes");
            }
            var mName = "Tray_Feedback";
            // 先这样 等下做缓存
            return $"{baseName}.{mName}.[{idx}]";
        }
    }
}
