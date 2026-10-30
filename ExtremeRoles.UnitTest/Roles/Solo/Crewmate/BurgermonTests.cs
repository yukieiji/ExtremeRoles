using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using Moq;
using UnityEngine;
using Xunit;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class BurgermonSystemTests
{
	public BurgermonSystemTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
	}

	[Fact]
	public void CalculateSpeedMod_PositiveAndNegative_ReturnsExpectedValues()
	{
		// Positive speed (e.g. +3% per burger)
		var system1 = new BurgermonSystem(0.03f, 0.25f, true, true);
		Assert.Equal(1.0f, system1.CalculateSpeedMod(0));
		Assert.Equal(1.03f, system1.CalculateSpeedMod(1), 3);
		Assert.Equal(1.06f, system1.CalculateSpeedMod(2), 3);

		// Negative speed (e.g. -5% per burger)
		var system2 = new BurgermonSystem(-0.05f, 0.25f, true, true);
		Assert.Equal(0.95f, system2.CalculateSpeedMod(1), 3);
		Assert.Equal(0.90f, system2.CalculateSpeedMod(2), 3);

		// Zero speed change
		var system3 = new BurgermonSystem(0.0f, 0.25f, true, true);
		Assert.Equal(1.0f, system3.CalculateSpeedMod(5));
	}

	[Fact]
	public void CalculateSpeedMod_ClampsToMinimum0_1()
	{
		var system = new BurgermonSystem(-0.50f, 0.25f, true, true);
		Assert.Equal(0.1f, system.CalculateSpeedMod(3));
	}

	[Fact]
	public void CalculateScaleFactor_PositiveAndNegative_ReturnsExpectedValues()
	{
		// Positive size (e.g. +25% per burger)
		var system1 = new BurgermonSystem(0.03f, 0.25f, true, true);
		Assert.Equal(1.0f, system1.CalculateScaleFactor(0));
		Assert.Equal(1.25f, system1.CalculateScaleFactor(1), 3);
		Assert.Equal(1.50f, system1.CalculateScaleFactor(2), 3);

		// Negative size (e.g. -20% per burger)
		var system2 = new BurgermonSystem(0.03f, -0.20f, true, true);
		Assert.Equal(0.80f, system2.CalculateScaleFactor(1), 3);
		Assert.Equal(0.60f, system2.CalculateScaleFactor(2), 3);

		// Zero size change
		var system3 = new BurgermonSystem(0.03f, 0.0f, true, true);
		Assert.Equal(1.0f, system3.CalculateScaleFactor(3));
	}

	[Fact]
	public void CalculateScaleFactor_ClampsToMinimum0_1()
	{
		var system = new BurgermonSystem(0.03f, -0.50f, true, true);
		Assert.Equal(0.1f, system.CalculateScaleFactor(3));
	}

	[Fact]
	public void FeedBurger_IncrementsPendingCount_AndTransfersOnMeetingStart()
	{
		var system = new BurgermonSystem();
		byte targetId = 2;

		Assert.Equal(0, system.GetTotalFedCount(targetId));
		Assert.Equal(0, system.GetActiveFedCount(targetId));

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BurgermonRpcOps.FeedBurger)
			.Returns(targetId);

		system.UpdateSystem(null!, mockReader.Object);

		Assert.Equal(1, system.GetTotalFedCount(targetId));
		Assert.Equal(0, system.GetActiveFedCount(targetId));

		system.Reset(ResetTiming.MeetingStart);

		Assert.Equal(1, system.GetTotalFedCount(targetId));
		Assert.Equal(1, system.GetActiveFedCount(targetId));
	}

	[Fact]
	public void Reset_OnPlayer_SpecificPlayer_ClearsFedCountsForPlayer()
	{
		var system = new BurgermonSystem();
		byte targetId1 = 2;
		byte targetId2 = 3;

		var mockReader1 = new Mock<MessageReader>(IntPtr.Zero);
		mockReader1.SetupSequence(r => r.ReadByte())
			.Returns((byte)BurgermonRpcOps.FeedBurger)
			.Returns(targetId1);
		system.UpdateSystem(null!, mockReader1.Object);

		var mockReader2 = new Mock<MessageReader>(IntPtr.Zero);
		mockReader2.SetupSequence(r => r.ReadByte())
			.Returns((byte)BurgermonRpcOps.FeedBurger)
			.Returns(targetId2);
		system.UpdateSystem(null!, mockReader2.Object);

		system.Reset(ResetTiming.MeetingStart);

		Assert.Equal(1, system.GetActiveFedCount(targetId1));
		Assert.Equal(1, system.GetActiveFedCount(targetId2));

		var mockPlayer1 = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer1.SetupGet(p => p.PlayerId).Returns(targetId1);

		system.Reset(ResetTiming.OnPlayer, mockPlayer1.Object);

		Assert.Equal(0, system.GetActiveFedCount(targetId1));
		Assert.Equal(1, system.GetActiveFedCount(targetId2));
	}

	[Fact]
	public void Reset_OnPlayer_NullPlayer_ClearsAllFedCounts()
	{
		var system = new BurgermonSystem();
		byte targetId = 2;

		var mockReader = new Mock<MessageReader>(IntPtr.Zero);
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BurgermonRpcOps.FeedBurger)
			.Returns(targetId);

		system.UpdateSystem(null!, mockReader.Object);
		system.Reset(ResetTiming.MeetingStart);

		Assert.Equal(1, system.GetActiveFedCount(targetId));

		system.Reset(ResetTiming.OnPlayer, null);

		Assert.Equal(0, system.GetActiveFedCount(targetId));
	}
}

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class BurgermonRoleTests
{
	private readonly Mock<AmongUsClient> clientMock;

	public BurgermonRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		MockSetupHelper.SetupOptionManager();

		SetupHudManagerMock();

		this.clientMock = MockSetupHelper.SetupAmongUsClientMock();
		SetLobbyMode(false);

		SetupAssetBundleMock();

		ExtremeGameModeManager.Create(GameModes.Normal);
		ExtremeRoleManager.GameRole.Clear();
	}

	private static void SetupHudManagerMock()
	{
		if (MockVector3get_oneHelper.Instance == null)
		{
			var mockOne = new Mock<MockVector3get_oneHelper>();
			mockOne.Setup(x => x.Invoke()).Returns(new Vector3(1f, 1f, 1f));
			MockVector3get_oneHelper.Instance = mockOne.Object;
		}

		var mockGridArrange = new Mock<GridArrange>(IntPtr.Zero);
		mockGridArrange.Setup(g => g.ArrangeChilds());

		var cachedArrangeField = typeof(ExtremeRoles.Extension.Manager.HudManagerExtension).GetField("cachedArrange", BindingFlags.NonPublic | BindingFlags.Static);
		cachedArrangeField?.SetValue(null, mockGridArrange.Object);

		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		var mockMaterial = new Mock<Material>(IntPtr.Zero);

		var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockSpriteRenderer.SetupProperty(s => s.sprite);
		mockSpriteRenderer.SetupProperty(s => s.color);
		mockSpriteRenderer.SetupProperty(s => s.enabled);
		mockSpriteRenderer.SetupGet(s => s.material).Returns(mockMaterial.Object);

		var mockLabelText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		var mockCoolText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		mockCoolText.SetupGet(t => t.gameObject).Returns(mockGameObject.Object);
		mockCoolText.SetupGet(t => t.transform).Returns(mockTransform.Object);

		var mockPersistentCallGroup = new Mock<UnityEngine.Events.PersistentCallGroup>(IntPtr.Zero);
		var mockOnClick = new Mock<UnityEngine.UI.Button.ButtonClickedEvent>(IntPtr.Zero);
		mockOnClick.SetupGet(e => e.m_PersistentCalls).Returns(mockPersistentCallGroup.Object);

		var mockPassiveButton = new Mock<PassiveButton>(IntPtr.Zero);
		mockPassiveButton.SetupGet(p => p.OnClick).Returns(mockOnClick.Object);

		var mockKillButton = new Mock<KillButton>(IntPtr.Zero);
		mockKillButton.SetupGet(b => b.transform).Returns(mockTransform.Object);
		mockKillButton.SetupGet(b => b.gameObject).Returns(mockGameObject.Object);
		mockKillButton.SetupGet(b => b.graphic).Returns(mockSpriteRenderer.Object);
		mockKillButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
		mockKillButton.SetupGet(b => b.cooldownTimerText).Returns(mockCoolText.Object);
		mockKillButton.Setup(b => b.GetComponent<PassiveButton>()).Returns(mockPassiveButton.Object);

		var mockUseButton = new Mock<UseButton>(IntPtr.Zero);
		mockUseButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
		mockUseButton.SetupGet(b => b.transform).Returns(mockTransform.Object);

		var mockInstantiate = new Mock<MockObjectInstantiateHelper>();
		mockInstantiate.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Vector3>(), It.IsAny<Quaternion>()))
			.Returns((UnityEngine.Object original, Vector3 pos, Quaternion rot) => original);
		MockObjectInstantiateHelper.Instance = mockInstantiate.Object;

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		hudMock.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
		hudMock.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);
	}

	private static void SetupAssetBundleMock()
	{
		var mockAssetBundle = new Mock<AssetBundle>(IntPtr.Zero);
		var mockSprite = new Mock<Sprite>(IntPtr.Zero);

		mockAssetBundle.Setup(b => b.LoadAsset(It.IsAny<string>(), It.IsAny<Il2CppSystem.Type>()))
			.Returns(mockSprite.Object);

		var field = typeof(UnityObjectLoader).GetField("cachedBundle", BindingFlags.NonPublic | BindingFlags.Static);
		if (field?.GetValue(null) is Dictionary<string, AssetBundle> dict)
		{
			dict[ObjectPath.GetRoleAssetPath(ExtremeRoleId.Burgermon)] = mockAssetBundle.Object;
		}
	}

	private void SetLobbyMode(bool isLobby)
	{
		if (isLobby)
		{
			this.clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.NotJoined);
			var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
		else
		{
			this.clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns((LobbyBehaviour)null!);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
	}

	[Fact]
	public void RoleSpecificInit_SetsSystemOptionsCorrectly()
	{
		var burgermon = new Burgermon();
		burgermon.CreateRoleAllOption();
		burgermon.Initialize();

		var system = BurgermonSystem.Get();

		Assert.Equal(0.03f, system.SpeedValue, 3);
		Assert.Equal(0.25f, system.SizeValue, 3);
		Assert.True(system.KeepAfterDeath);
		Assert.True(system.AffectDeadBody);
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenTargetHasFedBurgers_ReturnsFormattedTag()
	{
		var burgermon = new Burgermon();
		burgermon.CreateRoleAllOption();
		burgermon.Initialize();

		byte targetId = 3;
		var system = BurgermonSystem.Get();

		string tagBefore = burgermon.GetRolePlayerNameTag(burgermon, targetId);
		Assert.Equal("", tagBefore);

		var mockReader1 = new Mock<MessageReader>(IntPtr.Zero);
		mockReader1.SetupSequence(r => r.ReadByte())
			.Returns((byte)BurgermonRpcOps.FeedBurger)
			.Returns(targetId);
		system.UpdateSystem(null!, mockReader1.Object);

		var mockReader2 = new Mock<MessageReader>(IntPtr.Zero);
		mockReader2.SetupSequence(r => r.ReadByte())
			.Returns((byte)BurgermonRpcOps.FeedBurger)
			.Returns(targetId);
		system.UpdateSystem(null!, mockReader2.Object);

		string tagAfter = burgermon.GetRolePlayerNameTag(burgermon, targetId);
		Assert.Contains("(2)", tagAfter);
		Assert.Contains("#B16423", tagAfter);
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenTargetHasZeroBurgers_ReturnsDefaultTag()
	{
		var burgermon = new Burgermon();
		burgermon.CreateRoleAllOption();
		burgermon.Initialize();

		byte targetId = 4;
		string tag = burgermon.GetRolePlayerNameTag(burgermon, targetId);

		Assert.Equal("", tag);
	}

	[Fact]
	public void ResetOnMeetingStart_And_ResetOnMeetingEnd_ResetsTargetWithoutThrowing()
	{
		var burgermon = new Burgermon();
		burgermon.CreateRoleAllOption();
		burgermon.Initialize();

		burgermon.ResetOnMeetingStart();
		burgermon.ResetOnMeetingEnd(null);
	}

	[Fact]
	public void ExiledAction_And_RolePlayerKilledAction_ClearsTaskWithoutThrowing()
	{
		var burgermon = new Burgermon();
		burgermon.CreateRoleAllOption();
		burgermon.Initialize();

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		var mockTasks = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		mockTasks.SetupGet(t => t.Count).Returns(0);
		mockData.SetupGet(d => d.Tasks).Returns(mockTasks.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		burgermon.ExiledAction(mockPlayer.Object);
		burgermon.RolePlayerKilledAction(mockPlayer.Object, mockPlayer.Object);
	}

	[Fact]
	public void TaskHandler_Update_And_ClearTask_ExecutesWithoutError()
	{
		var handler = new BurgermonTaskHandler();

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		var mockTasks = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		mockTasks.SetupGet(t => t.Count).Returns(0);
		mockData.SetupGet(d => d.Tasks).Returns(mockTasks.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		handler.Update(mockPlayer.Object);
		handler.ClearTask(mockPlayer.Object);
	}

	[Fact]
	public void Update_IncrementsAbilityCount_WhenTasksCompletedReachesThreshold()
	{
		byte playerId = 1;
		var burgermon = new Burgermon();
		burgermon.CreateRoleAllOption();
		burgermon.Initialize();

		ExtremeRoleManager.GameRole[playerId] = burgermon;

		int abilityCount = 0;
		var mockBehavior = new Mock<BehaviorBase>("Test", null!);
		var mockCountBehavior = mockBehavior.As<ICountBehavior>();
		mockCountBehavior.SetupGet(b => b.AbilityCount).Returns(() => abilityCount);
		mockCountBehavior.Setup(b => b.SetAbilityCount(It.IsAny<int>())).Callback((int c) => abilityCount = c);

		var mockActivator = new Mock<IButtonAutoActivator>();
		var button = new ExtremeAbilityButton(mockBehavior.Object, mockActivator.Object, KeyCode.F);
		burgermon.Button = button;

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.PlayerId).Returns(playerId);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);

		var mockVanillaRole = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockVanillaRole.SetupGet(r => r.TasksCountTowardProgress).Returns(true);
		mockData.SetupGet(d => d.Role).Returns(mockVanillaRole.Object);

		var mockTaskList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
		var task1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		task1.SetupGet(t => t.Complete).Returns(true);

		mockTaskList.SetupGet(l => l.Count).Returns(1);
		mockTaskList.Setup(l => l[0]).Returns(task1.Object);

		mockData.SetupGet(d => d.Tasks).Returns(mockTaskList.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		Assert.Equal(0, abilityCount);
		burgermon.Update(mockPlayer.Object);
		Assert.Equal(1, abilityCount);
	}
}
