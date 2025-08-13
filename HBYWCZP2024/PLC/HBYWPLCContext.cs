
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.PLC.Models;
using HBYWCZP2024.Service;
using HBYWCZP2024.Utils;
using LinxUniverse.OPCUA;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.PLC
{
    public class HBYWPLCContext(IMediator mediator, ILogger<HBYWPLCContext> logger) : IPLCContext
    {
        public HBYWPLCContext():this(App.GetService<IMediator>(), App.GetService<ILogger<HBYWPLCContext>>())
        {
        }
        public string Name;
        public string PlcGuid { get; set; }
        public SciOpcUaClient Plc { get; set; }

        public ITaskCodeDelectDB CodeDelectDB => throw new NotImplementedException();

        public ITaskCodeFeedbackDB CodeFeedbackDB => throw new NotImplementedException();

        public ITaskCodeDelectDB PCCodeDelectDB => throw new NotImplementedException();

        public ITaskCodeFeedbackDB PCCodeFeedbackDB => throw new NotImplementedException();


        public DB500 DB500 { get; set; } = new();
        public DB501 DB501 { get; set; } = new();
        public DB510 DB510 { get; set; } = new();
        public DB511 DB511 { get; set; } = new();
        public DB10 DB10 { get; set; } = new();
        public DB11 DB11 { get; set; } = new();
        public IMediator Mediator { get; } = mediator;
        public ILogger<HBYWPLCContext> Logger { get; } = logger;
        public Dictionary<string, string> OpcTable => MainStorage.Saves.KeysSave[Name].OpcTable;

        public ITaskCodeDelectDB GetCodeDelectDB(int idx) => idx switch
        {
            500 => DB500,
            510 => DB510,
            10 => DB10,
            _ => DB500,
        };

        public ITaskCodeFeedbackDB GetCodeFeedbackDB(int idx) => idx switch
        {
            501 => DB501,
            511 => DB511,
            11 => DB11,
            _ => DB501,
        };
        long heartTag = 0;
        bool heartFlag = false;
        public async Task Loop()
        {
            if (!Plc.Connected) return;

            //throw new NotImplementedException();
            await CheckError(DB500.ErrorCode);
            if (heartTag % 5 == 0)
            {
                var test = Plc.WriteTagNode("WCS_to_JXS.Data.to_heartbeat", heartFlag ? (byte)1 : (byte)0, Name);

                heartFlag = !heartFlag;


            }
            heartTag++;
        }
        ushort _lastCode = 0;
        DateTime _lastErrorStartTime = DateTime.Now;
        async Task CheckError(ushort errorCode)
        {
            if (errorCode != _lastCode)
            {
                if (errorCode == 0)
                {


                    await Mediator.Publish(new UploadDeivceErrorNotice(Name, errorCode));

                }
                else
                {
                    //if (_lastCode != 0)
                    //{
     

        

                    //    await Mediator.Publish(new UploadDeivceErrorNotice(errorCode));
                    //    //return;
                    //}
                    _lastErrorStartTime = DateTime.Now;

                    await Mediator.Publish(new UploadDeivceErrorNotice(Name, errorCode));


                }
                _lastCode = errorCode;
            }
        }
        public async Task<bool> UpdateData()
        {
            // return true;
            if (!Plc.Connected) return false;
            var flag = true;
            // Todo: 要加名字参数
            string[] a = [
                "WCS_JXS.Data", 
                "WCS_to_JXS.Data", 
                "PLC_TO_WCS_ScanTheCode.PLC", 
                "WCS_TO_PLC_ScanTheCode.WCS", 
                "PLC_TO_WCS_Tray", 
                "WCS_TO_PLC_Tray"
                ];

            var task1 = Task.Run(() => Plc.ReadClass(DB500, "WCS_JXS.Data".GetOPCAddress(Name)));
            var task2 = Task.Run(() => Plc.ReadClass(DB501, "WCS_to_JXS.Data".GetOPCAddress(Name)));
            var task3 = Task.Run(() => Plc.ReadClass(DB510, "PLC_TO_WCS_ScanTheCode.PLC".GetOPCAddress(Name)));
            var task4 = Task.Run(() => Plc.ReadClass(DB511, "WCS_TO_PLC_ScanTheCode.WCS".GetOPCAddress(Name)));
            var task5 = Task.Run(() => Plc.ReadClass(DB10, "PLC_TO_WCS_Tray".GetOPCAddress(Name)));
            var task6 = Task.Run(() => Plc.ReadClass(DB11, "WCS_TO_PLC_Tray".GetOPCAddress(Name)));

            Task[] tasks = [task1, task2, task3, task4, task5, task6];

            await Task.WhenAll(tasks);


            //a.AsParallel().Iter(s =>
            //{

            //});

            

            //Plc.ReadClass(DB500, "WCS_JXS.Data".GetOPCAddress(Name));
            //Plc.ReadClass(DB501, "WCS_to_JXS.Data".GetOPCAddress(Name));
            //Plc.ReadClass(DB510, "PLC_TO_WCS_ScanTheCode.PLC".GetOPCAddress(Name));
            //Plc.ReadClass(DB511, "WCS_TO_PLC_ScanTheCode.WCS".GetOPCAddress(Name));
            //Plc.ReadClass(DB10, "PLC_TO_WCS_Tray".GetOPCAddress(Name));
            //Plc.ReadClass(DB11, "WCS_TO_PLC_Tray".GetOPCAddress(Name));

            return flag;
        }
    }
}
