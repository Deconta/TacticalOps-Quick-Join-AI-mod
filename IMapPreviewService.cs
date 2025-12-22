#nullable enable
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TacticalOpsQuickJoin
{
    public interface IMapPreviewService
    {
        void InitiateMapPreview(string? imageUrl, string mapName, string serverName, string currentMap, Point cursorPosition);
        void CloseMapPreview();
    }
}
