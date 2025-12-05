using System;
using System.Collections.Generic;
using System.Linq;

namespace TacticalOpsQuickJoin {
    public class ServerData {
        public int Id { get; private set; }
        public string ServerIP { get; private set; }
        public int ServerPort { get; private set; }
        public string ServerName { get; private set; }
        
        public int Ping { get; set; } = 999;
        
        public int NumPlayers { get; private set; } = 0;
        public int MaxPlayers { get; private set; } = 0;
        
        // NEU: Bot Count
        public int BotCount { get; private set; } = 0;
        
        public bool IsTO220 { get; private set; }
        public bool IsTO340 { get; private set; }
        public bool IsTO350 { get; private set; }

        private Dictionary<string, string> serverInfo;

        public ServerData(int id, string serverAddress) {
            Id = id;
            string[] parts = serverAddress.Split(':');
            ServerIP = parts.Length > 0 ? parts[0] : serverAddress;
            ServerPort = parts.Length > 1 && int.TryParse(parts[1], out int p) ? p : 7777;

            serverInfo = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string GetProperty(string name) {
            if (serverInfo.TryGetValue(name, out string value))
            {
                return value;
            }
            return GetDefaultValueForKey(name);
        }

        public string GetDefaultValueForKey(string key) {
            if (key.StartsWith("frags_", StringComparison.OrdinalIgnoreCase)) return "0";
            if (key.StartsWith("deaths_", StringComparison.OrdinalIgnoreCase)) return "0";
            if (key.StartsWith("score_", StringComparison.OrdinalIgnoreCase)) return "0";
            if (key.StartsWith("ping_", StringComparison.OrdinalIgnoreCase)) return "999";
            if (key.StartsWith("team_", StringComparison.OrdinalIgnoreCase)) return "0";

            switch (key.ToLower()) {
                case "tostversion":
                case "protection":
                case "esemode": return "-";
                case "frags":
                case "deaths":
                case "team":
                case "score": return "0";
                case "ping": return "999";
            }
            return string.Empty;
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
                IsTO220 = (gameType == "TO220");
                IsTO340 = (gameType == "TO340");
                IsTO350 = (gameType == "TO350");
            }
            
            if (serverInfo.TryGetValue("hostname", out string hostname)) ServerName = hostname;

            if (Ping == 999 && serverInfo.TryGetValue("ping", out string pVal) && int.TryParse(pVal, out int p)) 
                Ping = p;

            if (serverInfo.TryGetValue("numplayers", out string npVal) && int.TryParse(npVal, out int np)) NumPlayers = np;
            if (serverInfo.TryGetValue("maxplayers", out string mpVal) && int.TryParse(mpVal, out int mp)) MaxPlayers = mp;

            // --- BOT DETECTION ---
            // Wir scannen alle Spieler-Pings. Ping 0 = Bot.
            CalculateBots();
        }

        private void CalculateBots()
        {
            int bots = 0;
            // Unreal Engine Player IDs gehen meist von 0 bis 64
            for (int i = 0; i < 64; i++)
            {
                // Prüfen ob ein Spieler an diesem Index existiert (hat einen Namen?)
                if (serverInfo.ContainsKey("player_" + i))
                {
                    // Ping holen
                    if (serverInfo.TryGetValue("ping_" + i, out string pStr) && int.TryParse(pStr, out int p))
                    {
                        if (p == 0) bots++;
                    }
                    // Fallback: Wenn kein Ping gesendet wurde, aber Spieler existiert, könnte es auch 0 sein, 
                    // aber wir zählen sicherheitshalber nur explizite Nullen.
                }
            }
            BotCount = bots;
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
            var keysToRemove = serverInfo.Keys
                .Where(k => k.StartsWith("player_", StringComparison.OrdinalIgnoreCase) ||
                            k.StartsWith("score_", StringComparison.OrdinalIgnoreCase) ||
                            k.StartsWith("frags_", StringComparison.OrdinalIgnoreCase) ||
                            k.StartsWith("deaths_", StringComparison.OrdinalIgnoreCase) ||
                            k.StartsWith("ping_", StringComparison.OrdinalIgnoreCase) ||
                            k.StartsWith("team_", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (string key in keysToRemove) {
                serverInfo.Remove(key);
            }
            BotCount = 0;
        }
    }
}
