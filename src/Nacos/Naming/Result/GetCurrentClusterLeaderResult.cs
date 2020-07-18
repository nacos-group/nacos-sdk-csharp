namespace Nacos
{
    public class GetCurrentClusterLeaderResult
    {
        public int HeartbeatDueMs { get; set; }

        public string Ip { get; set; }

        public int LeaderDueMs { get; set; }

        public string State { get; set; }

        public int Term { get; set; }

        public string VoteFor { get; set; }
    }
}
