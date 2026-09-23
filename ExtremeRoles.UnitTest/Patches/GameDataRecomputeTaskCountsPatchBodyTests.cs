using System;
using System.Collections.Generic;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Patches;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Patches;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class GameDataRecomputeTaskCountsPatchBodyTests : IDisposable
{
	private sealed class DummyRole : SingleRoleBase
	{
		public DummyRole(RoleArgs args) : base(args) { }

		public override Color GetNameColor(bool isDead) => Color.white;
		public override string GetColoredRoleName(bool isDead = false) => "Dummy";
		public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId) => "";
		public override Color GetTargetRoleSeeColor(SingleRoleBase targetRole, byte targetPlayerId) => Color.white;

		protected override void RoleSpecificInit() { }
		protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory parentOps) { }
	}

	public GameDataRecomputeTaskCountsPatchBodyTests()
	{
		ResetState();
	}

	public void Dispose()
	{
		ResetState();
	}

	private static void ResetState()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupTimeHelpers();
		MockSetupHelper.SetupPlayerControlMocks();
		ExtremeRoleManager.GameRole.Clear();
	}

	[Fact]
	public void IsDisableTaskWin_WhenGameDataInstanceNull_ReturnsTrue()
	{
		var mockGameDataHelper = new Mock<MockGameDataget_InstanceHelper>();
		mockGameDataHelper.Setup(h => h.Invoke()).Returns((GameData)null!);
		MockGameDataget_InstanceHelper.Instance = mockGameDataHelper.Object;

		var result = GameDataRecomputeTaskCountsPatchBody.IsDisableTaskWin;

		Assert.True(result);
	}

	[Fact]
	public void IsDisableTaskWin_WhenForceDisableTaskNumAndZeroCompletedTasks_ReturnsTrue()
	{
		var mockGameData = new Mock<GameData>(IntPtr.Zero);
		mockGameData.SetupGet(g => g.TotalTasks).Returns(88659);
		mockGameData.SetupGet(g => g.CompletedTasks).Returns(0);

		var mockGameDataHelper = new Mock<MockGameDataget_InstanceHelper>();
		mockGameDataHelper.Setup(h => h.Invoke()).Returns(mockGameData.Object);
		MockGameDataget_InstanceHelper.Instance = mockGameDataHelper.Object;

		var result = GameDataRecomputeTaskCountsPatchBody.IsDisableTaskWin;

		Assert.True(result);
	}

	[Fact]
	public void IsDisableTaskWin_WhenNormalTaskCounts_ReturnsFalse()
	{
		var mockGameData = new Mock<GameData>(IntPtr.Zero);
		mockGameData.SetupGet(g => g.TotalTasks).Returns(10);
		mockGameData.SetupGet(g => g.CompletedTasks).Returns(5);

		var mockGameDataHelper = new Mock<MockGameDataget_InstanceHelper>();
		mockGameDataHelper.Setup(h => h.Invoke()).Returns(mockGameData.Object);
		MockGameDataget_InstanceHelper.Instance = mockGameDataHelper.Object;

		var result = GameDataRecomputeTaskCountsPatchBody.IsDisableTaskWin;

		Assert.False(result);
	}

	[Fact]
	public void IsDisableTaskWin_WhenForceDisableTaskNumButCompletedTasksNotZero_ReturnsFalse()
	{
		var mockGameData = new Mock<GameData>(IntPtr.Zero);
		mockGameData.SetupGet(g => g.TotalTasks).Returns(88659);
		mockGameData.SetupGet(g => g.CompletedTasks).Returns(1);

		var mockGameDataHelper = new Mock<MockGameDataget_InstanceHelper>();
		mockGameDataHelper.Setup(h => h.Invoke()).Returns(mockGameData.Object);
		MockGameDataget_InstanceHelper.Instance = mockGameDataHelper.Object;

		var result = GameDataRecomputeTaskCountsPatchBody.IsDisableTaskWin;

		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenIsGameNowIsFalse_ReturnsTrue()
	{
		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(false);

		var mockRuntime = new Mock<IGameRuntime>();
		var mockGameData = new Mock<GameData>(IntPtr.Zero);

		var patchBody = new GameDataRecomputeTaskCountsPatchBody(mockProgress.Object, mockRuntime.Object);

		var result = patchBody.Prefix(mockGameData.Object);

		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenTryGetGameContextReturnsFalse_ReturnsTrue()
	{
		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(false);

		var mockGameData = new Mock<GameData>(IntPtr.Zero);

		var patchBody = new GameDataRecomputeTaskCountsPatchBody(mockProgress.Object, mockRuntime.Object);

		var result = patchBody.Prefix(mockGameData.Object);

		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenRolesCountZero_SetsForceDisableTaskNumAndReturnsFalse()
	{
		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockContext = new Mock<IGameContext>();
		var mockRoles = new Mock<INomalGameRoleContainer>();
		mockRoles.SetupGet(r => r.All).Returns(new Dictionary<byte, SingleRoleBase>());
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockOption = new Mock<IShipGlobalOption>();
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockOption.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var mockGameData = new Mock<GameData>(IntPtr.Zero);

		var patchBody = new GameDataRecomputeTaskCountsPatchBody(mockProgress.Object, mockRuntime.Object);

		var result = patchBody.Prefix(mockGameData.Object);

		Assert.False(result);
		mockGameData.VerifySet(g => g.TotalTasks = 88659, Times.Once);
		mockGameData.VerifySet(g => g.CompletedTasks = 0, Times.Once);
	}

	[Fact]
	public void Prefix_WhenDisableTaskWinAndDisableTaskWinWhenNoneTaskCrewBothTrue_SetsForceDisableTaskNumAndReturnsFalse()
	{
		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var crewRole = new DummyRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
		var mockContext = new Mock<IGameContext>();
		var mockRoles = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, crewRole } };
		mockRoles.SetupGet(r => r.All).Returns(rolesDict);
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockOption = new Mock<IShipGlobalOption>();
		mockOption.SetupGet(o => o.DisableTaskWin).Returns(true);
		mockOption.SetupGet(o => o.DisableTaskWinWhenNoneTaskCrew).Returns(true);
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockOption.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		var mockGameData = new Mock<GameData>(IntPtr.Zero);

		var patchBody = new GameDataRecomputeTaskCountsPatchBody(mockProgress.Object, mockRuntime.Object);

		var result = patchBody.Prefix(mockGameData.Object);

		Assert.False(result);
		mockGameData.VerifySet(g => g.TotalTasks = 88659, Times.Once);
		mockGameData.VerifySet(g => g.CompletedTasks = 0, Times.Once);
	}

	[Fact]
	public void Prefix_WhenValidCrewmatesWithTasks_ComputesTaskCountsCorrectly()
	{
		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var crewRole = new DummyRole(RoleArgs.BuildCrewmate(ExtremeRoleId.Sheriff, Color.white));
		var mockContext = new Mock<IGameContext>();
		var mockRoles = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, crewRole } };
		mockRoles.SetupGet(r => r.All).Returns(rolesDict);
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockOption = new Mock<IShipGlobalOption>();
		mockOption.SetupGet(o => o.DisableTaskWin).Returns(false);
		mockOption.SetupGet(o => o.DisableTaskWinWhenNoneTaskCrew).Returns(false);
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockOption.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		byte playerId = 0;
		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[playerId] = crewRole;

		var mockRoleInfo = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleInfo.SetupGet(r => r.TasksCountTowardProgress).Returns(true);

		var mockPlayerControl = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayerControl.SetupGet(p => p.PlayerId).Returns(playerId);

		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockPlayerInfo.SetupGet(p => p.PlayerId).Returns(playerId);
		mockPlayerInfo.SetupGet(p => p.Disconnected).Returns(false);
		mockPlayerInfo.SetupGet(p => p.IsDead).Returns(false);
		mockPlayerInfo.SetupGet(p => p.Role).Returns(mockRoleInfo.Object);
		mockPlayerInfo.SetupGet(p => p.Object).Returns(mockPlayerControl.Object);

		var t1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		t1.SetupGet(t => t.Complete).Returns(true);
		var t2 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		t2.SetupGet(t => t.Complete).Returns(false);

		var mockTaskList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		mockTaskList.SetupGet(l => l.Count).Returns(2);
		mockTaskList.SetupGet(l => l[0]).Returns(t1.Object);
		mockTaskList.SetupGet(l => l[1]).Returns(t2.Object);
		mockPlayerInfo.SetupGet(p => p.Tasks).Returns(mockTaskList.Object);

		var mockGameData = new Mock<GameData>(IntPtr.Zero);
		var mockPlayerList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockPlayerList.SetupGet(l => l.Count).Returns(1);
		mockPlayerList.SetupGet(l => l[0]).Returns(mockPlayerInfo.Object);
		mockGameData.SetupGet(g => g.AllPlayers).Returns(mockPlayerList.Object);

		var patchBody = new GameDataRecomputeTaskCountsPatchBody(mockProgress.Object, mockRuntime.Object);

		var result = patchBody.Prefix(mockGameData.Object);

		Assert.False(result);
		mockGameData.VerifySet(g => g.TotalTasks = 2, Times.Once);
		mockGameData.VerifySet(g => g.CompletedTasks = 1, Times.Once);
	}

	[Fact]
	public void Prefix_WhenNoTaskCrewmatesAndDisableTaskWinWhenNoneTaskCrewIsTrue_SetsForceDisableTaskNum()
	{
		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var impRole = new DummyRole(RoleArgs.BuildImpostor(ExtremeRoleId.Assassin));
		var mockContext = new Mock<IGameContext>();
		var mockRoles = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, impRole } };
		mockRoles.SetupGet(r => r.All).Returns(rolesDict);
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockOption = new Mock<IShipGlobalOption>();
		mockOption.SetupGet(o => o.DisableTaskWin).Returns(false);
		mockOption.SetupGet(o => o.DisableTaskWinWhenNoneTaskCrew).Returns(true);
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockOption.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		byte playerId = 0;
		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[playerId] = impRole;

		var mockRoleInfo = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleInfo.SetupGet(r => r.TasksCountTowardProgress).Returns(true);

		var mockPlayerControl = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayerControl.SetupGet(p => p.PlayerId).Returns(playerId);

		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockPlayerInfo.SetupGet(p => p.PlayerId).Returns(playerId);
		mockPlayerInfo.SetupGet(p => p.Disconnected).Returns(false);
		mockPlayerInfo.SetupGet(p => p.IsDead).Returns(false);
		mockPlayerInfo.SetupGet(p => p.Role).Returns(mockRoleInfo.Object);
		mockPlayerInfo.SetupGet(p => p.Object).Returns(mockPlayerControl.Object);

		var mockTaskList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		mockTaskList.SetupGet(l => l.Count).Returns(0);
		mockPlayerInfo.SetupGet(p => p.Tasks).Returns(mockTaskList.Object);

		var mockGameData = new Mock<GameData>(IntPtr.Zero);
		var mockPlayerList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockPlayerList.SetupGet(l => l.Count).Returns(1);
		mockPlayerList.SetupGet(l => l[0]).Returns(mockPlayerInfo.Object);
		mockGameData.SetupGet(g => g.AllPlayers).Returns(mockPlayerList.Object);

		var patchBody = new GameDataRecomputeTaskCountsPatchBody(mockProgress.Object, mockRuntime.Object);

		var result = patchBody.Prefix(mockGameData.Object);

		Assert.False(result);
		mockGameData.VerifySet(g => g.TotalTasks = 88659, Times.Once);
		mockGameData.VerifySet(g => g.CompletedTasks = 0, Times.Once);
	}

	[Fact]
	public void Prefix_WhenNoTaskCrewmatesAndDisableTaskWinWhenNoneTaskCrewIsFalse_SetsTotalAndCompletedToZero()
	{
		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var impRole = new DummyRole(RoleArgs.BuildImpostor(ExtremeRoleId.Assassin));
		var mockContext = new Mock<IGameContext>();
		var mockRoles = new Mock<INomalGameRoleContainer>();
		var rolesDict = new Dictionary<byte, SingleRoleBase> { { 0, impRole } };
		mockRoles.SetupGet(r => r.All).Returns(rolesDict);
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockOption = new Mock<IShipGlobalOption>();
		mockOption.SetupGet(o => o.DisableTaskWin).Returns(false);
		mockOption.SetupGet(o => o.DisableTaskWinWhenNoneTaskCrew).Returns(false);
		mockContext.SetupGet(c => c.GlobalOption).Returns(mockOption.Object);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? ctx = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out ctx)).Returns(true);

		byte playerId = 0;
		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[playerId] = impRole;

		var mockRoleInfo = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleInfo.SetupGet(r => r.TasksCountTowardProgress).Returns(true);

		var mockPlayerControl = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayerControl.SetupGet(p => p.PlayerId).Returns(playerId);

		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockPlayerInfo.SetupGet(p => p.PlayerId).Returns(playerId);
		mockPlayerInfo.SetupGet(p => p.Disconnected).Returns(false);
		mockPlayerInfo.SetupGet(p => p.IsDead).Returns(false);
		mockPlayerInfo.SetupGet(p => p.Role).Returns(mockRoleInfo.Object);
		mockPlayerInfo.SetupGet(p => p.Object).Returns(mockPlayerControl.Object);

		var mockTaskList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		mockTaskList.SetupGet(l => l.Count).Returns(0);
		mockPlayerInfo.SetupGet(p => p.Tasks).Returns(mockTaskList.Object);

		var mockGameData = new Mock<GameData>(IntPtr.Zero);
		var mockPlayerList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockPlayerList.SetupGet(l => l.Count).Returns(1);
		mockPlayerList.SetupGet(l => l[0]).Returns(mockPlayerInfo.Object);
		mockGameData.SetupGet(g => g.AllPlayers).Returns(mockPlayerList.Object);

		var patchBody = new GameDataRecomputeTaskCountsPatchBody(mockProgress.Object, mockRuntime.Object);

		var result = patchBody.Prefix(mockGameData.Object);

		Assert.False(result);
		mockGameData.VerifySet(g => g.TotalTasks = 0, Times.Once);
		mockGameData.VerifySet(g => g.CompletedTasks = 0, Times.Once);
	}
}
