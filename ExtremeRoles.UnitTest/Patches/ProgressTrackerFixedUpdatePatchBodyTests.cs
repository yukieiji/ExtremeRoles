using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Patches;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Patches;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ProgressTrackerFixedUpdatePatchBodyTests : IDisposable
{
	public ProgressTrackerFixedUpdatePatchBodyTests()
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
	}

	private static Agency CreateAgency(bool canSeeTaskBar)
	{
		var agency = new Agency();
		typeof(Agency).GetProperty(nameof(Agency.CanSeeTaskBar))?.SetValue(agency, canSeeTaskBar);
		return agency;
	}

	private static SlaveDriver CreateSlaveDriver(bool canSeeTaskBar)
	{
		var slaveDriver = new SlaveDriver();
		typeof(SlaveDriver).GetProperty(nameof(SlaveDriver.CanSeeTaskBar))?.SetValue(slaveDriver, canSeeTaskBar);
		return slaveDriver;
	}

	[Fact]
	public void Prefix_WhenAllNotNull_ReturnsTrue()
	{
		var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
		var mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);

		var mockGameManagerHelper = new Mock<IMockGameManagerget_Instance>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		IMockGameManagerget_Instance.Instance = mockGameManagerHelper.Object;

		MockSetupHelper.SetupPlayerControlMocks();

		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		var mockTileParent = new Mock<MeshRenderer>(IntPtr.Zero);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns(mockTileParent.Object);

		var progress = new Mock<IGameProgress>();
		var runtime = new Mock<IGameRuntime>();
		var patchBody = new ProgressTrackerFixedUpdatePatchBody(progress.Object, runtime.Object);

		bool result = patchBody.Prefix(mockProgressTracker.Object);

		Assert.True(result);
	}

	[Fact]
	public void Prefix_WhenGameManagerInstanceNull_ReturnsFalse()
	{
		var mockGameManagerHelper = new Mock<IMockGameManagerget_Instance>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns((GameManager)null!);
		IMockGameManagerget_Instance.Instance = mockGameManagerHelper.Object;

		MockSetupHelper.SetupPlayerControlMocks();

		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		var mockTileParent = new Mock<MeshRenderer>(IntPtr.Zero);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns(mockTileParent.Object);

		var progress = new Mock<IGameProgress>();
		var runtime = new Mock<IGameRuntime>();
		var patchBody = new ProgressTrackerFixedUpdatePatchBody(progress.Object, runtime.Object);

		bool result = patchBody.Prefix(mockProgressTracker.Object);

		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenLogicOptionsNull_ReturnsFalse()
	{
		var mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns((LogicOptions)null!);

		var mockGameManagerHelper = new Mock<IMockGameManagerget_Instance>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		IMockGameManagerget_Instance.Instance = mockGameManagerHelper.Object;

		MockSetupHelper.SetupPlayerControlMocks();

		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		var mockTileParent = new Mock<MeshRenderer>(IntPtr.Zero);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns(mockTileParent.Object);

		var progress = new Mock<IGameProgress>();
		var runtime = new Mock<IGameRuntime>();
		var patchBody = new ProgressTrackerFixedUpdatePatchBody(progress.Object, runtime.Object);

		bool result = patchBody.Prefix(mockProgressTracker.Object);

		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenTileParentNull_ReturnsFalse()
	{
		var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
		var mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);

		var mockGameManagerHelper = new Mock<IMockGameManagerget_Instance>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		IMockGameManagerget_Instance.Instance = mockGameManagerHelper.Object;

		MockSetupHelper.SetupPlayerControlMocks();

		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns((MeshRenderer)null!);

		var progress = new Mock<IGameProgress>();
		var runtime = new Mock<IGameRuntime>();
		var patchBody = new ProgressTrackerFixedUpdatePatchBody(progress.Object, runtime.Object);

		bool result = patchBody.Prefix(mockProgressTracker.Object);

		Assert.False(result);
	}

	[Fact]
	public void Prefix_WhenLocalPlayerNull_ReturnsFalse()
	{
		var mockLogicOptions = new Mock<LogicOptions>(IntPtr.Zero);
		var mockGameManager = new Mock<GameManager>(IntPtr.Zero);
		mockGameManager.SetupGet(g => g.LogicOptions).Returns(mockLogicOptions.Object);

		var mockGameManagerHelper = new Mock<IMockGameManagerget_Instance>();
		mockGameManagerHelper.Setup(h => h.Invoke()).Returns(mockGameManager.Object);
		IMockGameManagerget_Instance.Instance = mockGameManagerHelper.Object;

		var mockLocalHelper = new Mock<IMockPlayerControlget_LocalPlayer>();
		mockLocalHelper.Setup(h => h.Invoke()).Returns((PlayerControl)null!);
		IMockPlayerControlget_LocalPlayer.Instance = mockLocalHelper.Object;

		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		var mockTileParent = new Mock<MeshRenderer>(IntPtr.Zero);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns(mockTileParent.Object);

		var progress = new Mock<IGameProgress>();
		var runtime = new Mock<IGameRuntime>();
		var patchBody = new ProgressTrackerFixedUpdatePatchBody(progress.Object, runtime.Object);

		try
		{
			bool result = patchBody.Prefix(mockProgressTracker.Object);
			Assert.False(result);
		}
		finally
		{
			IMockPlayerControlget_LocalPlayer.Instance = null;
			MockSetupHelper.SetupPlayerControlMocks();
		}
	}

	[Fact]
	public void Postfix_WhenTryGetGameContextReturnsFalse_ReturnsEarly()
	{
		var mockProgress = new Mock<IGameProgress>();
		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = null;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(false);

		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		var mockTileParent = new Mock<MeshRenderer>(IntPtr.Zero);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns(mockTileParent.Object);

		var patchBody = new ProgressTrackerFixedUpdatePatchBody(mockProgress.Object, mockRuntime.Object);
		patchBody.Postfix(mockProgressTracker.Object);

		mockTileParent.VerifySet(t => t.enabled = It.IsAny<bool>(), Times.Never);
	}

	[Fact]
	public void Postfix_WhenIsGameNowIsFalse_ReturnsEarly()
	{
		var mockContext = new Mock<IGameContext>();
		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(false);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		var mockTileParent = new Mock<MeshRenderer>(IntPtr.Zero);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns(mockTileParent.Object);

		var patchBody = new ProgressTrackerFixedUpdatePatchBody(mockProgress.Object, mockRuntime.Object);
		patchBody.Postfix(mockProgressTracker.Object);

		mockTileParent.VerifySet(t => t.enabled = It.IsAny<bool>(), Times.Never);
	}

	[Fact]
	public void Postfix_WhenNeitherAgencyNorSlaveDriver_ReturnsEarly()
	{
		var mockRoles = new Mock<INomalGameRoleContainer>();
		Agency? agency = null;
		SlaveDriver? slaveDriver = null;
		mockRoles.Setup(r => r.TryGetSafeCastedLocalRole<Agency>(out agency)).Returns(false);
		mockRoles.Setup(r => r.TryGetSafeCastedLocalRole<SlaveDriver>(out slaveDriver)).Returns(false);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		var mockTileParent = new Mock<MeshRenderer>(IntPtr.Zero);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns(mockTileParent.Object);

		var patchBody = new ProgressTrackerFixedUpdatePatchBody(mockProgress.Object, mockRuntime.Object);
		patchBody.Postfix(mockProgressTracker.Object);

		mockTileParent.VerifySet(t => t.enabled = It.IsAny<bool>(), Times.Never);
	}

	[Fact]
	public void Postfix_WhenAgencyCanSeeTaskBarIsFalse_ReturnsEarly()
	{
		var agencyInstance = CreateAgency(canSeeTaskBar: false);
		var mockRoles = new Mock<INomalGameRoleContainer>();
		Agency? agency = agencyInstance;
		SlaveDriver? slaveDriver = null;
		mockRoles.Setup(r => r.TryGetSafeCastedLocalRole<Agency>(out agency)).Returns(true);
		mockRoles.Setup(r => r.TryGetSafeCastedLocalRole<SlaveDriver>(out slaveDriver)).Returns(false);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		var mockTileParent = new Mock<MeshRenderer>(IntPtr.Zero);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns(mockTileParent.Object);

		var patchBody = new ProgressTrackerFixedUpdatePatchBody(mockProgress.Object, mockRuntime.Object);
		patchBody.Postfix(mockProgressTracker.Object);

		mockTileParent.VerifySet(t => t.enabled = It.IsAny<bool>(), Times.Never);
	}

	[Fact]
	public void Postfix_WhenSlaveDriverCanSeeTaskBarIsFalse_ReturnsEarly()
	{
		var slaveDriverInstance = CreateSlaveDriver(canSeeTaskBar: false);
		var mockRoles = new Mock<INomalGameRoleContainer>();
		Agency? agency = null;
		SlaveDriver? slaveDriver = slaveDriverInstance;
		mockRoles.Setup(r => r.TryGetSafeCastedLocalRole<Agency>(out agency)).Returns(false);
		mockRoles.Setup(r => r.TryGetSafeCastedLocalRole<SlaveDriver>(out slaveDriver)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		var mockTileParent = new Mock<MeshRenderer>(IntPtr.Zero);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns(mockTileParent.Object);

		var patchBody = new ProgressTrackerFixedUpdatePatchBody(mockProgress.Object, mockRuntime.Object);
		patchBody.Postfix(mockProgressTracker.Object);

		mockTileParent.VerifySet(t => t.enabled = It.IsAny<bool>(), Times.Never);
	}

	[Fact]
	public void Postfix_WhenGameDataInstanceNull_EnablesTileParentAndReturnsEarly()
	{
		var agencyInstance = CreateAgency(canSeeTaskBar: true);
		var mockRoles = new Mock<INomalGameRoleContainer>();
		Agency? agency = agencyInstance;
		mockRoles.Setup(r => r.TryGetSafeCastedLocalRole<Agency>(out agency)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		var mockTileParent = new Mock<MeshRenderer>(IntPtr.Zero);
		mockTileParent.SetupGet(t => t.enabled).Returns(false);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns(mockTileParent.Object);

		var mockGameDataHelper = new Mock<IMockGameDataget_Instance>();
		mockGameDataHelper.Setup(h => h.Invoke()).Returns((GameData)null!);
		IMockGameDataget_Instance.Instance = mockGameDataHelper.Object;

		var patchBody = new ProgressTrackerFixedUpdatePatchBody(mockProgress.Object, mockRuntime.Object);
		patchBody.Postfix(mockProgressTracker.Object);

		mockTileParent.VerifySet(t => t.enabled = true, Times.Once);
	}

	[Fact]
	public void Postfix_WhenTotalTasksZero_EnablesTileParentAndReturnsEarly()
	{
		var agencyInstance = CreateAgency(canSeeTaskBar: true);
		var mockRoles = new Mock<INomalGameRoleContainer>();
		Agency? agency = agencyInstance;
		mockRoles.Setup(r => r.TryGetSafeCastedLocalRole<Agency>(out agency)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		var mockTileParent = new Mock<MeshRenderer>(IntPtr.Zero);
		mockTileParent.SetupGet(t => t.enabled).Returns(false);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns(mockTileParent.Object);

		var mockGameData = new Mock<GameData>(IntPtr.Zero);
		mockGameData.SetupGet(g => g.TotalTasks).Returns(0);

		var mockGameDataHelper = new Mock<IMockGameDataget_Instance>();
		mockGameDataHelper.Setup(h => h.Invoke()).Returns(mockGameData.Object);
		IMockGameDataget_Instance.Instance = mockGameDataHelper.Object;

		var patchBody = new ProgressTrackerFixedUpdatePatchBody(mockProgress.Object, mockRuntime.Object);
		patchBody.Postfix(mockProgressTracker.Object);

		mockTileParent.VerifySet(t => t.enabled = true, Times.Once);
	}

	[Fact]
	public void Postfix_WhenAgencyCanSeeTaskBar_NormalGame_UpdatesTracker()
	{
		var agencyInstance = CreateAgency(canSeeTaskBar: true);
		var mockRoles = new Mock<INomalGameRoleContainer>();
		Agency? agency = agencyInstance;
		mockRoles.Setup(r => r.TryGetSafeCastedLocalRole<Agency>(out agency)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		// Material and TileParent Setup
		var mockMaterial = new Mock<Material>(IntPtr.Zero);
		var mockTileParent = new Mock<MeshRenderer>(IntPtr.Zero);
		mockTileParent.SetupGet(t => t.enabled).Returns(true);
		mockTileParent.SetupGet(t => t.material).Returns(mockMaterial.Object);

		// GameObject Setup
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);

		// ProgressTracker Setup
		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns(mockTileParent.Object);
		mockProgressTracker.SetupGet(p => p.gameObject).Returns(mockGameObject.Object);

		// TutorialManager Setup (false)
		var mockTutorialExists = new Mock<IMockDestroyableSingletonget_InstanceExists<TutorialManager>>();
		mockTutorialExists.Setup(h => h.Invoke()).Returns(false);
		IMockDestroyableSingletonget_InstanceExists<TutorialManager>.Instance = mockTutorialExists.Object;

		// GameOptionsManager Setup (NumImpostors = 2)
		var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
		mockGameOptions.Setup(o => o.GetInt(Int32OptionNames.NumImpostors)).Returns(2);

		var mockGameOptionsManager = new Mock<GameOptionsManager>(IntPtr.Zero);
		mockGameOptionsManager.SetupGet(g => g.CurrentGameOptions).Returns(mockGameOptions.Object);

		var mockOptionsMgrHelper = new Mock<IMockGameOptionsManagerget_Instance>();
		mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockGameOptionsManager.Object);
		IMockGameOptionsManagerget_Instance.Instance = mockOptionsMgrHelper.Object;

		// GameData Setup
		// PlayerCount = 10, TotalTasks = 20, CompletedTasks = 10
		// AllPlayers: 10 players, 1 disconnected -> num = (10 - 2) - 1 = 7.
		var mockGameData = new Mock<GameData>(IntPtr.Zero);
		mockGameData.SetupGet(g => g.TotalTasks).Returns(20);
		mockGameData.SetupGet(g => g.CompletedTasks).Returns(10);
		mockGameData.SetupGet(g => g.PlayerCount).Returns(10);

		var mockPlayerList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		var playerArray = new NetworkedPlayerInfo[10];
		for (int i = 0; i < 10; i++)
		{
			var p = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
			p.SetupGet(x => x.Disconnected).Returns(i == 0); // 1 disconnected
			playerArray[i] = p.Object;
		}
		mockPlayerList.Setup(l => l.ToArray()).Returns(new Il2CppReferenceArray<NetworkedPlayerInfo>(playerArray));
		mockGameData.SetupGet(g => g.AllPlayers).Returns(mockPlayerList.Object);

		var mockGameDataHelper = new Mock<IMockGameDataget_Instance>();
		mockGameDataHelper.Setup(h => h.Invoke()).Returns(mockGameData.Object);
		IMockGameDataget_Instance.Instance = mockGameDataHelper.Object;

		var patchBody = new ProgressTrackerFixedUpdatePatchBody(mockProgress.Object, mockRuntime.Object);
		patchBody.Postfix(mockProgressTracker.Object);

		mockGameObject.Verify(g => g.SetActive(true), Times.Once);
		mockMaterial.Verify(m => m.SetFloat("_Buckets", 7f), Times.Once);
		mockMaterial.Verify(m => m.SetFloat("_FullBuckets", mockProgressTracker.Object.curValue), Times.Once);
	}

	[Fact]
	public void Postfix_WhenSlaveDriverCanSeeTaskBar_TutorialGame_UpdatesTracker()
	{
		var slaveDriverInstance = CreateSlaveDriver(canSeeTaskBar: true);
		var mockRoles = new Mock<INomalGameRoleContainer>();
		Agency? agency = null;
		SlaveDriver? slaveDriver = slaveDriverInstance;
		mockRoles.Setup(r => r.TryGetSafeCastedLocalRole<Agency>(out agency)).Returns(false);
		mockRoles.Setup(r => r.TryGetSafeCastedLocalRole<SlaveDriver>(out slaveDriver)).Returns(true);

		var mockContext = new Mock<IGameContext>();
		mockContext.SetupGet(c => c.Roles).Returns(mockRoles.Object);

		var mockProgress = new Mock<IGameProgress>();
		mockProgress.SetupGet(p => p.IsGameNow).Returns(true);

		var mockRuntime = new Mock<IGameRuntime>();
		IGameContext? context = mockContext.Object;
		mockRuntime.Setup(r => r.TryGetGameContext(out context)).Returns(true);

		// Material and TileParent Setup
		var mockMaterial = new Mock<Material>(IntPtr.Zero);
		var mockTileParent = new Mock<MeshRenderer>(IntPtr.Zero);
		mockTileParent.SetupGet(t => t.enabled).Returns(false);
		mockTileParent.SetupGet(t => t.material).Returns(mockMaterial.Object);

		// GameObject Setup
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);

		// ProgressTracker Setup
		var mockProgressTracker = new Mock<ProgressTracker>(IntPtr.Zero);
		mockProgressTracker.SetupGet(p => p.TileParent).Returns(mockTileParent.Object);
		mockProgressTracker.SetupGet(p => p.gameObject).Returns(mockGameObject.Object);

		// TutorialManager Setup (true)
		var mockTutorialExists = new Mock<IMockDestroyableSingletonget_InstanceExists<TutorialManager>>();
		mockTutorialExists.Setup(h => h.Invoke()).Returns(true);
		IMockDestroyableSingletonget_InstanceExists<TutorialManager>.Instance = mockTutorialExists.Object;

		// GameData Setup
		// PlayerCount = 1, TotalTasks = 10, CompletedTasks = 5
		// num = 1 - 0 = 1. curProgress = (5 / 10) * 1 = 0.5.
		var mockGameData = new Mock<GameData>(IntPtr.Zero);
		mockGameData.SetupGet(g => g.TotalTasks).Returns(10);
		mockGameData.SetupGet(g => g.CompletedTasks).Returns(5);
		mockGameData.SetupGet(g => g.PlayerCount).Returns(1);

		var mockPlayerList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		var playerArray = new NetworkedPlayerInfo[1];
		var p = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		p.SetupGet(x => x.Disconnected).Returns(false);
		playerArray[0] = p.Object;
		mockPlayerList.Setup(l => l.ToArray()).Returns(new Il2CppReferenceArray<NetworkedPlayerInfo>(playerArray));
		mockGameData.SetupGet(g => g.AllPlayers).Returns(mockPlayerList.Object);

		var mockGameDataHelper = new Mock<IMockGameDataget_Instance>();
		mockGameDataHelper.Setup(h => h.Invoke()).Returns(mockGameData.Object);
		IMockGameDataget_Instance.Instance = mockGameDataHelper.Object;

		var patchBody = new ProgressTrackerFixedUpdatePatchBody(mockProgress.Object, mockRuntime.Object);
		patchBody.Postfix(mockProgressTracker.Object);

		mockTileParent.VerifySet(t => t.enabled = true, Times.Once);
		mockGameObject.Verify(g => g.SetActive(true), Times.Once);
		mockMaterial.Verify(m => m.SetFloat("_Buckets", 1f), Times.Once);
		mockMaterial.Verify(m => m.SetFloat("_FullBuckets", mockProgressTracker.Object.curValue), Times.Once);
	}
}
