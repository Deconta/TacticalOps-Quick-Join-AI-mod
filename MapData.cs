using System.Text.Json.Serialization;

namespace TacticalOpsQuickJoin
{
    public class MapData
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("previewSmall")]
        public string? PreviewSmall { get; set; }

        [JsonPropertyName("preview")]
        public string? Preview { get; set; }

        [JsonPropertyName("previewBig")]
        public string? PreviewBig { get; set; }
    }
}