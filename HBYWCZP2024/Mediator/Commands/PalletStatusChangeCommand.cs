using LanguageExt.ClassInstances;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024.Mediator.Commands
{
    public record PalletStatusChangeCommand(string name, int PalletIdx, PalletStatusChange PalletStatusChange) : IRequest<bool>;

    public enum PalletStatusChange
    {
        /// <summary>
        /// 空托盘
        /// </summary>
        EmptyPallet = 0,
        /// <summary>
        /// 有电池
        /// </summary>
        WithBattery = 1,
        /// <summary>
        /// 有电池且已筛选
        /// </summary>
        WithBatteryAndChecked = 2,
    }
}


