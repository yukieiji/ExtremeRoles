using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.GameResult.WinnerProcessor;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameResult.WinnerProcessor;

[Collection("UnityMock")]
public class WinnerProcessorTests
{
    private sealed class DummySingleRole : SingleRoleBase
    {
        public DummySingleRole(RoleCore core, int gameControlId = 0)
        {
            var field = typeof(SingleRoleBase).GetField("<Core>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(this, core);
            this.SetControlId(gameControlId);
        }

        protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory) { }
        protected override void RoleSpecificInit() { }
    }

    public WinnerProcessorTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger("WinnerProcessorTests");
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        if (ExtremeRolesPlugin.ShipState == null)
        {
            var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty("ShipState", BindingFlags.Public | BindingFlags.Static);
            shipStateProp?.SetValue(null, new ExtremeShipStatus());
        }

        var debugConfig = new ConfigFile(Path.Combine(Path.GetTempPath(), "test_debug.cfg"), true);
        var debugEntry = debugConfig.Bind("Debug", "DebugMode", false);
        var debugProp = typeof(ExtremeRolesPlugin).GetProperty("DebugMode", BindingFlags.Public | BindingFlags.Static);
        debugProp?.SetValue(null, debugEntry);
    }

    private static NetworkedPlayerInfo CreateMockPlayerInfo(byte playerId, string name = "Player")
    {
        var mockPlayer = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
        mockPlayer.SetupGet(p => p.PlayerName).Returns($"{name}{playerId}");
        return mockPlayer.Object;
    }

    private static CachedPlayerData CreateMockCachedPlayerData(string playerName)
    {
        var mock = new Mock<CachedPlayerData>(IntPtr.Zero);
        mock.SetupGet(c => c.PlayerName).Returns(playerName);
        return mock.Object;
    }

    private static void AddMockPlayerToPool(WinnerContainer container, NetworkedPlayerInfo player)
    {
        var poolField = typeof(WinnerContainer).GetField("allWinnerPool", BindingFlags.NonPublic | BindingFlags.Instance);
        var pool = (System.Collections.Generic.Dictionary<byte, CachedPlayerData>)poolField!.GetValue(container)!;
        pool[player.PlayerId] = CreateMockCachedPlayerData(player.PlayerName);
    }

    private static SingleRoleBase CreateMockRole(ExtremeRoleId roleId, int gameControlId = 1)
    {
        var core = new RoleCore(roleId, ExtremeRoleType.Neutral, Color.white, "TestRole");
        return new DummySingleRole(core, gameControlId);
    }

    [Fact]
    public void RemoveAddPlusWinnerProcessor_RemovesPlusWinnerFromFinalWinners()
    {
        var container = new WinnerContainer();
        var plusPlayer = CreateMockPlayerInfo(1, "Plus");
        AddMockPlayerToPool(container, plusPlayer);
        container.AddPlusWinner(plusPlayer);
        container.Add(plusPlayer);

        var processor = new RemoveAddPlusWinnerProcessor();
        var state = new WinnerState([], [], []);

        processor.Process(container, state);

        var finalField = typeof(WinnerContainer).GetField("finalWinPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
        var final = (System.Collections.Generic.List<CachedPlayerData>)finalField!.GetValue(container)!;
        Assert.Empty(final);
    }

    [Fact]
    public void AddGhostRoleWinnerProcessor_GhostRoleWins_AddsPlayerToWinner()
    {
        var container = new WinnerContainer();
        var ghostPlayer = CreateMockPlayerInfo(2, "Ghost");
        AddMockPlayerToPool(container, ghostPlayer);

        var mockGhostRole = new Mock<IGhostRoleWinable>();
        mockGhostRole.Setup(g => g.IsWin(It.IsAny<GameOverReason>(), ghostPlayer)).Returns(true);

        var ghostInfo = new GhostRoleWinInfo(ghostPlayer, mockGhostRole.Object);
        var state = new WinnerState([], [], [ghostInfo]);

        var processor = new AddGhostRoleWinnerProcessor();
        processor.Process(container, state);

        Assert.Contains(ghostPlayer, container.PlusedWinner);
    }

    [Fact]
    public void AddGhostRoleWinnerProcessor_GhostRoleDoesNotWin_DoesNotAddPlayer()
    {
        var container = new WinnerContainer();
        var ghostPlayer = CreateMockPlayerInfo(2, "Ghost");
        AddMockPlayerToPool(container, ghostPlayer);

        var mockGhostRole = new Mock<IGhostRoleWinable>();
        mockGhostRole.Setup(g => g.IsWin(It.IsAny<GameOverReason>(), ghostPlayer)).Returns(false);

        var ghostInfo = new GhostRoleWinInfo(ghostPlayer, mockGhostRole.Object);
        var state = new WinnerState([], [], [ghostInfo]);

        var processor = new AddGhostRoleWinnerProcessor();
        processor.Process(container, state);

        Assert.DoesNotContain(ghostPlayer, container.PlusedWinner);
    }

    [Fact]
    public void MergeWinnerProcessor_MergesPlusWinnerIntoFinalWinner()
    {
        var container = new WinnerContainer();
        var plusPlayer = CreateMockPlayerInfo(3, "Plus");
        AddMockPlayerToPool(container, plusPlayer);
        container.AddPlusWinner(plusPlayer);

        var processor = new MergeWinnerProcessor();
        var state = new WinnerState([], [], []);

        processor.Process(container, state);

        Assert.Contains(plusPlayer, container.PlusedWinner);
    }

    [Fact]
    public void ModifiedWinnerProcessor_CallsModifiedWinPlayerOnModRoles()
    {
        var container = new WinnerContainer();
        var modPlayer = CreateMockPlayerInfo(4, "Mod");
        var mockModRole = new Mock<IRoleWinPlayerModifier>();

        var modInfo = new WinModRoleInfo(modPlayer, mockModRole.Object);
        var state = new WinnerState([], [modInfo], []);

        var processor = new ModifiedWinnerProcessor();
        processor.Process(container, state);

        mockModRole.Verify(m => m.ModifiedWinPlayer(modPlayer, It.IsAny<GameOverReason>(), container), Times.Once);
    }

    [Fact]
    public void ReplaceWinnerProcessor_JackalKillAllOther_ClearsWinnerContainer()
    {
        ExtremeRolesPlugin.ShipState.SetGameOverReason((GameOverReason)RoleGameOverReason.JackalKillAllOther);

        var container = new WinnerContainer();
        var player = CreateMockPlayerInfo(1);
        AddMockPlayerToPool(container, player);
        container.Add(player);

        var processor = new ReplaceWinnerProcessor(1);
        var neutralRoleInfo = new NeutralRoleInfo(player, CreateMockRole(ExtremeRoleId.Jackal, 1));
        var state = new WinnerState([neutralRoleInfo], [], []);

        processor.Process(container, state);

        var finalField = typeof(WinnerContainer).GetField("finalWinPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
        var final = (System.Collections.Generic.List<CachedPlayerData>)finalField!.GetValue(container)!;
        Assert.NotEmpty(final);
    }

    [Fact]
    public void AddNeutralWinnerProcessor_Process_RunsWithoutException()
    {
        var container = new WinnerContainer();
        var player = CreateMockPlayerInfo(1);
        AddMockPlayerToPool(container, player);

        var role = CreateMockRole(ExtremeRoleId.Jester);
        role.IsWin = true;

        var neutralInfo = new NeutralRoleInfo(player, role);
        var state = new WinnerState([neutralInfo], [], []);

        var processor = new AddNeutralWinnerProcessor();
        processor.Process(container, state);

        Assert.NotNull(processor);
    }
}
