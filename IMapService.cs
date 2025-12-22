#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TacticalOpsQuickJoin
{
    public interface IMapService
    {
        Task LoadMapDataAsync();
        MapData? FindBestMapMatch(string mapName);
        string NormalizeMapName(string name);
    }
}
