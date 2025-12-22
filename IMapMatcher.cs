#nullable enable
using System.Collections.Generic;

namespace TacticalOpsQuickJoin
{
    public interface IMapMatcher
    {
        MapData? FindBestMapMatch(string mapName, List<MapData> mapList);
    }
}
