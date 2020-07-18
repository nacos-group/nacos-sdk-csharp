namespace Nacos
{
    public class ClusterLeader
    {
        public int HeartbeatDueMs { get; set; }

        public string Ip { get; set; }

        public int LeaderDueMs { get; set; }

        /// <summary>
        /// 1. LEADER     Leader of the cluster, only one leader stands in a cluster
        /// 2. FOLLOWER   Follower of the cluster, report to and copy from leader
        /// 3. CANDIDATE  Candidate leader to be elected
        /// </summary>
        public string State { get; set; }

        public int Term { get; set; }

        public string VoteFor { get; set; }
    }
}
