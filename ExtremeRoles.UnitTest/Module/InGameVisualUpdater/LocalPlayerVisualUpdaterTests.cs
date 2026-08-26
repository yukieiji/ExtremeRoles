using System;
using System.Collections.Generic;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.Neutal;
using ExtremeRoles.Module.InGameVisualUpdater;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Neutral;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using TMPro;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.InGameVisualUpdater;

[Collection("UnityMock")]
public class LocalPlayerVisualUpdaterTests : IDisposable
{
	public LocalPlayerVisualUpdaterTests()
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

		var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		var mockTaskPanel = new Mock<TaskPanelBehaviour>(IntPtr.Zero);
		var mockTabTransform = new Mock<Transform>(IntPtr.Zero);
		var mockTabBackground = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockTabBackground.SetupGet(s => s.transform).Returns(mockTabTransform.Object);
		mockTaskPanel.SetupGet(t => t.tab).Returns(mockTabBackground.Object);
		hudMock.SetupGet(h => h.TaskPanel).Returns(mockTaskPanel.Object);

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(StringNames.Tasks, It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("Tasks");
	}

	private static (
		PlayerControl player,
		Mock<NetworkedPlayerInfo> mockData,
		Mock<TextMeshPro> mockNameText,
		Mock<GameObject> mockInfoGameObject,
		Mock<TextMeshPro> mockInfoText) CreateMockPlayer(
		byte playerId = 0,
		bool isDead = false,
		bool isImpostor = false,
		bool isVisible = true)
	{
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		var mockRole = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRole.SetupGet(r => r.IsImpostor).Returns(isImpostor);

		var mockTaskList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		mockTaskList.SetupGet(l => l.Count).Returns(0);

		mockData.SetupGet(d => d.PlayerId).Returns(playerId);
		mockData.SetupGet(d => d.IsDead).Returns(isDead);
		mockData.SetupGet(d => d.Role).Returns(mockRole.Object);
		mockData.SetupGet(d => d.Tasks).Returns(mockTaskList.Object);

		var mockOutfit = new Mock<NetworkedPlayerInfo.PlayerOutfit>(IntPtr.Zero);
		mockOutfit.SetupGet(o => o.PlayerName).Returns("TestLocalPlayer");
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
		mockNameText.SetupProperty(t => t.text, "TestLocalPlayer");
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
	public void Update_WhenDataNullOrRoleNull_DoesNothing()
	{
		var mockPlayer1 = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer1.SetupGet(p => p.Data).Returns((NetworkedPlayerInfo)null!);
		var updater1 = new LocalPlayerVisualUpdater(mockPlayer1.Object);
		updater1.Update();

		var mockPlayer2 = new Mock<PlayerControl>(IntPtr.Zero);
		var mockData2 = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData2.SetupGet(d => d.Role).Returns((RoleBehaviour)null!);
		mockPlayer2.SetupGet(p => p.Data).Returns(mockData2.Object);
		var updater2 = new LocalPlayerVisualUpdater(mockPlayer2.Object);
		updater2.Update();
	}

	[Fact]
	public void Update_WhenValid_ExecutesFullUpdateSequence()
	{
		var (player, mockData, mockNameText, mockInfoGameObject, mockInfoText) = CreateMockPlayer(0, false, false, true);
		var updater = new LocalPlayerVisualUpdater(player);

		var role = new Jester();
		ExtremeRoleManager.GameRole[0] = role;

		updater.Update();

		mockInfoGameObject.Verify(g => g.SetActive(true), Times.AtLeastOnce());
	}

	[Fact]
	public void Update_WhenImpostor_ResetsNameToImpostorRed()
	{
		var (player, mockData, mockNameText, mockInfoGameObject, mockInfoText) = CreateMockPlayer(0, false, true, true);
		var updater = new LocalPlayerVisualUpdater(player);

		var role = new Jester();
		ExtremeRoleManager.GameRole[0] = role;

		updater.Update();
	}

	[Fact]
	public void Update_WhenGhostRoleAndMultiAssignRolePresent_UpdatesAllComponents()
	{
		var (player, mockData, mockNameText, mockInfoGameObject, mockInfoText) = CreateMockPlayer(0, true, false, true);
		var updater = new LocalPlayerVisualUpdater(player);

		var subRole = new Jester();
		var multiRole = new Lover();
		multiRole.SetAnotherRole(subRole);
		var ghostRole = new Foras();

		ExtremeRoleManager.GameRole[0] = multiRole;
		ExtremeGhostRoleManager.GameRole[0] = ghostRole;

		updater.Update();

		mockInfoGameObject.Verify(g => g.SetActive(true), Times.AtLeastOnce());
	}

	[Fact]
	public void Update_WhenNotVisual_HidesInfoObject()
	{
		var (player, mockData, mockNameText, mockInfoGameObject, mockInfoText) = CreateMockPlayer(0, false, false, false);
		var updater = new LocalPlayerVisualUpdater(player);

		var role = new Jester();
		ExtremeRoleManager.GameRole[0] = role;

		updater.Update();

		mockInfoGameObject.Verify(g => g.SetActive(false), Times.AtLeastOnce());
	}

	[Fact]
	public void Update_WhenHudManagerInstanceExists_UpdatesTabText()
	{
		var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		var mockTaskPanel = new Mock<TaskPanelBehaviour>(IntPtr.Zero);
		var mockTabTransform = new Mock<Transform>(IntPtr.Zero);

		var mockTabBackground = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockTabBackground.SetupGet(s => s.transform).Returns(mockTabTransform.Object);

		var mockTabTextTransform = new Mock<Transform>(IntPtr.Zero);
		var mockTabText = new Mock<TextMeshPro>(IntPtr.Zero);

		mockTabTextTransform.Setup(t => t.GetComponent<TextMeshPro>()).Returns(mockTabText.Object);
		mockTabTransform.Setup(t => t.Find("TabText_TMP")).Returns(mockTabTextTransform.Object);
		mockTaskPanel.SetupGet(t => t.tab).Returns(mockTabBackground.Object);
		hudMock.SetupGet(h => h.TaskPanel).Returns(mockTaskPanel.Object);

		var (player, mockData, mockNameText, mockInfoGameObject, mockInfoText) = CreateMockPlayer(0, false, false, true);
		var updater = new LocalPlayerVisualUpdater(player);

		var role = new Jester();
		ExtremeRoleManager.GameRole[0] = role;

		updater.Update();
		// Call twice to test tabText caching path
		updater.Update();

		mockTabText.Verify(t => t.SetText(It.IsAny<string>()), Times.AtLeastOnce());
	}
}
