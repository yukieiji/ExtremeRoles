using System.Collections.Generic;

namespace ExtremeRoles.Module.Meeting
{
    public sealed class VoteCollection
    {
        private readonly List<VoteModification> _votes = new List<VoteModification>();

        public void Add(VoteModification vote)
        {
            _votes.Add(vote);
        }

        public void AddRange(IEnumerable<VoteModification> votes)
        {
            _votes.AddRange(votes);
        }

        public IReadOnlyList<VoteModification> GetAllVotes()
        {
            return _votes;
        }

    }
}
