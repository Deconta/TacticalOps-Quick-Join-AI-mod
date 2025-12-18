namespace TacticalOpsQuickJoin;

public static class PlayerListRenderer
{
    public static void RenderPlayerList(DataGridView playerListView, ServerData serverData)
    {
        ArgumentNullException.ThrowIfNull(playerListView);
        ArgumentNullException.ThrowIfNull(serverData);

        playerListView.Rows.Clear();

        foreach (var player in serverData.Players)
        {
            bool isBot = player.Ping == 0;
            string displayName = FormatPlayerName(player.Name, isBot);
            
            var row = new DataGridViewRow();
            row.CreateCells(playerListView, displayName, player.Score, player.Kills, player.Deaths, player.Ping, player.Team);
            
            ApplyPlayerRowStyle(row, player.Team, isBot);
            
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
