using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.Neutal;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.InGameVisualUpdater;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Neutral;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using TMPro;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.InGameVisualUpdater;

[Collection("UnityMock")]
public class OtherPlayerVisualUpdaterTests : IDisposable
{
	public OtherPlayerVisualUpdaterTests()
	{
		MockSetupHelper.SetupCommonMocks();
		MockSetupHelper.SetupLogger();
		MockSetupHelper.SetupMockExtremeRolePlugin();
		ResetState();
	}

	public void Dispose()
	{
		ResetState();
	}

	private static void ResetState()
	{
		ExtremeRoleManager.GameRole.Clear();
		ExtremeGhostRoleManager.GameRole.Clear();

		if (ClientOption.Instance == null)
		{
			ClientOption.Create();
		}

		var clientOption = ClientOption.Instance;
		clientOption.GhostsSeeRole.Value = true;
		clientOption.GhostsSeeTask.Value = true;

		if (ExtremeRolesPlugin.ShipState != null)
		{
			ExtremeRolesPlugin.ShipState.Initialize();
		}
	}

	private static (
		PlayerControl player,
		Mock<NetworkedPlayerInfo> mockData,
		Mock<TextMeshPro> mockNameText,
		Mock<GameObject> mockInfoGameObject,
		Mock<TextMeshPro> mockInfoText) CreateMockPlayer(
		byte playerId,
		bool isDead = false,
		bool isImpostor = false,
		bool isVisible = true,
		RoleTypes roleType = RoleTypes.Crewmate)
	{
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		var mockRole = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRole.SetupGet(r => r.IsImpostor).Returns(isImpostor);
		mockRole.SetupGet(r => r.Role).Returns(roleType);

		var mockTaskList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		mockTaskList.SetupGet(l => l.Count).Returns(0);

		mockData.SetupGet(d => d.PlayerId).Returns(playerId);
		mockData.SetupGet(d => d.IsDead).Returns(isDead);
		mockData.SetupGet(d => d.Role).Returns(mockRole.Object);
		mockData.SetupGet(d => d.Tasks).Returns(mockTaskList.Object);

		var mockOutfit = new Mock<NetworkedPlayerInfo.PlayerOutfit>(IntPtr.Zero);
		mockOutfit.SetupGet(o => o.PlayerName).Returns($"Player_{playerId}");
		mockPlayer.SetupGet(p => p.CurrentOutfit).Returns(mockOutfit.Object);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
		mockPlayer.SetupGet(p => p.Visible).Returns(isVisible);

		var mockNameText = new Mock<TextMeshPro>(IntPtr.Zero);
		var mockNameTransform = new Mock<Transform>(IntPtr.Zero);
		var mockParentTransform = new Mock<Transform>(IntPtr.Zero);

		mockNameTransform.SetupGet(t => t.parent).Returns(mockParentTransform.Object);
		mockNameTransform.SetupProperty(t => t.localPosition, Vector3.zero);
		mockNameText.SetupGet(t => t.transform).Returns(mockNameTransform.Object);
		mockNameText.SetupProperty(t => t.text, $"Player_{playerId}");
		mockNameText.SetupProperty(t => t.color, Color.white);

		var mockCosmetics = new Mock<CosmeticsLayer>(IntPtr.Zero);
		mockCosmetics.SetupGet(c => c.nameText).Returns(mockNameText.Object);
		mockCosmetics.Setup(c => c.SetName(It.IsAny<string>()))
			.Callback<string>(s => mockNameText.Object.text = s);
		mockCosmetics.Setup(c => c.SetNameColor(It.IsAny<Color>()))
			.Callback<Color>(c => mockNameText.Object.color = c);
		mockPlayer.SetupGet(p => p.cosmetics).Returns(mockCosmetics.Object);

		var mockInfoText = new Mock<TextMeshPro>(IntPtr.Zero);
		var mockInfoTransform = new Mock<Transform>(IntPtr.Zero);
		mockInfoTransform.SetupProperty(t => t.localPosition, Vector3.zero);
		mockInfoText.SetupGet(t => t.transform).Returns(mockInfoTransform.Object);
		mockInfoText.SetupProperty(t => t.fontSize, 10f);
		mockInfoText.SetupProperty(t => t.text, "");

		var mockInfoGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockInfoGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));
		mockInfoText.SetupGet(t => t.gameObject).Returns(mockInfoGameObject.Object);

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns(mockInfoText.Object);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns(mockInfoText.Object);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		return (mockPlayer.Object, mockData, mockNameText, mockInfoGameObject, mockInfoText);
	}

	[Fact]
	public void Update_WhenLocalOrTargetInvalid_DoesNothing()
	{
		var (localPlayer, _, _, _, _) = CreateMockPlayer(0);
		var (targetPlayer, _, _, _, _) = CreateMockPlayer(1);

		var mockNullDataPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockNullDataPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var updaterNullLocal = new OtherPlayerVisualUpdater(mockNullDataPlayer.Object, targetPlayer);
		updaterNullLocal.Update();

		var updaterNullTarget = new OtherPlayerVisualUpdater(localPlayer, mockNullDataPlayer.Object);
		updaterNullTarget.Update();
	}

	[Fact]
	public void Update_WhenTargetRoleNotFound_LogsErrorAndReturns()
	{
		var (localPlayer, _, _, _, _) = CreateMockPlayer(0);
		var (targetPlayer, _, _, _, _) = CreateMockPlayer(1);

		var localRole = new Jester();
		ExtremeRoleManager.GameRole[0] = localRole;
		// targetRole (ID 1) not registered in ExtremeRoleManager

		var updater = new OtherPlayerVisualUpdater(localPlayer, targetPlayer);
		updater.Update();
	}

	[Fact]
	public void Update_WhenBothAlive_UpdatesVisualNameAndColor()
	{
		var (localPlayer, _, _, _, _) = CreateMockPlayer(0, isDead: false);
		var (targetPlayer, _, targetNameText, targetInfoObj, _) = CreateMockPlayer(1, isDead: false);

		var localRole = new Jester();
		var targetRole = new Bait();
		ExtremeRoleManager.GameRole[0] = localRole;
		ExtremeRoleManager.GameRole[1] = targetRole;

		var updater = new OtherPlayerVisualUpdater(localPlayer, targetPlayer);
		updater.Update();

		targetInfoObj.Verify(g => g.SetActive(false), Times.AtLeastOnce());
	}

	[Fact]
	public void Update_WhenLocalDeadAndGhostsSeeRole_ShowsRoleInfo()
	{
		var (localPlayer, _, _, _, _) = CreateMockPlayer(0, isDead: true);
		var (targetPlayer, _, targetNameText, targetInfoObj, targetInfoText) = CreateMockPlayer(1, isDead: false);

		var localRole = new Jester();
		var targetRole = new Bait();
		ExtremeRoleManager.GameRole[0] = localRole;
		ExtremeRoleManager.GameRole[1] = targetRole;

		var updater = new OtherPlayerVisualUpdater(localPlayer, targetPlayer);
		updater.Update();

		targetInfoObj.Verify(g => g.SetActive(true), Times.AtLeastOnce());
	}

	[Fact]
	public void Update_WhenGuardianAngelRole_BlocksShowingInfo()
	{
		var (localPlayer, _, _, _, _) = CreateMockPlayer(0, isDead: true, roleType: RoleTypes.GuardianAngel);
		var (targetPlayer, _, _, targetInfoObj, _) = CreateMockPlayer(1, isDead: false);

		var localRole = new Jester();
		var targetRole = new Bait();
		ExtremeRoleManager.GameRole[0] = localRole;
		ExtremeRoleManager.GameRole[1] = targetRole;

		var updater = new OtherPlayerVisualUpdater(localPlayer, targetPlayer);
		updater.Update();

		targetInfoObj.Verify(g => g.SetActive(false), Times.AtLeastOnce());
	}

	[Fact]
	public void Update_WhenTargetHasGhostRole_FormatsGhostRoleNameInInfo()
	{
		var (localPlayer, _, _, _, _) = CreateMockPlayer(0, isDead: true);
		var (targetPlayer, _, _, targetInfoObj, _) = CreateMockPlayer(1, isDead: false);

		var localRole = new Jester();
		var targetRole = new Bait();
		var targetGhostRole = new Foras();

		ExtremeRoleManager.GameRole[0] = localRole;
		ExtremeRoleManager.GameRole[1] = targetRole;
		ExtremeGhostRoleManager.GameRole[1] = targetGhostRole;

		var updater = new OtherPlayerVisualUpdater(localPlayer, targetPlayer);
		updater.Update();

		targetInfoObj.Verify(g => g.SetActive(true), Times.AtLeastOnce());
	}

	[Fact]
	public void Update_WhenGhostSeeOptionsDisabled_HidesInfo()
	{
		ClientOption.Instance.GhostsSeeRole.Value = false;
		ClientOption.Instance.GhostsSeeTask.Value = false;

		var (localPlayer, _, _, _, _) = CreateMockPlayer(0, isDead: true);
		var (targetPlayer, _, _, targetInfoObj, _) = CreateMockPlayer(1, isDead: false);

		var localRole = new Jester();
		var targetRole = new Bait();
		ExtremeRoleManager.GameRole[0] = localRole;
		ExtremeRoleManager.GameRole[1] = targetRole;

		var updater = new OtherPlayerVisualUpdater(localPlayer, targetPlayer);
		updater.Update();

		targetInfoObj.Verify(g => g.SetActive(false), Times.AtLeastOnce());
	}

	[Fact]
	public void Update_WhenLocalHasGhostRole_AppliesGhostColorAndBlocksInfo()
	{
		var (localPlayer, _, _, _, _) = CreateMockPlayer(0, isDead: true);
		var (targetPlayer, _, _, targetInfoObj, _) = CreateMockPlayer(1, isDead: false);

		var localRole = new Jester();
		var localGhostRole = new Foras();
		var targetRole = new Bait();

		ExtremeRoleManager.GameRole[0] = localRole;
		ExtremeGhostRoleManager.GameRole[0] = localGhostRole;
		ExtremeRoleManager.GameRole[1] = targetRole;

		var updater = new OtherPlayerVisualUpdater(localPlayer, targetPlayer);
		updater.Update();

		targetInfoObj.Verify(g => g.SetActive(false), Times.AtLeastOnce());
	}

	[Fact]
	public void Update_WhenMultiAssignRole_EvaluatesBlockConditionOnAnotherRole()
	{
		var (localPlayer, _, _, _, _) = CreateMockPlayer(0, isDead: true);
		var (targetPlayer, _, _, targetInfoObj, _) = CreateMockPlayer(1, isDead: false);

		var subRole = new Jester();
		var multiRole = new Lover();
		multiRole.SetAnotherRole(subRole);

		var targetRole = new Bait();
		ExtremeRoleManager.GameRole[0] = multiRole;
		ExtremeRoleManager.GameRole[1] = targetRole;

		var updater = new OtherPlayerVisualUpdater(localPlayer, targetPlayer);
		updater.Update();
	}
}
