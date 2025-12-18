#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TacticalOpsQuickJoin.Properties;

namespace TacticalOpsQuickJoin
{
    public static class GameLauncher
    {
        public static void LaunchGame(string version, ISettingsService settingsService, string args = "")
        {
            string? path = settingsService.GetGamePath(version);

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                if (MessageBox.Show($"TO {version} not found. Locate it?", $"Locate TO {version}", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using var ofd = new OpenFileDialog { Title = $"Select TO {version}", Filter = "TO Executable|*.exe|All Files|*.*" };
                    if (ofd.ShowDialog() != DialogResult.OK) return;
                    path = ofd.FileName;
                    settingsService.SetGamePath(version, path);
                    settingsService.Save();
                }
                else
                {
                    return;
                }
            }

            string executableName = Path.GetFileNameWithoutExtension(path);
            Process[] runningProcesses = Process.GetProcessesByName(executableName);
            if (runningProcesses.Length > 0)
            {
                MessageBox.Show($"'{executableName}' läuft bereits. Es kann keine zweite Instanz gestartet werden.", "Spiel läuft bereits", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Process.Start(path, args);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start game: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
