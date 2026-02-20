using HBYWCZP2024.Attritubes;
using HBYWCZP2024.ViewModel;
using Microsoft.Extensions.Logging;
using OpcUaHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HBYWCZP2024.Utils
{
    public static class Utils
    {
        public static string GetRoleName(RoleEnum role)
        {
            return role switch
            {
                RoleEnum.操作员 => Properties.Resources.OPNOperator,
                RoleEnum.技师 => Properties.Resources.OPNTechnician,
                RoleEnum.me技术员 => Properties.Resources.PEEngineer,
                RoleEnum.ME工程师 => Properties.Resources.MEEngineer,
                RoleEnum.系统管理员 => Properties.Resources.Administrator,
                _ => role.ToString()
            };
        }

        /// <summary>
        /// 获取权限名称
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static string GetPowerName(PowerEnum power)
        {
            return power switch
            {
                PowerEnum.报警日志 => Properties.Resources.AlarmLog,
                PowerEnum.WCS交互日志 => Properties.Resources.InteractionLog,
                PowerEnum.用户管理界面 => Properties.Resources.UserManagementInterface,

                PowerEnum.工位状态 => Properties.Resources.WorkstationStatus,
                PowerEnum.电芯计数 => Properties.Resources.CellCount,
                PowerEnum.组盘日志 => Properties.Resources.AssemblyLog,
                PowerEnum.调度号工位表 => Properties.Resources.DispatchNumberAndWorkstationNumberTable,
                PowerEnum.条码规则表 => Properties.Resources.BarcodeRulesTable,
                PowerEnum.权限管理 => Properties.Resources.AccessControl,
                PowerEnum.程序参数设定 => Properties.Resources.ParameterSettings,
                PowerEnum.交互日志 => Properties.Resources.InteractionLog,

                PowerEnum.日志窗口 => Properties.Resources.LogWindow,
                PowerEnum.权限配置表 => Properties.Resources.PermissionsTableConfiguration,
                PowerEnum.重新匹配PLC节点 => Properties.Resources.RematchPLCNodes,



                _ => power.ToString()
            };
        }
        public static string GetOPCAddress(this string address, string name)
        {
            if (MainStorage.Saves.KeysSave[name].OpcTable == null) return default;
            var res = MainStorage.Saves.KeysSave[name].OpcTable.TryGetValue(address, out var opcAddress);
            return opcAddress;
        }
        [JsonIgnore]
        static Lazy<ILogger> _lazyLogger = new Lazy<ILogger>(App.GetService<ILogger<MainViewModel>>);
        [JsonIgnore]
        static ILogger _logger => _lazyLogger.Value;
        public static bool WriteTagNode<T>(this OpcUaClient client, string address, T data, string name)
        {
            var add = address.GetOPCAddress(name);
            if (add == default)
            {
                _logger.LogWarning("没有此地址, {add}", address);
                return true;
            }
            try
            {
                return client.WriteNode(add, data);

            }
            catch (Exception ex) {
                Console.WriteLine(address);
                throw;
            
            }
    
            // return add;
            // return MainStorage.Saves.OpcTable[address];
        }
    }
}
