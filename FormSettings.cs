#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TacticalOpsQuickJoin
{
    public partial class FormSettings : Form
    {
        private readonly ISettingsService _settingsService;
        private readonly HashSet<string> _ignoredServers;
        private readonly string? _initialTabName;

        public FormSettings(ISettingsService settingsService, string? initialTabName = null)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _ignoredServers = new HashSet<string>((_settingsService.IgnoredServers ?? "").Split(',').Where(s => !string.IsNullOrWhiteSpace(s)));
            _initialTabName = initialTabName;
            LoadSettings();
            this.Load += new System.EventHandler(this.FormSettings_Load);
        }

        private void FormSettings_Load(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_initialTabName))
            {
                foreach (TabPage page in tabControl.TabPages)
                {
                    if (page.Text == _initialTabName)
                    {
                        tabControl.SelectedTab = page;
                        break;
                    }
                }
            }
        }

        private void LoadSettings()
        {
            // General Tab
            closeOnJoinCheckBox.Checked = _settingsService.CloseOnJoin;
            darkModeCheckBox.Checked = _settingsService.DarkMode;
            autoRefreshIntervalNumericUpDown.Value = _settingsService.AutoRefreshInterval;

            // Paths Tab
            pathTO22TextBox.Text = _settingsService.GetGamePath("2.2");
            pathTO34TextBox.Text = _settingsService.GetGamePath("3.4");
            pathTO35TextBox.Text = _settingsService.GetGamePath("3.5");

            // Master Servers Tab
            masterServersTextBox.Text = _settingsService.MasterServers;
        }

        private void ApplySettings()
        {
            // General Tab
            _settingsService.CloseOnJoin = closeOnJoinCheckBox.Checked;
            _settingsService.DarkMode = darkModeCheckBox.Checked;
            _settingsService.AutoRefreshInterval = (int)autoRefreshIntervalNumericUpDown.Value;

            // Paths Tab
            _settingsService.SetGamePath("2.2", pathTO22TextBox.Text);
            _settingsService.SetGamePath("3.4", pathTO34TextBox.Text);
            _settingsService.SetGamePath("3.5", pathTO35TextBox.Text);

            // Master Servers Tab
            _settingsService.MasterServers = masterServersTextBox.Text;

            _settingsService.Save();
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            ApplySettings();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void applyButton_Click(object sender, System.EventArgs e)
        {
            ApplySettings();
        }

        private void browseTO22Button_Click(object sender, System.EventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                FileName = "TacticalOps.exe",
                Filter = "Tactical Ops 2.2|TacticalOps.exe|All files (*.*)|*.*",
                Title = "Please select Tactical Ops 2.2 executable"
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    pathTO22TextBox.Text = dialog.FileName;
                }
            }
        }

        private void browseTO34Button_Click(object sender, System.EventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                FileName = "TacticalOps.exe",
                Filter = "Tactical Ops 3.4|TacticalOps.exe|All files (*.*)|*.*",
                Title = "Please select Tactical Ops 3.4 executable"
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    pathTO34TextBox.Text = dialog.FileName;
                }
            }
        }

        private void browseTO35Button_Click(object sender, System.EventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                FileName = "TacticalOps.exe",
                Filter = "Tactical Ops 3.5|TacticalOps.exe|All files (*.*)|*.*",
                Title = "Please select Tactical Ops 3.5 executable"
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    pathTO35TextBox.Text = dialog.FileName;
                }
            }
        }

        private void ignoredServersButton_Click(object sender, System.EventArgs e)
        {
            using(var form = new FormIgnoredServers(_settingsService, _ignoredServers))
            {
                form.ShowDialog(this);
            }
        }
    }
}
