#nullable enable

namespace TacticalOpsQuickJoin
{
    public class Player
    {
        public int Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public int Score { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Ping { get; set; }
        public int Team { get; set; }
    }
}
