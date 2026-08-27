using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using ExtremeRoles;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance.Il2Cpp;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class VanillaRolePlayerAssignDataProviderSelectorTests
{
    public VanillaRolePlayerAssignDataProviderSelectorTests()
    {
        MockSetupHelper.SetupCommonMocks();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();

        if (ClientOption.Instance == null)
        {
            OptionCreator.Create();
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

    [Fact]
    public void Test_DefaultVanillaRolePlayerAssignDataProvider_ReturnsDataFromGameData()
    {
        var mockPlayer = CreateMockPlayerInfo(0, "HostPlayer");
        Il2CppEnumeratorExtension.UnitTestDataOverride = new List<NetworkedPlayerInfo> { mockPlayer };

        try
        {
            var provider = new DefaultVanillaRolePlayerAssignDataProvider();
            var result = provider.Data.ToList();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(0, result[0].PlayerId);
            Assert.Equal("HostPlayer", result[0].PlayerName);
            Assert.Equal(RoleTypes.Crewmate, result[0].Role);
        }
        finally
        {
            Il2CppEnumeratorExtension.UnitTestDataOverride = null;
        }
    }

    [Fact]
    public void Test_MockVanillaRolePlayerAssignDataProvider_PadsPlayersToRequestedNumber()
    {
        var mockPlayer = CreateMockPlayerInfo(0, "HostPlayer");
        Il2CppEnumeratorExtension.UnitTestDataOverride = new List<NetworkedPlayerInfo> { mockPlayer };

        try
        {
            var option = new VanillaRolePlayerOption
            {
                MockOption = new VanillaRolePlayerMockOption(4)
            };

            var provider = new MockVanillaRolePlayerAssignDataProvider(option);
            var result = provider.Data.ToList();

            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Contains(result, p => p.PlayerId == 0 && p.PlayerName == "HostPlayer");
            Assert.Contains(result, p => p.PlayerName.StartsWith("MockPlayer_"));
        }
        finally
        {
            Il2CppEnumeratorExtension.UnitTestDataOverride = null;
        }
    }

    [Fact]
    public void Test_VanillaRolePlayerAssignDataProviderSelector_WhenMockOptionSet_QueriesMockProviderFromServiceProviderAndReturnsData()
    {
        var mockPlayer = CreateMockPlayerInfo(0, "HostPlayer");
        Il2CppEnumeratorExtension.UnitTestDataOverride = new List<NetworkedPlayerInfo> { mockPlayer };

        try
        {
            var mockOption = new VanillaRolePlayerOption { MockOption = new VanillaRolePlayerMockOption(2) };

            var services = new ServiceCollection();
            services.AddSingleton(mockOption);
            services.AddSingleton<MockVanillaRolePlayerAssignDataProvider>();
            var serviceProvider = services.BuildServiceProvider();

            var selector = new VanillaRolePlayerAssignDataProviderSelector(mockOption, serviceProvider);
            var result = selector.Data.ToList();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
        finally
        {
            Il2CppEnumeratorExtension.UnitTestDataOverride = null;
        }
    }

    [Fact]
    public void Test_VanillaRolePlayerAssignDataProviderSelector_WhenMockOptionNull_QueriesDefaultProviderFromServiceProvider()
    {
        var mockPlayer = CreateMockPlayerInfo(0, "HostPlayer");
        Il2CppEnumeratorExtension.UnitTestDataOverride = new List<NetworkedPlayerInfo> { mockPlayer };

        try
        {
            var services = new ServiceCollection();
            services.AddSingleton<DefaultVanillaRolePlayerAssignDataProvider>();
            var serviceProvider = services.BuildServiceProvider();

            var mockOptionNull = new VanillaRolePlayerOption { MockOption = null };
            var selector = new VanillaRolePlayerAssignDataProviderSelector(mockOptionNull, serviceProvider);

            var result = selector.Data.ToList();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("HostPlayer", result[0].PlayerName);
        }
        finally
        {
            Il2CppEnumeratorExtension.UnitTestDataOverride = null;
        }
    }
}
