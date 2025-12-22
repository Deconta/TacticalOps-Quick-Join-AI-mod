#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace TacticalOpsQuickJoin
{
    public static class ServerListSorter
    {
        public static IEnumerable<DataGridViewRow> Sort(
            List<DataGridViewRow> rows,
            DataGridViewColumn sortColumn,
            ListSortDirection sortDirection,
            ServerListManager serverListManager,
            Dictionary<int, ServerData> serverLookup)
        {
            int colIndex = sortColumn.Index;
            bool isAscending = sortDirection == ListSortDirection.Ascending;

            Func<DataGridViewRow, bool> isFavorite = r =>
                r.Tag is int id && serverLookup.ContainsKey(id) && serverListManager.FavoriteServers.Contains($"{serverLookup[id].ServerIP}:{serverLookup[id].ServerPort}");

            if (isAscending)
            {
                return rows
                    .OrderByDescending(isFavorite)
                    .ThenBy(r => GetComparableValue(r, colIndex, serverLookup));
            }
            else
            {
                return rows
                    .OrderByDescending(isFavorite)
                    .ThenByDescending(r => GetComparableValue(r, colIndex, serverLookup));
            }
        }

        private static IComparable GetComparableValue(DataGridViewRow row, int colIndex, Dictionary<int, ServerData> serverLookup)
        {
            if (row.Tag is not int serverId) return "";
            if (!serverLookup.TryGetValue(serverId, out var server)) return "";

            if (colIndex == UIConstants.PING_COLUMN_INDEX)
            {
                return server.Ping;
            }
            if (colIndex == UIConstants.PLAYERS_COLUMN_INDEX)
            {
                return Math.Max(0, server.NumPlayers - server.BotCount);
            }
            // Default to string comparison from the cell
            return row.Cells[colIndex].Value?.ToString() ?? "";
        }
    }
}
