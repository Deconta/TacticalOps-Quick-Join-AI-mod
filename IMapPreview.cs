#nullable enable
using System.Threading.Tasks;

namespace TacticalOpsQuickJoin
{
    public interface IMapPreview
    {
        void ShowMapPreview(string? imageUrl, string mapName, string serverName, string currentMap, System.Drawing.Point cursorPosition);
        void CloseMapPreview();
    }
}
