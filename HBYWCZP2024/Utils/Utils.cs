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
        public static string GetOPCAddress(this string address, string name)
        {
            if (MainStorage.Saves.OpcTable == null) return default;
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
            return client.WriteNode(add, data);
            // return add;
            // return MainStorage.Saves.OpcTable[address];
        }
    }
}
