using HBYWCZP2024.Mediator.Commands;
using HBYWCZP2024.Mediator.ECS;
using HBYWCZP2024.Models.CZPallet;
using HBYWCZP2024.Models.ECS;
using HBYWCZP2024.Utils;
using LinxUniverse.OPCUA;
using LinxUniverse.OPCUA.Attributes;

using Microsoft.Extensions.Logging;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HBYWCZP2024.PLC.Tasks
{
    [OPCDelectInfo(500, 501)]

    public class SinglePlacementCompletedTask : HBBaseTask
    {
        private ushort grabStation;
        private ushort putStaion;
        private ushort[] grabChannel;
        private ushort[] putChannel;
        private bool[] claw;
        private string[] batteryCodes;

        public SinglePlacementCompletedTask() : base(5)
        {
            //TaskName = "单次放置完成";
            TaskName = Properties.Resources.SinglePlacementCompleted;
        }

        public override bool GetPLCData()
        {
            grabStation = Context.DB500.GrabStation;
            putStaion = Context.DB500.PutStation;

            grabChannel = Context.DB500.GrabChannels;
            putChannel = Context.DB500.PutChannels;

            claw = Context.DB500.Claws;

            batteryCodes = Context.DB500.BatteryCodes;//.Select(s => Encoding.UTF8.GetString(s.Code).Trim()).ToArray();

            return base.GetPLCData();
        }

        public override async Task<bool> DoSth()
        {
            var grab = CZPUtils.GetXYLStationByInsideNum(Context.Name, grabStation);
            var put = CZPUtils.GetXYLStationByInsideNum(Context.Name, putStaion);
             
            if (grab == null)
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.CaptureWorkstation}: {grabStation} {Properties.Resources.NotFound}"));
                return false;
            }

            if (put == null)
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.PlaceWorkstation}: {putStaion} {Properties.Resources.NotFound}"));
                return false;
            }
             

            if (grab is Pallet grabPallet)
            {
                if (string.IsNullOrEmpty(grabPallet.PalletCode))
                {
                    await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.CaptureWorkstation}: {grabStation} {Properties.Resources.Tray}{grabPallet.PalletId} {Properties.Resources.BarcodeIsEmpty}"));
                    return false;
                }
            }

            if (put is Pallet putPallet)
            {
                if (string.IsNullOrEmpty(putPallet.PalletCode))
                {
                    await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.PlaceWorkstation}: {putStaion} {Properties.Resources.Tray} {putPallet.PalletId} {Properties.Resources.BarcodeIsEmpty}"));
                    return false;
                }
            }

            var data = new S105
            {
                TrayNumber = put is Pallet ? put!.PalletCode : string.Empty,
                BatteryCount = grabChannel.Count(s => s > 0),
                BatteryBarcodes = batteryCodes.Where(s => !string.IsNullOrWhiteSpace(s)).ToList(),
                // 工位号是什么
            };


            for (int i = 0; i < grabChannel.Length; i++)
            {
                int zeroBaseGrabChannel = grabChannel[i] - 1;

                if (claw[i])
                {
                    Context.Logger.LogInformation
                    ("grabStation:{grabStation} grabChannel:{grabChannel} putStation:{putStaion} putChannel:{putChannel} batteryCode:{batteryCode}",
                    grabStation, grabChannel[i], putStaion, putChannel[i], batteryCodes[i]);

                    if (grabChannel[i] <= 0) // || grabChannel[i] >= grab.ChannelNum)
                    {
                        await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.CaptureWorkstation}: {grabStation} {Properties.Resources.Channel} {grabChannel[i]} {Properties.Resources.OutOfRange}"));
                        return false;
                    }

                    if (putChannel[i] <= 0) // || putChannel[i] >= put.Materials.Count)
                    {
                        await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.PlaceWorkstation}: {putStaion} {Properties.Resources.Tray}{putChannel[i]}{Properties.Resources.OutOfRange}"));
                        return false;
                    }

                    string finCode = string.IsNullOrWhiteSpace(batteryCodes[i]) ? grab!.Channels[zeroBaseGrabChannel].Code : batteryCodes[i];

                    if (string.IsNullOrWhiteSpace(finCode))
                    {
                        Context.Logger.LogWarning("跳过一抓抓取为空，夹爪：{claw}", i + 1);
                        continue;
                    }

                    //data.Materials.Add(new GrabMaterialInfo
                    //{
                    //    ChannelFrom = grabChannel[i],
                    //    ChannelTo = putChannel[i],  
                    //    MaterialCode = finCode,
                    //});

                    //if (grab!.Name == "机器人夹爪")
                    //{
                    //    data.ContainerFrom = grab.Channels[zeroBaseGrabChannel].ContainerFrom;
                    //    data.StationFrom = grab.Channels[zeroBaseGrabChannel].StationFrom.ToString();
                    //    data.Materials.Last().ChannelFrom = grab.Channels[zeroBaseGrabChannel].ChannelFrom;
                    //}
                }
            }

            if (!claw.Any())
            {
                await Context.Mediator.Send(new PlcDataWarningBoxCommand(Properties.Resources.GripperStatusIsEmpty));
                return false;
            }

            //if (!data.Materials.Any())
            //{
            //    await Context.Mediator.Send(new PlcDataWarningBoxCommand($"{Properties.Resources.AllGrabbingPositionDataIsEmpty}: {string.Join(',', claw)}"));
            //    return false;
            //}

            //if (put!.Name != "机器人夹爪")
            //{
            //    if(!await Context.Mediator.Send(new GrabCommand(data)))
            //    {
            //        //await Context.Mediator.Send(new WarningBoxCommand($"工位{grabStation} {grab!.Name} -> 工位{putStaion} {put!.Name}"));
            //        return false;
            //    }
            //}
            //if (put is Pallet || grab is Pallet)
            //{
            //    // 要发两次可能
            //    if (!await Context.Mediator.Send(new GrabCommand(Context.Name)))
            //    {
            //        //await Context.Mediator.Send(new WarningBoxCommand($"工位{grabStation} {grab!.Name} -> 工位{putStaion} {put!.Name}"));
            //        return false;
            //    }
            //}
            var codes = Enumerable.Range(0, grabChannel.Length)
                .Map(i => (grabChannel[i], putChannel[i], claw[i], batteryCodes[i]))
                .Filter(s => s.Item3)
                .Map(s => new BatteryMoveInfo(s.Item4, s.Item1, s.Item2))
                .ToArr();
            if (!await Context.Mediator.Send(new GrabCommand(Context.Name, new GrabInfo(grab, put, codes))))
            {
                //await Context.Mediator.Send(new WarningBoxCommand($"工位{grabStation} {grab!.Name} -> 工位{putStaion} {put!.Name}"));
                return false;
            }

            for (int i = 0; i < grabChannel.Length; ++i)
            {
                if (claw[i])
                {
                    // grab.Channels[grabChannel[i] - 1].Code = string.Empty;
                    CZPUtils.MoveTo(grab!, put!, grabChannel[i] - 1, putChannel[i] - 1, batteryCodes[i]); // 思考一下这里
                }
            }


            return await base.DoSth();
        }

        protected override Task WriteDataToPlc()
        {
            //Context.Plc.Write("DB501.DBW4", grabStation);
            //Context.Plc.Write("DB501.DBW6", putStaion);
            Context.Plc.WriteTagNode("WCS_to_JXS.Data.to_grabStation", grabStation, Context.Name);
            Context.Plc.WriteTagNode("WCS_to_JXS.Data.to_putStation", putStaion, Context.Name);
            //Context.Plc.WriteNode("WCS_to_JXS.Data.to_grabStation".GetOPCAddress(), grabStation);

            //Context.Plc.WriteNode("WCS_to_JXS.Data.to_putStation".GetOPCAddress(), putStaion);


            return base.WriteDataToPlc();
        }

        protected override Task<bool> CleanData()
        {
            //Context.Plc.Write("DB501.DBW4", 0);
            //Context.Plc.Write("DB501.DBW6", 0);
            Context.Plc.WriteTagNode("WCS_to_JXS.Data.to_grabStation", (ushort)0, Context.Name);
            Context.Plc.WriteTagNode("WCS_to_JXS.Data.to_putStation", (ushort)0, Context.Name);

            //Context.Plc.WriteNode("WCS_to_JXS.Data.to_grabStation".GetOPCAddress(), (ushort)0);
            //Context.Plc.WriteNode("WCS_to_JXS.Data.to_putStation".GetOPCAddress(), (ushort)0);

            return base.CleanData();
        }
    }
}
