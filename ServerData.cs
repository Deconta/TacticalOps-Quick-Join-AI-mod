using System;
using System.Collections.Generic;
using System.Linq;

namespace TacticalOpsQuickJoin {
    public class ServerData {
        public int Id { get; private set; }
        public string ServerIP { get; private set; }
        public int ServerPort { get; private set; }
        public string ServerName { get; set; } = string.Empty;
        public int Ping { get; set; } = 999;
        public int NumPlayers { get; private set; } = 0;
        public int MaxPlayers { get; private set; } = 0;
        public int BotCount { get; private set; } = 0;
        public bool IsTO220 { get; private set; }
        public bool IsTO340 { get; private set; }
        public bool IsTO350 { get; private set; }

        public string MapTitle { get; set; } = string.Empty;
        public bool Password { get; set; }
        public string GameType { get; set; } = string.Empty;
        public string HostPort { get; set; } = string.Empty;
        public string AdminName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string TostVersion { get; set; } = string.Empty;
        public string Protection { get; set; } = string.Empty;
        public string EseMode { get; set; } = string.Empty;
        public string TimeLimit { get; set; } = string.Empty;
        public string MinPlayers { get; set; } = string.Empty;
        public string FriendlyFire { get; set; } = string.Empty;
        public string ExplosionFF { get; set; } = string.Empty;

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
            if (serverInfo.TryGetValue("gametype", out string gameType))
            {
                GameType = gameType;
                IsTO220 = (gameType == "TO220");
                IsTO340 = (gameType == "TO340");
                IsTO350 = (gameType == "TO350");
            }

            if (serverInfo.TryGetValue("hostname", out string hostname)) ServerName = hostname;
            if (serverInfo.TryGetValue("maptitle", out string maptitle)) MapTitle = maptitle;
            if (serverInfo.TryGetValue("password", out string password)) Password = password == "True";
            if (serverInfo.TryGetValue("hostport", out string hostport)) HostPort = hostport;
            if (serverInfo.TryGetValue("adminname", out string adminname)) AdminName = adminname;
            if (serverInfo.TryGetValue("adminemail", out string adminemail)) AdminEmail = adminemail;
            if (serverInfo.TryGetValue("tostversion", out string tostversion)) TostVersion = tostversion;
            if (serverInfo.TryGetValue("protection", out string protection)) Protection = protection;
            if (serverInfo.TryGetValue("esemode", out string esemode)) EseMode = esemode;
            if (serverInfo.TryGetValue("timelimit", out string timelimit)) TimeLimit = timelimit;
            if (serverInfo.TryGetValue("minplayers", out string minplayers)) MinPlayers = minplayers;
            if (serverInfo.TryGetValue("friendlyfire", out string friendlyfire)) FriendlyFire = friendlyfire;
            if (serverInfo.TryGetValue("explositionff", out string explositionff)) ExplosionFF = explositionff;


            if (Ping == 999 && serverInfo.TryGetValue("ping", out string pVal) && int.TryParse(pVal, out int p))
                Ping = p;

            int actualPlayerCount = CountActualPlayers();
            if (serverInfo.TryGetValue("numplayers", out string npVal) && int.TryParse(npVal, out int np)) {
                NumPlayers = Math.Max(np, actualPlayerCount);
            } else {
                NumPlayers = actualPlayerCount;
            }

            if (serverInfo.TryGetValue("maxplayers", out string mpVal) && int.TryParse(mpVal, out int mp)) MaxPlayers = mp;
            
            CalculateBotsAndPlayers();
        }

        private void CalculateBotsAndPlayers()
        {
            Players.Clear();
            int bots = 0;
            for (int i = 0; i < 64; i++)
            {
                if (serverInfo.TryGetValue("player_" + i, out string playerName))
                {
                    var player = new Player { Id = i, Name = playerName };
                    if (serverInfo.TryGetValue("ping_" + i, out string pStr) && int.TryParse(pStr, out int p))
                    {
                        player.Ping = p;
                        if (p == 0) bots++;
                    }
                    if (serverInfo.TryGetValue("score_" + i, out string sStr) && int.TryParse(sStr, out int s)) player.Score = s;
                    if (serverInfo.TryGetValue("frags_" + i, out string fStr) && int.TryParse(fStr, out int f)) player.Kills = f;
                    if (serverInfo.TryGetValue("deaths_" + i, out string dStr) && int.TryParse(dStr, out int d)) player.Deaths = d;
                    if (serverInfo.TryGetValue("team_" + i, out string tStr) && int.TryParse(tStr, out int t)) player.Team = t;
                    Players.Add(player);
                }
            }
            BotCount = bots;
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

            for (int i = 0; i < dataElements.Length - 1; i += 2)
            {
                string key = dataElements[i];
                string value = dataElements[i + 1];

                if (key.Equals("final", StringComparison.OrdinalIgnoreCase))
                {
                    containsFinal = true;
                    continue;
                }
                if (key.Equals("queryid", StringComparison.OrdinalIgnoreCase)) continue;

                serverInfo[key] = value;
            }

            if (!containsFinal && dataElements.Length > 0)
            {
                if (dataElements[dataElements.Length - 1].Equals("final", StringComparison.OrdinalIgnoreCase))
                {
                    containsFinal = true;
                }
            }

            return containsFinal;
        }

        public void ClearPlayerList() {
            Players.Clear();
            BotCount = 0;
            NumPlayers = 0;
        }
    }
}
