using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using ExtremeRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class VanillaRolePlayerAssignDataProviderSelectorTests
{
    public VanillaRolePlayerAssignDataProviderSelectorTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();

        if (ClientOption.Instance == null)
        {
            OptionCreator.Create();
            OptionManager.Load();
        }

        var mockOptions = new Mock<IGameOptions>(System.IntPtr.Zero);
        mockOptions.SetupGet(o => o.NumImpostors).Returns(1);
        mockOptions.SetupGet(o => o.GameMode).Returns(GameModes.HideNSeek);

        var mockOptionsMgr = new Mock<GameOptionsManager>(System.IntPtr.Zero);
        mockOptionsMgr.SetupGet(m => m.currentGameOptions).Returns(mockOptions.Object);

        var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
        mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockOptionsMgr.Object);
        MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
    }

    private static NetworkedPlayerInfo CreateMockPlayerInfo(byte playerId, string name, RoleTypes roleType = RoleTypes.Crewmate)
    {
        var mockPlayer = new Mock<NetworkedPlayerInfo>(System.IntPtr.Zero);
        mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

        var mockOutfit = new Mock<NetworkedPlayerInfo.PlayerOutfit>(System.IntPtr.Zero);
        mockOutfit.SetupGet(o => o.PlayerName).Returns(name);
        mockPlayer.SetupGet(p => p.DefaultOutfit).Returns(mockOutfit.Object);

        var mockRole = new Mock<RoleBehaviour>(System.IntPtr.Zero);
        mockRole.SetupGet(r => r.Role).Returns(roleType);
        mockPlayer.SetupGet(p => p.Role).Returns(mockRole.Object);

        return mockPlayer.Object;
    }

    private static void SetGameDataPlayers(List<NetworkedPlayerInfo> players)
    {
        var mockGameData = MockSetupHelper.SetupGameDataMock();
        var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(System.IntPtr.Zero);
        mockList.SetupGet(l => l.Count).Returns(players.Count);
        mockList.Setup(l => l[It.IsAny<int>()]).Returns((int index) => players[index]);

        mockGameData.SetupGet(g => g.AllPlayers).Returns(mockList.Object);
    }

    [Fact]
    public void Test_DefaultVanillaRolePlayerAssignDataProvider_SinglePlayer()
    {
        var mockPlayer = CreateMockPlayerInfo(0, "HostPlayer");
        SetGameDataPlayers(new List<NetworkedPlayerInfo> { mockPlayer });

        var provider = new DefaultVanillaRolePlayerAssignDataProvider();
        var result = provider.Data.ToList();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(0, result[0].PlayerId);
        Assert.Equal("HostPlayer", result[0].PlayerName);
        Assert.Equal(RoleTypes.Crewmate, result[0].Role);
    }

    [Fact]
    public void Test_DefaultVanillaRolePlayerAssignDataProvider_MultiplePlayers()
    {
        var player1 = CreateMockPlayerInfo(0, "Player1", RoleTypes.Crewmate);
        var player2 = CreateMockPlayerInfo(1, "Player2", RoleTypes.Impostor);
        var player3 = CreateMockPlayerInfo(2, "Player3", RoleTypes.Scientist);
        SetGameDataPlayers(new List<NetworkedPlayerInfo> { player1, player2, player3 });

        var provider = new DefaultVanillaRolePlayerAssignDataProvider();
        var result = provider.Data.ToList();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal(0, result[0].PlayerId);
        Assert.Equal("Player1", result[0].PlayerName);
        Assert.Equal(1, result[1].PlayerId);
        Assert.Equal("Player2", result[1].PlayerName);
        Assert.Equal(2, result[2].PlayerId);
        Assert.Equal("Player3", result[2].PlayerName);
    }

    [Fact]
    public void Test_MockVanillaRolePlayerAssignDataProvider_WhenLobbyCountLessThanRequested_PadsPlayers()
    {
        var player1 = CreateMockPlayerInfo(0, "HostPlayer");
        var player2 = CreateMockPlayerInfo(1, "Player1");
        SetGameDataPlayers(new List<NetworkedPlayerInfo> { player1, player2 });

        var option = new VanillaRolePlayerOption
        {
            MockOption = new VanillaRolePlayerMockOption(5)
        };

        var provider = new MockVanillaRolePlayerAssignDataProvider(option);
        var result = provider.Data.ToList();

        Assert.NotNull(result);
        Assert.Equal(5, result.Count);
        Assert.Contains(result, p => p.PlayerId == 0 && p.PlayerName == "HostPlayer");
        Assert.Contains(result, p => p.PlayerId == 1 && p.PlayerName == "Player1");
        Assert.Equal(3, result.Count(p => p.PlayerName.StartsWith("MockPlayer_")));
    }

    [Fact]
    public void Test_MockVanillaRolePlayerAssignDataProvider_WhenLobbyCountGreaterThanRequested_TrimsPlayers()
    {
        var player1 = CreateMockPlayerInfo(0, "HostPlayer");
        var player2 = CreateMockPlayerInfo(1, "Player1");
        var player3 = CreateMockPlayerInfo(2, "Player2");
        SetGameDataPlayers(new List<NetworkedPlayerInfo> { player1, player2, player3 });

        var option = new VanillaRolePlayerOption
        {
            MockOption = new VanillaRolePlayerMockOption(2)
        };

        var provider = new MockVanillaRolePlayerAssignDataProvider(option);
        var result = provider.Data.ToList();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, p => p.PlayerName.StartsWith("MockPlayer_"));
    }

    [Fact]
    public void Test_MockVanillaRolePlayerAssignDataProvider_WhenLobbyCountEqualsRequested_ReturnsExactPlayers()
    {
        var player1 = CreateMockPlayerInfo(0, "HostPlayer");
        var player2 = CreateMockPlayerInfo(1, "Player1");
        var player3 = CreateMockPlayerInfo(2, "Player2");
        SetGameDataPlayers(new List<NetworkedPlayerInfo> { player1, player2, player3 });

        var option = new VanillaRolePlayerOption
        {
            MockOption = new VanillaRolePlayerMockOption(3)
        };

        var provider = new MockVanillaRolePlayerAssignDataProvider(option);
        var result = provider.Data.ToList();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, p => p.PlayerName.StartsWith("MockPlayer_"));
    }

    [Fact]
    public void Test_VanillaRolePlayerAssignDataProviderSelector_WhenMockOptionSet_QueriesMockProviderFromServiceProviderAndReturnsData()
    {
        var player1 = CreateMockPlayerInfo(0, "HostPlayer");
        var player2 = CreateMockPlayerInfo(1, "Player1");
        SetGameDataPlayers(new List<NetworkedPlayerInfo> { player1, player2 });

        var mockOption = new VanillaRolePlayerOption { MockOption = new VanillaRolePlayerMockOption(4) };

        var services = new ServiceCollection();
        services.AddSingleton(mockOption);
        services.AddSingleton<MockVanillaRolePlayerAssignDataProvider>();
        var serviceProvider = services.BuildServiceProvider();

        var selector = new VanillaRolePlayerAssignDataProviderSelector(mockOption, serviceProvider);
        var result = selector.Data.ToList();

        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void Test_VanillaRolePlayerAssignDataProviderSelector_WhenMockOptionNull_QueriesDefaultProviderFromServiceProvider()
    {
        var player1 = CreateMockPlayerInfo(0, "HostPlayer");
        var player2 = CreateMockPlayerInfo(1, "Player1");
        SetGameDataPlayers(new List<NetworkedPlayerInfo> { player1, player2 });

        var services = new ServiceCollection();
        services.AddSingleton<DefaultVanillaRolePlayerAssignDataProvider>();
        var serviceProvider = services.BuildServiceProvider();

        var mockOptionNull = new VanillaRolePlayerOption { MockOption = null };
        var selector = new VanillaRolePlayerAssignDataProviderSelector(mockOptionNull, serviceProvider);

        var result = selector.Data.ToList();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("HostPlayer", result[0].PlayerName);
        Assert.Equal("Player1", result[1].PlayerName);
    }
}
