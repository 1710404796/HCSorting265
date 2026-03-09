using HBYWCZP2024.Service;
using System.Collections.Concurrent;

namespace HBYWCZP2024.Utils
{
    internal static class EquipStatusState
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<int, EquipStatus>> _state = new();

        public static void Set(string key, int insideStationNum, EquipStatus status)
        {
            var lineState = _state.GetOrAdd(key, _ => new ConcurrentDictionary<int, EquipStatus>());
            lineState[insideStationNum] = status;
        }

        public static bool TryGet(string key, int insideStationNum, out EquipStatus status)
        {
            status = EquipStatus.I;
            return _state.TryGetValue(key, out var lineState) && lineState.TryGetValue(insideStationNum, out status);
        }
    }
}
