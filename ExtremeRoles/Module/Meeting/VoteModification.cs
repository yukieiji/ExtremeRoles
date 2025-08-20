namespace ExtremeRoles.Module.Meeting
{
    public readonly struct VoteModification
    {
        public VoteModification(byte voterId, byte targetId, int voteCount)
        {
            VoterId = voterId;
            TargetId = targetId;
            VoteCount = voteCount;
        }

        public byte VoterId { get; }
        public byte TargetId { get; }
        public int VoteCount { get; }
    }
}
