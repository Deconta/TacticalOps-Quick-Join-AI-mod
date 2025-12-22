#nullable enable
using System.Reflection;
using System.Windows.Forms;

namespace TacticalOpsQuickJoin
{
    public static class UIHelper
    {
        public static void EnableDoubleBuffering(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, control, new object[] { true });
        }
    }
}
