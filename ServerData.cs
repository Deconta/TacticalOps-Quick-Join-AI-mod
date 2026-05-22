using System;
using System.Collections.Generic;
using System.Linq;

namespace TacticalOpsQuickJoin {
    public class ServerData {
        public int Id { get; private set; }
        public string ServerIP { get; private set; }
        public int ServerPort { get; private set; }
        public string? ServerName { get; set; } = string.Empty;
        public int Ping { get; set; } = 999;
        public int NumPlayers { get; private set; } = 0;
        public int MaxPlayers { get; private set; } = 0;
        public int BotCount { get; private set; } = 0;
        public int HiddenFakePlayerCount { get; private set; } = 0;
        public bool IsTO220 { get; private set; }
        public bool IsTO340 { get; private set; }
        public bool IsTO350 { get; private set; }

        public string? MapTitle { get; set; } = string.Empty;
        public bool Password { get; set; }
        public string? GameType { get; set; } = string.Empty;
        public string? HostPort { get; set; } = string.Empty;
        public string? AdminName { get; set; } = string.Empty;
        public string? AdminEmail { get; set; } = string.Empty;
        public string? TostVersion { get; set; } = string.Empty;
        public string? Protection { get; set; } = string.Empty;
        public string? EseMode { get; set; } = string.Empty;
        public string? TimeLimit { get; set; } = string.Empty;
        public string? MinPlayers { get; set; } = string.Empty;
        public string? FriendlyFire { get; set; } = string.Empty;
        public string? ExplosionFF { get; set; } = string.Empty;

        public List<Player> Players { get; private set; } = new List<Player>();
        public string RawInfo { get; private set; } = string.Empty;

        private Dictionary<string, string> serverInfo;
        private static readonly string[] PlayerInfoPrefixes =
        {
            "player_",
            "score_",
            "frags_",
            "deaths_",
            "ping_",
            "team_"
        };

        public ServerData(int id, string serverAddress) {
            Id = id;
            string[] parts = serverAddress.Split(':');
            ServerIP = parts.Length > 0 ? parts[0] : serverAddress;
            ServerPort = parts.Length > 1 && int.TryParse(parts[1], out int p) ? p : 7777;

            serverInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public void SetInfo(string data) {
            RawInfo = data;
            ParseData(data);
            UpdateProperties();
        }

        public bool UpdateInfo(string data) {
            bool containsFinal = ParseData(data);
            UpdateProperties();
            return containsFinal;
        }

        private void UpdateProperties()
        {
            if (serverInfo.TryGetValue("gametype", out string? gameType))
            {
                GameType = gameType;
                IsTO220 = (gameType == "TO220");
                IsTO340 = (gameType == "TO340");
                IsTO350 = (gameType == "TO350");
            }

            if (serverInfo.TryGetValue("hostname", out string? hostname)) ServerName = hostname;
            if (serverInfo.TryGetValue("maptitle", out string? maptitle)) MapTitle = maptitle;
            if (serverInfo.TryGetValue("password", out string? password)) Password = password == "True";
            if (serverInfo.TryGetValue("hostport", out string? hostport)) HostPort = hostport;
            if (serverInfo.TryGetValue("adminname", out string? adminname)) AdminName = adminname;
            if (serverInfo.TryGetValue("adminemail", out string? adminemail)) AdminEmail = adminemail;
            if (serverInfo.TryGetValue("tostversion", out string? tostversion)) TostVersion = tostversion;
            if (serverInfo.TryGetValue("protection", out string? protection)) Protection = protection;
            if (serverInfo.TryGetValue("esemode", out string? esemode)) EseMode = esemode;
            if (serverInfo.TryGetValue("timelimit", out string? timelimit)) TimeLimit = timelimit;
            if (serverInfo.TryGetValue("minplayers", out string? minplayers)) MinPlayers = minplayers;
            if (serverInfo.TryGetValue("friendlyfire", out string? friendlyfire)) FriendlyFire = friendlyfire;
            if (serverInfo.TryGetValue("explositionff", out string? explositionff)) ExplosionFF = explositionff;


                        if (Ping == 999 && serverInfo.TryGetValue("ping", out string? pVal) && int.TryParse(pVal, out int p))


                            Ping = p;


            


            if (serverInfo.TryGetValue("maxplayers", out string? mpVal) && int.TryParse(mpVal, out int mp)) MaxPlayers = mp;
            if (serverInfo.TryGetValue("numplayers", out string? npVal) && int.TryParse(npVal, out int np)) NumPlayers = np;
            HiddenFakePlayerCount = IsKnownFakePlayerServer ? NumPlayers : 0;
        }

        private int CountActualPlayers()
        {
            int actualPlayers = 0;
            for (int i = 0; i < 64; i++)
            {
                if (serverInfo.ContainsKey("player_" + i))
                {
                    actualPlayers++;
                }
            }
            return actualPlayers;
        }

        private bool ParseData(string data)
        {
            if (string.IsNullOrEmpty(data)) return false;

            string[] dataElements = data.Split('\\');
            bool containsFinal = false;

            for (int i = 0; i < dataElements.Length;)
            {
                string tag = dataElements[i++];
                if (string.IsNullOrEmpty(tag)) continue;

                if (tag.Equals("final", StringComparison.OrdinalIgnoreCase))
                {
                    containsFinal = true;
                    continue;
                }
                if (tag.Equals("queryid", StringComparison.OrdinalIgnoreCase))
                {
                    if (i < dataElements.Length) i++;
                    continue;
                }

                if (i >= dataElements.Length) break;
                string content = dataElements[i++];
                serverInfo[tag] = content;
            }

            return containsFinal;
        }

        public void ClearPlayerList() {
            Players.Clear();
            BotCount = 0;
            NumPlayers = 0;
            HiddenFakePlayerCount = 0;
            foreach (var key in serverInfo.Keys.Where(IsPlayerInfoKey).ToList())
            {
                serverInfo.Remove(key);
            }
        }

        public int PlayerInfoCount => CountActualPlayers();
        public int VisiblePlayerInfoCount => GetVisiblePlayers().Count();
        public int DisplayHumanPlayerCount => Math.Max(0, NumPlayers - BotCount - HiddenFakePlayerCount);

        public bool IsKnownFakePlayerServer =>
            (ServerName ?? string.Empty).Contains("EL DORADO TACTICAL OPS", StringComparison.OrdinalIgnoreCase);

        public string GetPlayerSummary()
        {
            if (BotCount > 0)
                return $"{DisplayHumanPlayerCount} (+{BotCount} Bots) / {MaxPlayers}";

            return $"{DisplayHumanPlayerCount} / {MaxPlayers}";
        }

        public IEnumerable<Player> GetVisiblePlayers()
        {
            for (int i = 0; i < 64; i++)
            {
                var player = GetPlayer(i);
                if (player != null && !ShouldHidePlayer(player))
                    yield return player;
            }
        }

        private Player? GetPlayer(int index)
        {
            string playerName = GetProperty("player_" + index);
            if (string.IsNullOrEmpty(playerName)) return null;

            return new Player
            {
                Id = index,
                Name = playerName,
                Score = ParsePlayerInt("score_", index),
                Kills = ParsePlayerInt("frags_", index),
                Deaths = ParsePlayerInt("deaths_", index),
                Ping = ParsePlayerInt("ping_", index, 999),
                Team = ParsePlayerInt("team_", index)
            };
        }

        private int ParsePlayerInt(string prefix, int index, int defaultValue = 0) =>
            int.TryParse(GetProperty(prefix + index), out int value) ? value : defaultValue;

        private bool ShouldHidePlayer(Player player)
        {
            if (IsKnownFakePlayerServer)
                return true;

            if (!int.TryParse(MinPlayers, out int minPlayers) || minPlayers <= 0)
                return false;

            bool looksLikeMinPlayersFill = PlayerInfoCount >= minPlayers && NumPlayers < PlayerInfoCount;
            return looksLikeMinPlayersFill && player.Ping >= UIConstants.PING_TIMEOUT;
        }

        private static bool IsPlayerInfoKey(string key) =>
            PlayerInfoPrefixes.Any(prefix => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        public string GetProperty(string name) {
            string? value = string.Empty;
            serverInfo.TryGetValue(name, out value);
            if (string.IsNullOrEmpty(value))
                value = GetDefaultValueForKey(name);
            return value;
        }

        public string GetDefaultValueForKey(string key) {
            if (key.StartsWith("frags_"))
                key = "frags";
            else if (key.StartsWith("deaths_"))
                key = "deaths";
            else if (key.StartsWith("score_"))
                key = "score";
            else if (key.StartsWith("ping_"))
                key = "ping";
            else if (key.StartsWith("team_"))
                key = "team";

            switch (key) {
                case "tostversion":
                case "protection":
                case "esemode": return "None";
                case "frags":
                case "deaths":
                case "team":
                case "score": return "0";
                case "ping": return "999";

            }
            return string.Empty;
        }
    }
}
