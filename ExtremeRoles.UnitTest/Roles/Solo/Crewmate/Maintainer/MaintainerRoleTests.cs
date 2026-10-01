using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using ExtremeRoles.Compat;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Moq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.MaintainerTests;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class MaintainerRoleTests
{
	private readonly Mock<AmongUsClient> clientMock;

	public MaintainerRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();

		if (MockVector3get_oneHelper.Instance == null)
		{
			var mockOne = new Mock<MockVector3get_oneHelper>();
			mockOne.Setup(x => x.Invoke()).Returns(new Vector3(1f, 1f, 1f));
			MockVector3get_oneHelper.Instance = mockOne.Object;
		}

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		SetupHudManagerMock();

		clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		SetLobbyMode(false);
	}

	private void SetLobbyMode(bool isLobby)
	{
		if (isLobby)
		{
			clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.NotJoined);
			var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
		else
		{
			clientMock.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
			var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
			mockLobbyHelper.Setup(x => x.Invoke()).Returns((LobbyBehaviour)null!);
			MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
		}
	}

	private static void SetupHudManagerMock()
	{
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));

		var mockGridArrange = new Mock<GridArrange>(IntPtr.Zero);
		mockGridArrange.Setup(g => g.ArrangeChilds());

		var mockParentGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockParentGameObject.Setup(g => g.GetComponent<GridArrange>()).Returns(mockGridArrange.Object);

		var mockParentTransform = new Mock<Transform>(IntPtr.Zero);
		mockParentTransform.Setup(t => t.gameObject).Returns(mockParentGameObject.Object);

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockTransform.Setup(t => t.parent).Returns(mockParentTransform.Object);
		mockTransform.Setup(t => t.FindChild(It.IsAny<string>())).Returns((Transform)null!);

		var mockMaterial = new Mock<Material>(IntPtr.Zero);
		mockMaterial.Setup(m => m.SetFloat(It.IsAny<string>(), It.IsAny<float>()));

		var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockSpriteRenderer.SetupProperty(s => s.sprite);
		mockSpriteRenderer.SetupProperty(s => s.color);
		mockSpriteRenderer.SetupProperty(s => s.enabled);
		mockSpriteRenderer.SetupGet(s => s.material).Returns(mockMaterial.Object);

		var mockLabelText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockLabelText.SetupProperty(t => t.color);
		mockLabelText.SetupProperty(t => t.fontMaterial);
		mockLabelText.SetupProperty(t => t.text);

		var mockCoolText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockCoolText.SetupProperty(t => t.color);
		mockCoolText.SetupProperty(t => t.enableWordWrapping);
		mockCoolText.SetupProperty(t => t.text);
		mockCoolText.SetupGet(t => t.gameObject).Returns(mockGameObject.Object);
		mockCoolText.SetupGet(t => t.transform).Returns(mockTransform.Object);

		var mockPersistentCallGroup = new Mock<PersistentCallGroup>(IntPtr.Zero);
		mockPersistentCallGroup.Setup(p => p.Clear());

		var mockOnClick = new Mock<UnityEngine.UI.Button.ButtonClickedEvent>(IntPtr.Zero);
		mockOnClick.Setup(e => e.RemoveAllListeners());
		mockOnClick.Setup(e => e.AddListener(It.IsAny<UnityAction>()));
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
		mockKillButton.SetupGet(b => b.isActiveAndEnabled).Returns(true);

		var mockUseButton = new Mock<UseButton>(IntPtr.Zero);
		mockUseButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
		mockUseButton.SetupGet(b => b.transform).Returns(mockTransform.Object);

		var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		hudMock.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
		hudMock.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);

		var mockInstantiate = new Mock<MockObjectInstantiateHelper>();
		mockInstantiate.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Vector3>(), It.IsAny<Quaternion>()))
			.Returns((UnityEngine.Object original, Vector3 pos, Quaternion rot) => original);
		MockObjectInstantiateHelper.Instance = mockInstantiate.Object;

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate7 = new Mock<MockObjectInstantiateHelper7>();
		mockInstantiate7.Setup(x => x.Invoke(It.IsAny<Material>()))
			.Returns(mockMaterial.Object);
		MockObjectInstantiateHelper7.Instance = mockInstantiate7.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		var mockUnityActionImplicit = new Mock<MockUnityActionop_ImplicitHelper>();
		mockUnityActionImplicit.Setup(x => x.Invoke(It.IsAny<Action>()))
			.Returns((Action action) => action != null ? new UnityAction(IntPtr.Zero) : null!);
		MockUnityActionop_ImplicitHelper.Instance = mockUnityActionImplicit.Object;
	}

	[Fact]
	public void Constructor_InitializesCorrectly()
	{
		// Act
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		// Assert
		Assert.NotNull(role);
		Assert.Equal(ExtremeRoleId.Maintainer, role.Core.Id);
		Assert.Equal(ColorPalette.MaintainerBlue, role.GetNameColor(true));
	}

	[Fact]
	public void Button_GetAndSet_ReturnsSetValue()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();
		var mockButton = (ExtremeAbilityButton)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeAbilityButton));

		// Act
		role.Button = mockButton;
		var result = role.Button;

		// Assert
		Assert.Equal(mockButton, result);
	}

	[Fact]
	public void CreateAbility_SetsButton()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();
		string spriteKey = $"{ObjectPath.MaintainerRepair}115";
		if (!LruCache<string, Sprite>.TryGetValue(spriteKey, out _))
		{
			var mockSprite = new Mock<Sprite>(IntPtr.Zero);
			LruCache<string, Sprite>.Add(spriteKey, mockSprite.Object);
		}

		// Act
		role.CreateAbility();

		// Assert
		Assert.NotNull(role.Button);
	}

	[Fact]
	public void CreateSpecificOption_CreatesExpectedOptions()
	{
		// Arrange
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Maintainer);
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		// Act
		role.CreateRoleAllOption();

		// Assert
		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);
	}

	[Fact]
	public void ResetOnMeetingStart_ExecutesWithoutException()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		// Act
		role.ResetOnMeetingStart();

		// Assert
		Assert.NotNull(role);
	}

	[Fact]
	public void ResetOnMeetingEnd_ExecutesWithoutException()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		// Act
		role.ResetOnMeetingEnd(null);

		// Assert
		Assert.NotNull(role);
	}

	[Fact]
	public void RoleSpecificInit_ExecutesWithoutException()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		// Act
		role.Initialize();

		// Assert
		Assert.NotNull(role);
	}

	[Fact]
	public void UseAbility_WhenDoorsExistWithAndWithoutDecon_RepairsSabotageAndOpensDoors()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(0);
		mockLocalPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

		var mockDoor1 = new Mock<OpenableDoor>(IntPtr.Zero);
		mockDoor1.SetupGet(d => d.Id).Returns(1);
		mockDoor1.Setup(d => d.GetComponentInChildren<DeconControl>()).Returns((DeconControl)null!);

		var mockDecon = new Mock<DeconControl>(IntPtr.Zero);
		var mockDoor2 = new Mock<OpenableDoor>(IntPtr.Zero);
		mockDoor2.SetupGet(d => d.Id).Returns(2);
		mockDoor2.Setup(d => d.GetComponentInChildren<DeconControl>()).Returns(mockDecon.Object);

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		mockShip.SetupGet(s => s.AllDoors).Returns(new Il2CppReferenceArray<OpenableDoor>([mockDoor1.Object, mockDoor2.Object]));

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		// Act
		bool result = role.UseAbility();

		// Assert
		Assert.True(result);
		mockDoor1.Verify(d => d.SetDoorway(true), Times.Once());
		mockShip.Verify(s => s.RpcUpdateSystem(SystemTypes.Doors, (byte)(1 | 64)), Times.Once());

		mockDoor2.Verify(d => d.SetDoorway(It.IsAny<bool>()), Times.Never());
		mockShip.Verify(s => s.RpcUpdateSystem(SystemTypes.Doors, (byte)(2 | 64)), Times.Never());
	}

	[Fact]
	public void UseAbility_WhenSabotageTasksActive_RepairsAllSabotages()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();

		var mockTaskReactor = new Mock<PlayerTask>(IntPtr.Zero);
		mockTaskReactor.SetupGet(t => t.TaskType).Returns(TaskTypes.ResetReactor);

		var mockTaskComms = new Mock<PlayerTask>(IntPtr.Zero);
		mockTaskComms.SetupGet(t => t.TaskType).Returns(TaskTypes.FixComms);

		var mockTaskOxy = new Mock<PlayerTask>(IntPtr.Zero);
		mockTaskOxy.SetupGet(t => t.TaskType).Returns(TaskTypes.RestoreOxy);

		var mockTaskSeismic = new Mock<PlayerTask>(IntPtr.Zero);
		mockTaskSeismic.SetupGet(t => t.TaskType).Returns(TaskTypes.ResetSeismic);

		var mockTaskCharles = new Mock<PlayerTask>(IntPtr.Zero);
		mockTaskCharles.SetupGet(t => t.TaskType).Returns(TaskTypes.StopCharles);

		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(6);
		mockTasksList.Setup(l => l[0]).Returns((PlayerTask)null!);
		mockTasksList.Setup(l => l[1]).Returns(mockTaskReactor.Object);
		mockTasksList.Setup(l => l[2]).Returns(mockTaskComms.Object);
		mockTasksList.Setup(l => l[3]).Returns(mockTaskOxy.Object);
		mockTasksList.Setup(l => l[4]).Returns(mockTaskSeismic.Object);
		mockTasksList.Setup(l => l[5]).Returns(mockTaskCharles.Object);

		mockLocalPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		mockShip.SetupGet(s => s.AllDoors).Returns(new Il2CppReferenceArray<OpenableDoor>(0));

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		// Act
		bool result = role.UseAbility();

		// Assert
		Assert.True(result);
		mockShip.Verify(s => s.RpcUpdateSystem(SystemTypes.Reactor, 16), Times.Once());
		mockShip.Verify(s => s.RpcUpdateSystem(SystemTypes.Comms, 0 | 16), Times.Once());
		mockShip.Verify(s => s.RpcUpdateSystem(SystemTypes.Comms, 1 | 16), Times.Once());
		mockShip.Verify(s => s.RpcUpdateSystem(SystemTypes.LifeSupp, 16), Times.Once());
		mockShip.Verify(s => s.RpcUpdateSystem(SystemTypes.Laboratory, 16), Times.Once());
		mockShip.Verify(s => s.RpcUpdateSystem(SystemTypes.HeliSabotage, 0 | 16), Times.Once());
		mockShip.Verify(s => s.RpcUpdateSystem(SystemTypes.HeliSabotage, 1 | 16), Times.Once());
	}

	[Fact]
	public void UseAbility_WhenCustomSabotageActive_CallsRpcRepairCustomSabotage()
	{
		// Arrange
		MockSetupHelper.SetupCompatModManager();

		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();

		var mockTaskCustom = new Mock<PlayerTask>(IntPtr.Zero);
		mockTaskCustom.SetupGet(t => t.TaskType).Returns((TaskTypes)88);

		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(mockTaskCustom.Object);

		mockLocalPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

		var mockMap = new Mock<IMapMod>();
		mockMap.Setup(m => m.IsCustomSabotageTask((TaskTypes)88)).Returns(true);

		var mapField = typeof(CompatModManager).GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		mapField?.SetValue(CompatModManager.Instance, mockMap.Object);

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		mockShip.SetupGet(s => s.AllDoors).Returns(new Il2CppReferenceArray<OpenableDoor>(0));

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		try
		{
			// Act
			bool result = role.UseAbility();

			// Assert
			Assert.True(result);
			mockMap.Verify(m => m.RpcRepairCustomSabotage((TaskTypes)88), Times.Once());
		}
		finally
		{
			mapField?.SetValue(CompatModManager.Instance, null);
		}
	}

	[Fact]
	public void IsAbilityUse_WhenNoTasks_ReturnsFalse()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(0);
		mockPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

		// Act
		bool canUse = role.IsAbilityUse();

		// Assert
		Assert.False(canUse);
	}

	[Fact]
	public void IsAbilityUse_WhenTaskIsNullInList_ReturnsFalse()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns((PlayerTask)null!);
		mockPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

		// Act
		bool canUse = role.IsAbilityUse();

		// Assert
		Assert.False(canUse);
	}

	[Fact]
	public void IsAbilityUse_WhenCustomSabotageTaskActive_ReturnsTrue()
	{
		// Arrange
		MockSetupHelper.SetupCompatModManager();

		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockPlayer.SetupGet(p => p.CanMove).Returns(true);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var mockTask = new Mock<PlayerTask>(IntPtr.Zero);
		mockTask.SetupGet(t => t.TaskType).Returns((TaskTypes)99);

		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(mockTask.Object);
		mockPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

		var mockMap = new Mock<IMapMod>();
		mockMap.Setup(m => m.IsCustomSabotageTask((TaskTypes)99)).Returns(true);

		var mapField = typeof(CompatModManager).GetField("map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		mapField?.SetValue(CompatModManager.Instance, mockMap.Object);

		try
		{
			// Act
			bool canUse = role.IsAbilityUse();

			// Assert
			Assert.True(canUse);
		}
		finally
		{
			mapField?.SetValue(CompatModManager.Instance, null);
		}
	}

	[Fact]
	public void IsAbilityUse_WhenEmergencyTaskActive_ReturnsTrue()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockPlayer.SetupGet(p => p.CanMove).Returns(true);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var mockTask = new Mock<HudOverrideTask>(IntPtr.Zero);
		mockTask.SetupGet(t => t.TaskType).Returns(TaskTypes.FixComms);

		var mockTaskIsEmergency = new Mock<MockPlayerTaskTaskIsEmergencyHelper>();
		mockTaskIsEmergency.Setup(x => x.Invoke(It.IsAny<PlayerTask>())).Returns(true);
		MockPlayerTaskTaskIsEmergencyHelper.Instance = mockTaskIsEmergency.Object;

		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(mockTask.Object);
		mockPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

		// Act
		bool canUse = role.IsAbilityUse();

		// Assert
		Assert.True(canUse);
	}

	[Fact]
	public void IsAbilityUse_WhenMushroomMixupSabotageActive_ReturnsTrue()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockPlayer.SetupGet(p => p.CanMove).Returns(true);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var mockTask = new Mock<PlayerTask>(IntPtr.Zero);
		mockTask.SetupGet(t => t.TaskType).Returns(TaskTypes.MushroomMixupSabotage);

		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(mockTask.Object);
		mockPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

		// Act
		bool canUse = role.IsAbilityUse();

		// Assert
		Assert.True(canUse);
	}

	[Fact]
	public void IsAbilityUse_WhenNormalTaskOnly_ReturnsFalse()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockPlayer.SetupGet(p => p.CanMove).Returns(true);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var mockTask = new Mock<NormalPlayerTask>(IntPtr.Zero);
		mockTask.SetupGet(t => t.TaskType).Returns(TaskTypes.SwipeCard);

		var mockTaskIsEmergency = new Mock<MockPlayerTaskTaskIsEmergencyHelper>();
		mockTaskIsEmergency.Setup(x => x.Invoke(It.IsAny<PlayerTask>())).Returns(false);
		MockPlayerTaskTaskIsEmergencyHelper.Instance = mockTaskIsEmergency.Object;

		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(mockTask.Object);
		mockPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

		// Act
		bool canUse = role.IsAbilityUse();

		// Assert
		Assert.False(canUse);
	}

	[Fact]
	public void IsAbilityUse_WhenSabotageActiveButPlayerDead_ReturnsFalse()
	{
		// Arrange
		var role = new ExtremeRoles.Roles.Solo.Crewmate.Maintainer();

		var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockPlayer.SetupGet(p => p.CanMove).Returns(true);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(true);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(mockPlayer.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

		var mockTask = new Mock<PlayerTask>(IntPtr.Zero);
		mockTask.SetupGet(t => t.TaskType).Returns(TaskTypes.MushroomMixupSabotage);

		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(mockTask.Object);
		mockPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

		// Act
		bool canUse = role.IsAbilityUse();

		// Assert
		Assert.False(canUse);
	}
}
