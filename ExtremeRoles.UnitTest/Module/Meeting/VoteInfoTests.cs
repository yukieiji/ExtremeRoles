using ExtremeRoles.Module.Meeting;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Meeting;

public class VoteInfoTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        byte voterId = 1;
        byte targetId = 2;
        int count = 3;

        var voteInfo = new VoteInfo(voterId, targetId, count);

        Assert.Equal(voterId, voteInfo.VoterId);
        Assert.Equal(targetId, voteInfo.TargetId);
        Assert.Equal(count, voteInfo.Count);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var voteInfo1 = new VoteInfo(1, 2, 5);
        var voteInfo2 = new VoteInfo(1, 2, 5);

        Assert.Equal(voteInfo1, voteInfo2);
        Assert.True(voteInfo1 == voteInfo2);
        Assert.False(voteInfo1 != voteInfo2);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var voteInfo1 = new VoteInfo(1, 2, 5);
        var voteInfo2 = new VoteInfo(1, 3, 5);

        Assert.NotEqual(voteInfo1, voteInfo2);
        Assert.False(voteInfo1 == voteInfo2);
        Assert.True(voteInfo1 != voteInfo2);
    }
}
