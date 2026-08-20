using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Helper;

public class PlayerTests
{
    [Fact]
    public void GetPlayerControlById_WithMatchingPlayerInCache_ShouldReturnPlayer()
    {
        PlayerCache.AllPlayerControl.Clear();

        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)5);

        PlayerCache.AllPlayerControl.Add(mockPlayer.Object);

        var result = Player.GetPlayerControlById(5);

        Assert.NotNull(result);
        Assert.Equal((byte)5, result.PlayerId);

        PlayerCache.AllPlayerControl.Clear();
    }

    [Fact]
    public void GetPlayerControlById_WhenPlayerNotFound_ShouldReturnNull()
    {
        PlayerCache.AllPlayerControl.Clear();

        var mockPlayer = new Mock<PlayerControl>();
        mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)5);

        PlayerCache.AllPlayerControl.Add(mockPlayer.Object);

        var result = Player.GetPlayerControlById(99);

        Assert.Null(result);

        PlayerCache.AllPlayerControl.Clear();
    }
}
