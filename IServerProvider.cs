#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TacticalOpsQuickJoin
{
    public interface IServerProvider
    {
        Task GetServerListAsync(Action<ServerData> onServerFound);
        Task GetServerDetailsAsync(ServerData serverData);
    }
}
