using LinxUniverse.PLC.Common.Interface;
using LinxUniverse.PLCProtos;
using S7.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.PLC
{
    public class HBYWPLCContext : IPLCContext
    {
        public string PlcGuid { get ; set ; }
        public Plc Plc { get; set; }

        public ITaskCodeDelectDB CodeDelectDB => throw new NotImplementedException();

        public ITaskCodeFeedbackDB CodeFeedbackDB => throw new NotImplementedException();

        public ITaskCodeDelectDB PCCodeDelectDB => throw new NotImplementedException();

        public ITaskCodeFeedbackDB PCCodeFeedbackDB => throw new NotImplementedException();

        public ITaskCodeDelectDB GetCodeDelectDB(int idx)
        {
            throw new NotImplementedException();
        }

        public ITaskCodeFeedbackDB GetCodeFeedbackDB(int idx)
        {
            throw new NotImplementedException();
        }

        public Task Loop()
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateData()
        {
            throw new NotImplementedException();
        }
    }
}
