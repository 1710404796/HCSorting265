using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HBYWCZP2024.ViewModel;

namespace HBYWCZP2024.Data.Models
{
    public class OKNGCnt
    {
        // 想办法通知一下

        
        public int Id { get; set; }

        public int OKCnt { get; set; }
        /// <summary>
        /// 扫码NG
        /// </summary>
        public int NG1Cnt { get; set; }

        /// <summary>
        /// 校验NG
        /// </summary>
        public int NG2Cnt { get; set; }

        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; }

        //public void AddOk(int value)
        //{
        //    OKCnt += value;
        //    App.GetService<DataDisplayViewModel>().Update();
        //}

        //public void AddNG1(int value)
        //{
        //    NG1Cnt += value;
        //    App.GetService<DataDisplayViewModel>().Update();
        //}

        //public void AddNG2(int value)
        //{
        //    NG2Cnt += value;
        //    App.GetService<DataDisplayViewModel>().Update();
        //}
    }
}
