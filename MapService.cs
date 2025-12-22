#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TacticalOpsQuickJoin
{
    public class MapService : IMapService
    {
        private readonly HttpClient _httpClient;
        private List<MapData> _mapList = [];
        private readonly Dictionary<string, MapData> _mapLookup = new();

        public MapService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task LoadMapDataAsync()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appSpecificFolder = Path.Combine(appDataFolder, "TacticalOpsQuickJoin");
            if (!Directory.Exists(appSpecificFolder))
            {
                Directory.CreateDirectory(appSpecificFolder);
            }
            string localJsonPath = Path.Combine(appSpecificFolder, "maps.json");

            try
            {
                string jsonString;
                if (File.Exists(localJsonPath))
                {
                    jsonString = await File.ReadAllTextAsync(localJsonPath);
                }
                else
                {
                    jsonString = await _httpClient.GetStringAsync(Constants.MAP_JSON_URL);
                    await File.WriteAllTextAsync(localJsonPath, jsonString);
                }
                
                _mapList = JsonSerializer.Deserialize<List<MapData>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                BuildMapLookup();
            }
            catch { /* Gulp */ }
        }

        private void BuildMapLookup()
        {
            _mapLookup.Clear();
            foreach (var map in _mapList)
            {
                if (map.Name != null)
                {
                    var normalized = NormalizeMapName(map.Name);
                    if (!_mapLookup.ContainsKey(normalized))
                        _mapLookup[normalized] = map;
                }
            }
        }

        public MapData? FindBestMapMatch(string mapName)
        {
            if (string.IsNullOrEmpty(mapName) || _mapList.Count == 0) return null;

            string decodedName = System.Net.WebUtility.HtmlDecode(mapName);
            string normalized = NormalizeMapName(decodedName);

            // Priority mappings - check these first
            var priorityMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "terroristmansion", "TO-TerrorMansion" },
                { "terrorsmansion", "TO-TerrorMansion" },
                { "terrormansion", "TO-TerrorMansion" },
                { "cia", "TO-CIA" },
                { "glasgowkiss", "TO-GlasgowKiss" },
                { "avalanche", "TO-Avalanche" }
            };

            if (priorityMappings.TryGetValue(normalized, out var priorityName))
            {
                var match = _mapList.FirstOrDefault(m => m.Name != null && m.Name.Equals(priorityName, StringComparison.OrdinalIgnoreCase));
                if (match != null) return match;

                // Try without TO- prefix
                var nameWithoutPrefix = priorityName.StartsWith("TO-") ? priorityName.Substring(3) : priorityName;
                match = _mapList.FirstOrDefault(m => m.Name != null && m.Name.EndsWith(nameWithoutPrefix, StringComparison.OrdinalIgnoreCase));
                if (match != null) return match;

                // Try partial match with just the name part
                var namePart = priorityName.Replace("TO-", "").Replace("-", "");
                match = _mapList.FirstOrDefault(m => m.Name != null && m.Name.Replace("-", "").Replace("_", "").Contains(namePart, StringComparison.OrdinalIgnoreCase));
                if (match != null) return match;
            }

            // Direct lookup - normalize both sides
            var directMatch = _mapList.FirstOrDefault(m => m.Name != null && NormalizeMapName(m.Name).Equals(normalized, StringComparison.OrdinalIgnoreCase));
            if (directMatch != null) return directMatch;

            if (normalized.Length < 3) return null;

            MapData? bestMatch = null;
            int bestScore = 0;
            int bestLength = int.MaxValue;

            foreach (var map in _mapList)
            {
                if (map.Name == null) continue;

                var mapNormalized = NormalizeMapName(map.Name);
                int score = 0;

                if (mapNormalized.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                {
                    int lengthPenalty = Math.Abs(map.Name.Length - mapName.Length);
                    score = 1000000 - lengthPenalty * 1000 - map.Name.Length;
                }
                else if (mapNormalized.EndsWith(normalized, StringComparison.OrdinalIgnoreCase) && normalized.Length >= 4)
                {
                    score = 500000 - (mapNormalized.Length - normalized.Length) * 100 - map.Name.Length;
                }
                else if (normalized.EndsWith(mapNormalized, StringComparison.OrdinalIgnoreCase) && mapNormalized.Length >= 4)
                {
                    score = 400000 - (normalized.Length - mapNormalized.Length) * 100 - map.Name.Length;
                }
                else if (mapNormalized.Contains(normalized, StringComparison.OrdinalIgnoreCase) && normalized.Length >= 4)
                {
                    int matchQuality = (normalized.Length * 100) / mapNormalized.Length;
                    score = 300000 + matchQuality * 1000 - map.Name.Length;
                }
                else if (normalized.Contains(mapNormalized, StringComparison.OrdinalIgnoreCase) && mapNormalized.Length >= 4)
                {
                    int matchQuality = (mapNormalized.Length * 100) / normalized.Length;
                    score = 200000 + matchQuality * 1000 - map.Name.Length;
                }
                else if (LevenshteinDistance(mapNormalized, normalized) <= 3 && Math.Abs(mapNormalized.Length - normalized.Length) <= 3)
                {
                    score = 100000 - LevenshteinDistance(mapNormalized, normalized) * 10000 - map.Name.Length;
                }
                else
                {
                    int commonChars = CountCommonSubstring(normalized, mapNormalized);
                    if (commonChars >= Math.Min(normalized.Length, mapNormalized.Length) * 0.6 && commonChars >= 4)
                    {
                        score = commonChars * 1000 - map.Name.Length;
                    }
                }

                if (score > bestScore || (score == bestScore && map.Name.Length < bestLength))
                {
                    bestScore = score;
                    bestMatch = map;
                    bestLength = map.Name.Length;
                }
            }

            return bestScore > 0 ? bestMatch : null;
        }

        private int CountCommonSubstring(string s1, string s2)
        {
            int maxLen = 0;
            for (int i = 0; i < s1.Length; i++)
            {
                for (int j = 0; j < s2.Length; j++)
                {
                    int len = 0;
                    while (i + len < s1.Length && j + len < s2.Length &&
                           char.ToLower(s1[i + len]) == char.ToLower(s2[j + len]))
                    {
                        len++;
                    }
                    if (len > maxLen) maxLen = len;
                }
            }
            return maxLen;
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];
            for (int i = 0; i <= s1.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++) d[0, j] = j;

            for (int j = 1; j <= s2.Length; j++)
            {
                for (int i = 1; i <= s1.Length; i++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[s1.Length, s2.Length];
        }

        public string NormalizeMapName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";

            name = System.Net.WebUtility.HtmlDecode(name);

            // Remove quotes first
            name = name.Trim('\'', '"').Trim();

            // Remove text prefixes BEFORE removing special chars
            if (name.StartsWith("Code-name:", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(10).Trim();
            else if (name.StartsWith("Codename:", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(9).Trim();
            else if (name.StartsWith("2nd -W-", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(7).Trim();

            name = name.Replace("'s ", "").Replace("'s", "");

            var prefixes = new[] { "TO-", "CTF-", "DM-", "AS-", "=FoE=", "-FoE-", "@8-", "-2-", "-X-", "-x-", "2W-", "SWAT-" };
            foreach (var prefix in prefixes)
            {
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(prefix.Length);
                    break;
                }
            }

            name = name.Replace("][", "").Replace(" v", "V").Replace(" V", "V")
                      .Replace("{", "").Replace("}", "").Replace("@", "")
                      .Replace("=", "").Replace("+", "").Replace("'", "").Replace("'", "")
                      .Replace("*", "").Replace(":", "").Replace("(", "").Replace(")", "")
                      .Replace(" ", "").Replace("-", "").Replace("_", "").Trim().ToLowerInvariant();

            return name;
        }
    }
}
