namespace TacticalOpsQuickJoin;

public static class PlayerListRenderer
{
    public static void RenderPlayerList(DataGridView playerListView, ServerData serverData)
    {
        ArgumentNullException.ThrowIfNull(playerListView);
        ArgumentNullException.ThrowIfNull(serverData);

        playerListView.Rows.Clear();

        for (int i = 0; i < UIConstants.MAX_PLAYERS; i++)
        {
            string playerName = serverData.GetProperty($"player_{i}");
            if (string.IsNullOrEmpty(playerName)) continue;

            if (!int.TryParse(serverData.GetProperty($"score_{i}"), out int score)) score = 0;
            if (!int.TryParse(serverData.GetProperty($"frags_{i}"), out int kills)) kills = 0;
            if (!int.TryParse(serverData.GetProperty($"deaths_{i}"), out int deaths)) deaths = 0;
            if (!int.TryParse(serverData.GetProperty($"ping_{i}"), out int ping)) ping = 0;
            if (!int.TryParse(serverData.GetProperty($"team_{i}"), out int team)) team = 255;

            bool isBot = ping == 0;
            
            string displayName = FormatPlayerName(playerName, isBot);
            
            var row = new DataGridViewRow();
            row.CreateCells(playerListView, displayName, score, kills, deaths, ping, team);
            
            ApplyPlayerRowStyle(row, team, isBot);
            
            playerListView.Rows.Add(row);
        }
    }

    private static string FormatPlayerName(string playerName, bool isBot)
    {
        if (isBot)
            return $"{playerName} (Bot)";
        return playerName;
    }

    private static void ApplyPlayerRowStyle(DataGridViewRow row, int team, bool isBot)
    {
        Color backColor;
        Color foreColor;

        if (isBot)
        {
            backColor = UIConstants.CommonColors.BotColor;
            foreColor = UIConstants.CommonColors.BotForeground;
        }
        else
        {
            backColor = GetTeamColor(team);
            foreColor = Color.WhiteSmoke;
        }

        foreach (DataGridViewCell cell in row.Cells)
        {
            cell.Style.BackColor = backColor;
            cell.Style.ForeColor = foreColor;
            cell.Style.SelectionBackColor = backColor;
            cell.Style.SelectionForeColor = foreColor;
        }
    }

    private static Color GetTeamColor(int team) => team switch
    {
        0 => UIConstants.CommonColors.TeamRed,
        1 => UIConstants.CommonColors.TeamBlue,
        _ => UIConstants.CommonColors.TeamNone
    };

    public static Color GetPingColor(int ping)
    {
        if (ping == UIConstants.PING_TIMEOUT)
            return UIConstants.CommonColors.PingTimeout;
        if (ping <= UIConstants.PING_EXCELLENT_THRESHOLD)
            return UIConstants.CommonColors.PingExcellent;
        if (ping <= UIConstants.PING_GOOD_THRESHOLD)
            return UIConstants.CommonColors.PingGood;
        if (ping <= UIConstants.PING_MEDIUM_THRESHOLD)
            return UIConstants.CommonColors.PingMedium;
        
        return UIConstants.CommonColors.PingPoor;
    }
}
