using LinxUniverse.OPCUA.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.PLC.Tasks
{
    [OPCDelectInfo(600,601)]
    public class ApplyBatteryInfoTaskB: ApplyBatteryInfoTask;

    [OPCDelectInfo(600,601)]
    public class SinglePlacementCompletedTaskB : SinglePlacementCompletedTask;

    [OPCDelectInfo(600, 601)]
    public class ChaiPanCompleted2TaskB : ChaiPanCompleted2Task;

    [OPCDelectInfo(20, 21)]
    public class PalletArrivedTaskB(int idx) : PalletArrivedTask(idx);

    [OPCDelectInfo(20, 21)]
    public class GrabActionTaskB(int idx) : GrabActionTask(idx);
    [OPCDelectInfo(20, 21)]
    public class PalletOutTaskB(int idx) : PalletOutTask(idx);

    [OPCDelectInfo(20, 21)]
    public class GrabActionExitTaskB(int idx) : GrabActionExitTask(idx);





    [OPCDelectInfo(610,611)]
    public class CheckLadaiTaskB: CheckLadaiTask;
    [OPCDelectInfo(610,611)]
    public class CheckLadaiTask2B: CheckLadaiTask2;
    [OPCDelectInfo(610,611)]
    public class CheckLadaiTask3B: CheckLadaiTask3;


}
