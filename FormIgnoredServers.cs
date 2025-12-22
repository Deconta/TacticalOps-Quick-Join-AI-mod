using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TacticalOpsQuickJoin
{
    public partial class FormIgnoredServers : Form
    {
        private readonly ISettingsService _settingsService;
        private readonly HashSet<string> _ignoredServers;

        public FormIgnoredServers(ISettingsService settingsService, HashSet<string> ignoredServers)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _ignoredServers = ignoredServers;

            LoadIgnoredServers();
        }

        private void LoadIgnoredServers()
        {
            ignoredServersListBox.Items.Clear();
            foreach (var server in _ignoredServers)
            {
                ignoredServersListBox.Items.Add(server);
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            if (ignoredServersListBox.SelectedItem is string selectedServer)
            {
                _ignoredServers.Remove(selectedServer);
                _settingsService.IgnoredServers = string.Join(",", _ignoredServers);
                _settingsService.Save();
                LoadIgnoredServers();
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
