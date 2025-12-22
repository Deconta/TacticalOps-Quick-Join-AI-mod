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

        private Dictionary<string, string> serverInfo;

        public ServerData(int id, string serverAddress) {
            Id = id;
            string[] parts = serverAddress.Split(':');
            ServerIP = parts.Length > 0 ? parts[0] : serverAddress;
            ServerPort = parts.Length > 1 && int.TryParse(parts[1], out int p) ? p : 7777;

            serverInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public void SetInfo(string data) {
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

            string[] dataElements = data.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            bool containsFinal = false;

            // Start from 0 for key, 1 for value
            for (int i = 0; i < dataElements.Length - 1; i = i + 2)
            {
                string tag = dataElements[i];
                string content = dataElements[i + 1];

                if (tag.Equals("final", StringComparison.OrdinalIgnoreCase))
                {
                    containsFinal = true;
                    // In the original code, it would not store "final" as a key
                    // but continue parsing other elements if they exist.
                    // However, we can break here as "final" should be the last tag.
                    break;
                }
                if (tag.Equals("queryid", StringComparison.OrdinalIgnoreCase))
                {
                    // Skip queryid and its value
                    continue;
                }
                serverInfo[tag] = content;
            }

            // Check if "final" is the very last element in the array
            if (dataElements.Length > 0 && dataElements[dataElements.Length - 1].Equals("final", StringComparison.OrdinalIgnoreCase))
            {
                containsFinal = true;
            }
            
            return containsFinal;
        }

        public void ClearPlayerList() {
            Players.Clear();
            BotCount = 0;
            NumPlayers = 0;
        }

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
