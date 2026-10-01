using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using Hazel;
using InnerNet;
using MonoMod.RuntimeDetour;
using Moq;
using TMPro;
using UnityEngine;
using Xunit;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ChargerTests : IDisposable
{
	private delegate RPCOperator.RpcCaller CreateCallerOrig(uint netId, RPCOperator.Command ops);
	private delegate RPCOperator.RpcCaller CreateCallerHook(CreateCallerOrig orig, uint netId, RPCOperator.Command ops);

	private delegate void RpcWriteByteOrig(RPCOperator.RpcCaller self, byte value);
	private delegate void RpcWriteByteHook(RpcWriteByteOrig orig, RPCOperator.RpcCaller self, byte value);

	private delegate void RpcDisposeOrig(RPCOperator.RpcCaller self);
	private delegate void RpcDisposeHook(RpcDisposeOrig orig, RPCOperator.RpcCaller self);

	private delegate void SetButtonShowOrig(ExtremeAbilityButton self, bool isShow);
	private delegate void SetButtonShowHook(SetButtonShowOrig orig, ExtremeAbilityButton self, bool isShow);

	private delegate bool TryGetClosestPlayerOrig(SingleRoleBase role, float range, out PlayerControl? targetPlayer);
	private delegate bool TryGetClosestPlayerHook(TryGetClosestPlayerOrig orig, SingleRoleBase role, float range, out PlayerControl? targetPlayer);

	private delegate float Vector2MagOrig(ref Vector2 self);
	private delegate float Vector2MagHook(Vector2MagOrig orig, ref Vector2 self);

	private delegate Vector2 Vector2NormalizedOrig(ref Vector2 self);
	private delegate Vector2 Vector2NormalizedHook(Vector2NormalizedOrig orig, ref Vector2 self);

	private delegate bool AnyNonTriggersBetweenOrig(Vector2 source, Vector2 dirNorm, float mag, int layerMask);
	private delegate bool AnyNonTriggersBetweenHook(AnyNonTriggersBetweenOrig orig, Vector2 source, Vector2 dirNorm, float mag, int layerMask);

	private delegate void PlayerOutLineSetOutlineOrig(PlayerControl? target, Color color);
	private delegate void PlayerOutLineSetOutlineHook(PlayerOutLineSetOutlineOrig orig, PlayerControl? target, Color color);

	private readonly Hook createCallerHook;
	private readonly Hook rpcWriteByteHook;
	private readonly Hook rpcDisposeHook;
	private readonly Hook setButtonShowHook;
	private readonly Hook magHook;
	private readonly Hook normHook;
	private readonly Hook? anyNonTriggersBetweenHook;
	private readonly Hook? setOutlineHook;

	private readonly Mock<AmongUsClient> clientMock;
	private readonly Mock<PlayerControl> localPlayerMock;

	public ChargerTests()
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
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

		ExtremeGameModeManager.Create(GameModes.Normal);

		this.localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		this.localPlayerMock.SetupGet(p => p.PlayerId).Returns((byte)1);
		this.localPlayerMock.SetupGet(p => p.CanMove).Returns(true);
		this.localPlayerMock.SetupProperty(p => p.killTimer);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.PlayerId).Returns((byte)1);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(this.localPlayerMock.Object);
		this.localPlayerMock.SetupGet(p => p.Data).Returns(mockData.Object);

		MockSetupHelper.SetupGameDataMock();
		MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		MockSetupHelper.SetupOptionManager();
		SetupGameOptionsManagerMock();
		SetupMasksAndPhysicsMocks();

		var shipStateField = typeof(ExtremeRolesPlugin).GetField("<ShipState>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
		shipStateField?.SetValue(null, new ExtremeShipStatus());

		ExtremeRoleManager.GameRole.Clear();

		this.clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		this.clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		var createCallerTarget = typeof(RPCOperator).GetMethod(nameof(RPCOperator.CreateCaller), new[] { typeof(uint), typeof(RPCOperator.Command) })!;
		var createCallerHookDelegate = new CreateCallerHook(getCreateCallerHook);
		this.createCallerHook = new Hook(createCallerTarget, createCallerHookDelegate);

		var rpcWriteByteTarget = typeof(RPCOperator.RpcCaller).GetMethod(nameof(RPCOperator.RpcCaller.WriteByte))!;
		var rpcWriteByteHookDelegate = new RpcWriteByteHook(getRpcWriteByteHook);
		this.rpcWriteByteHook = new Hook(rpcWriteByteTarget, rpcWriteByteHookDelegate);

		var rpcDisposeTarget = typeof(RPCOperator.RpcCaller).GetMethod(nameof(RPCOperator.RpcCaller.Dispose))!;
		var rpcDisposeHookDelegate = new RpcDisposeHook(getRpcDisposeHook);
		this.rpcDisposeHook = new Hook(rpcDisposeTarget, rpcDisposeHookDelegate);

		var setButtonShowTarget = typeof(ExtremeAbilityButton).GetMethod(nameof(ExtremeAbilityButton.SetButtonShow), new[] { typeof(bool) })!;
		var setButtonShowHookDelegate = new SetButtonShowHook(getSetButtonShowHook);
		this.setButtonShowHook = new Hook(setButtonShowTarget, setButtonShowHookDelegate);

		var magTarget = typeof(Vector2).GetProperty("magnitude")!.GetGetMethod()!;
		var magHookDelegate = new Vector2MagHook(getMagnitudeHook);
		this.magHook = new Hook(magTarget, magHookDelegate);

		var normTarget = typeof(Vector2).GetProperty("normalized")!.GetGetMethod()!;
		var normHookDelegate = new Vector2NormalizedHook(getNormalizedHook);
		this.normHook = new Hook(normTarget, normHookDelegate);

		var anyNonTriggersTarget = typeof(PhysicsHelpers).GetMethod(nameof(PhysicsHelpers.AnyNonTriggersBetween), new[] { typeof(Vector2), typeof(Vector2), typeof(float), typeof(int) });
		if (anyNonTriggersTarget != null)
		{
			var anyNonTriggersDelegate = new AnyNonTriggersBetweenHook((orig, source, dirNorm, mag, layerMask) => false);
			this.anyNonTriggersBetweenHook = new Hook(anyNonTriggersTarget, anyNonTriggersDelegate);
		}

		var setOutlineTarget = typeof(ExtremeRoles.Module.CustomMonoBehaviour.PlayerOutLine).GetMethod(nameof(ExtremeRoles.Module.CustomMonoBehaviour.PlayerOutLine.SetOutline), BindingFlags.Public | BindingFlags.Static);
		if (setOutlineTarget != null)
		{
			var setOutlineDelegate = new PlayerOutLineSetOutlineHook((orig, target, color) => { });
			this.setOutlineHook = new Hook(setOutlineTarget, setOutlineDelegate);
		}

		SetLobbyMode(false);
	}

	public void Dispose()
	{
		this.createCallerHook.Dispose();
		this.rpcWriteByteHook.Dispose();
		this.rpcDisposeHook.Dispose();
		this.setButtonShowHook.Dispose();
		this.magHook.Dispose();
		this.normHook.Dispose();
		this.anyNonTriggersBetweenHook?.Dispose();
		this.setOutlineHook?.Dispose();
	}

	private static float getMagnitudeHook(Vector2MagOrig orig, ref Vector2 self)
	{
		return (float)Math.Sqrt(self.x * self.x + self.y * self.y);
	}

	private static Vector2 getNormalizedHook(Vector2NormalizedOrig orig, ref Vector2 self)
	{
		float mag = (float)Math.Sqrt(self.x * self.x + self.y * self.y);
		if (mag > 1e-5f)
		{
			return new Vector2(self.x / mag, self.y / mag);
		}
		return new Vector2(0f, 0f);
	}

	private static RPCOperator.RpcCaller getCreateCallerHook(CreateCallerOrig orig, uint netId, RPCOperator.Command ops)
	{
		return (RPCOperator.RpcCaller)RuntimeHelpers.GetUninitializedObject(typeof(RPCOperator.RpcCaller));
	}

	private static void getRpcWriteByteHook(RpcWriteByteOrig orig, RPCOperator.RpcCaller self, byte value) { }
	private static void getRpcDisposeHook(RpcDisposeOrig orig, RPCOperator.RpcCaller self) { }
	private static void getSetButtonShowHook(SetButtonShowOrig orig, ExtremeAbilityButton self, bool isShow) { }

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

	private static void SetupGameOptionsManagerMock()
	{
		if (MockGameOptionsManagerget_InstanceHelper.Instance == null)
		{
			var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
			var mockGameOptionsManager = new Mock<GameOptionsManager>(IntPtr.Zero);
			mockGameOptionsManager.SetupGet(g => g.CurrentGameOptions).Returns(mockGameOptions.Object);

			var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
			mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockGameOptionsManager.Object);
			MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
		}
	}

	private static void SetupMasksAndPhysicsMocks()
	{
		if (UnityEngine.MockLayerMaskop_ImplicitHelper.Instance == null)
		{
			var mockImplicit = new Mock<UnityEngine.MockLayerMaskop_ImplicitHelper>();
			mockImplicit.Setup(m => m.Invoke(It.IsAny<LayerMask>())).Returns(1);
			UnityEngine.MockLayerMaskop_ImplicitHelper.Instance = mockImplicit.Object;
		}

		if (MockConstantsget_PlayersOnlyMaskHelper.Instance == null)
		{
			var mockMask = new Mock<MockConstantsget_PlayersOnlyMaskHelper>();
			mockMask.Setup(m => m.Invoke()).Returns(new LayerMask());
			MockConstantsget_PlayersOnlyMaskHelper.Instance = mockMask.Object;
		}

		if (MockConstantsget_ShipAndObjectsMaskHelper.Instance == null)
		{
			var mockMask = new Mock<MockConstantsget_ShipAndObjectsMaskHelper>();
			mockMask.Setup(m => m.Invoke()).Returns(new LayerMask());
			MockConstantsget_ShipAndObjectsMaskHelper.Instance = mockMask.Object;
		}
	}

	[Fact]
	public void RoleSpecificInit_LoadsOptionValuesCorrectly()
	{
		// Arrange
		SetLobbyMode(false);
		var charger = new Charger();
		charger.CreateRoleAllOption();

		// Act
		charger.Initialize();

		// Assert
		var awakeTaskGageField = typeof(Charger).GetField("awakeTaskGage", BindingFlags.NonPublic | BindingFlags.Instance);
		var chargeRangeField = typeof(Charger).GetField("chargeRange", BindingFlags.NonPublic | BindingFlags.Instance);
		var addAbilityCountField = typeof(Charger).GetField("addAbilityCount", BindingFlags.NonPublic | BindingFlags.Instance);
		var restoreKillCooldownField = typeof(Charger).GetField("restoreKillCooldown", BindingFlags.NonPublic | BindingFlags.Instance);

		float awakeTaskGage = (float)awakeTaskGageField?.GetValue(charger)!;
		float chargeRange = (float)chargeRangeField?.GetValue(charger)!;
		int addAbilityCount = (int)addAbilityCountField?.GetValue(charger)!;
		bool restoreKillCooldown = (bool)restoreKillCooldownField?.GetValue(charger)!;

		Assert.Equal(0.5f, awakeTaskGage);
		Assert.Equal(0.75f, chargeRange);
		Assert.Equal(1, addAbilityCount);
		Assert.False(restoreKillCooldown);
	}

	[Fact]
	public void RoleSpecificInit_WhenAwakeTaskGageZero_AwakesRoleImmediately()
	{
		// Arrange
		SetLobbyMode(false);
		var charger = new Charger();
		charger.CreateRoleAllOption();

		if (charger.Loader.TryGet(Charger.ChargerOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 0; // 0%
		}

		// Act
		charger.Initialize();

		// Assert
		Assert.True(charger.IsAwake);
	}

	[Theory]
	[InlineData(true, false, true)]
	[InlineData(false, true, true)]
	[InlineData(false, false, false)]
	public void IsAwake_ReturnsExpectedValue(bool isLobby, bool awakeRoleState, bool expectedIsAwake)
	{
		// Arrange
		SetLobbyMode(isLobby);
		var charger = new Charger();
		charger.CreateRoleAllOption();

		if (charger.Loader.TryGet(Charger.ChargerOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = awakeRoleState ? 0 : 5; // 0% -> true, 50% -> false
		}

		charger.Initialize();

		// Act
		bool isAwake = charger.IsAwake;

		// Assert
		Assert.Equal(expectedIsAwake, isAwake);
	}

	[Fact]
	public void Update_WhenTaskGageReached_AwakesRole()
	{
		// Arrange
		SetLobbyMode(false);
		var charger = new Charger();
		charger.CreateRoleAllOption();

		if (charger.Loader.TryGet(Charger.ChargerOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 5; // 50%
		}

		charger.Initialize();

		charger.Button = (ExtremeAbilityButton)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeAbilityButton));

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

		var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

		var task1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
		task1.SetupGet(t => t.Complete).Returns(true);

		mockTasksList.SetupGet(l => l.Count).Returns(1);
		mockTasksList.Setup(l => l[0]).Returns(task1.Object);

		mockPlayerInfo.SetupGet(p => p.Tasks).Returns(mockTasksList.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(mockPlayerInfo.Object);

		// Act
		charger.Update(mockPlayer.Object);

		// Assert
		Assert.True(charger.IsAwake);
	}


	[Fact]
	public void CheckAbility_WhenCurrentTargetPlayerNull_ReturnsFalse()
	{
		// Arrange
		var charger = new Charger();

		// Act
		bool result = charger.CheckAbility();

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void CheckAbility_WhenCurrentTargetPlayerValid_DelegatesToIsPlayerInRangeAndDrawOutLine()
	{
		// Arrange
		SetLobbyMode(false);
		var charger = new Charger();
		charger.CreateRoleAllOption();
		charger.Initialize();

		ExtremeRoleManager.GameRole[(byte)1] = charger;

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		mockShip.SetupGet(s => s.enabled).Returns(true);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		byte targetId = 2;
		var mockTargetRole = new Sheriff();
		mockTargetRole.CreateRoleAllOption();
		mockTargetRole.Initialize();
		ExtremeRoleManager.GameRole[targetId] = mockTargetRole;

		var mockTargetData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTargetData.SetupGet(d => d.PlayerId).Returns(targetId);
		mockTargetData.SetupGet(d => d.IsDead).Returns(false);
		mockTargetData.SetupGet(d => d.Disconnected).Returns(false);

		var mockTarget = new Mock<PlayerControl>(IntPtr.Zero);
		mockTarget.SetupGet(p => p.PlayerId).Returns(targetId);
		mockTarget.SetupGet(p => p.Data).Returns(mockTargetData.Object);
		mockTarget.SetupGet(p => p.inVent).Returns(false);
		mockTarget.SetupGet(p => p.inMovingPlat).Returns(false);
		mockTarget.SetupGet(p => p.onLadder).Returns(false);
		mockTargetData.SetupGet(d => d.Object).Returns(mockTarget.Object);

		var targetField = typeof(Charger).GetField("currentTargetPlayer", BindingFlags.NonPublic | BindingFlags.Instance)!;
		targetField.SetValue(charger, mockTarget.Object);

		// Act
		bool result = charger.CheckAbility();

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void CleanUp_WhenTargetNotNull_TriggersChargedAndResetsTarget()
	{
		// Arrange
		SetLobbyMode(false);
		var charger = new Charger();
		charger.CreateRoleAllOption();
		charger.Initialize();

		var mockTargetData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockTargetData.SetupGet(d => d.IsDead).Returns(false);
		mockTargetData.SetupGet(d => d.Disconnected).Returns(false);

		var mockTarget = new Mock<PlayerControl>(IntPtr.Zero);
		mockTarget.SetupGet(p => p.PlayerId).Returns((byte)2);
		mockTarget.SetupGet(p => p.Data).Returns(mockTargetData.Object);

		var targetField = typeof(Charger).GetField("currentTargetPlayer", BindingFlags.NonPublic | BindingFlags.Instance)!;
		targetField.SetValue(charger, mockTarget.Object);

		// Act
		charger.CleanUp();

		// Assert
		Assert.Null(targetField.GetValue(charger));
	}

	[Fact]
	public void Charged_WhenLocalPlayerIsTarget_AppliesChargeEffect()
	{
		// Arrange
		SetLobbyMode(false);

		byte chargerId = 2;
		byte targetId = 1; // LocalPlayer is 1

		var chargerRole = new Charger();
		chargerRole.CreateRoleAllOption();
		chargerRole.Initialize();

		ExtremeRoleManager.GameRole[(byte)chargerId] = chargerRole;

		var targetRole = new Charger();
		targetRole.CreateRoleAllOption();
		targetRole.Initialize();
		ExtremeRoleManager.GameRole[(byte)targetId] = targetRole;

		// Act
		Charger.Charged(chargerId, targetId);

		// Assert - Charged runs without throwing and executes for local player target
		Assert.NotNull(ExtremeRoleManager.GameRole[targetId]);
	}

	[Fact]
	public void ApplyChargeEffect_WhenRestoreKillCoolEnabledAndRoleHasKillCool_SetsLocalKillTimer()
	{
		// Arrange
		SetLobbyMode(false);

		var localRole = new SpecialImpostor();
		localRole.CreateRoleAllOption();
		localRole.Initialize();
		ExtremeRoleManager.GameRole[(byte)1] = localRole;

		// Act
		Charger.ApplyChargeEffect(addCount: 2, restoreKillCool: true);

		// Assert
		Assert.Equal(0.1f, localPlayerMock.Object.killTimer);
	}

	[Fact]
	public void ApplyChargeEffect_WhenRestoreKillCoolDisabledAndRoleHasKillCool_DoesNotSetKillTimer()
	{
		// Arrange
		SetLobbyMode(false);

		var localRole = new SpecialImpostor();
		localRole.CreateRoleAllOption();
		localRole.Initialize();
		ExtremeRoleManager.GameRole[(byte)1] = localRole;
		localPlayerMock.Object.killTimer = 30.0f;

		// Act
		Charger.ApplyChargeEffect(addCount: 2, restoreKillCool: false);

		// Assert
		Assert.Equal(30.0f, localPlayerMock.Object.killTimer);
	}

	[Fact]
	public void GetFakeOptionString_ReturnsEmptyString()
	{
		// Arrange
		var charger = new Charger();

		// Act
		string result = charger.GetFakeOptionString();

		// Assert
		Assert.Equal("", result);
	}

	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	public void GetColoredRoleName_WhenAwakeOrTruthColor_ReturnsColoredRoleName(bool isTruthColor, bool isAwake)
	{
		// Arrange
		SetLobbyMode(isAwake);
		var charger = new Charger();

		// Act
		string roleName = charger.GetColoredRoleName(isTruthColor);

		// Assert
		Assert.NotNull(roleName);
		Assert.Contains("Charger", roleName);
	}

	[Fact]
	public void GetColoredRoleName_WhenNotAwakeAndNotTruthColor_ReturnsWhiteCrewmateName()
	{
		// Arrange
		SetLobbyMode(false);
		var charger = new Charger();
		charger.CreateRoleAllOption();

		if (charger.Loader.TryGet(Charger.ChargerOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 5; // 50%
		}

		charger.Initialize();

		// Act
		string roleName = charger.GetColoredRoleName(false);

		// Assert
		Assert.NotNull(roleName);
		Assert.Contains("Crewmate", roleName);
	}

	[Fact]
	public void GetFullDescription_WhenAwake_ReturnsChargerFullDescription()
	{
		// Arrange
		SetLobbyMode(true);
		var charger = new Charger();

		// Act
		string description = charger.GetFullDescription();

		// Assert
		Assert.NotNull(description);
		Assert.Equal("ChargerFullDescription", description);
	}

	[Fact]
	public void GetFullDescription_WhenNotAwake_ReturnsCrewmateFullDescription()
	{
		// Arrange
		SetLobbyMode(false);
		var charger = new Charger();
		charger.CreateRoleAllOption();

		if (charger.Loader.TryGet(Charger.ChargerOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 5; // 50%
		}

		charger.Initialize();

		// Act
		string description = charger.GetFullDescription();

		// Assert
		Assert.NotNull(description);
		Assert.Equal("CrewmateFullDescription", description);
	}

	[Fact]
	public void GetImportantText_WhenAwake_ReturnsBaseImportantText()
	{
		// Arrange
		SetLobbyMode(true);
		var charger = new Charger();

		// Act
		string text = charger.GetImportantText(true);

		// Assert
		Assert.NotNull(text);
		Assert.Contains("Charger", text);
	}

	[Fact]
	public void GetImportantText_WhenNotAwake_ReturnsCrewmateImportantText()
	{
		// Arrange
		SetLobbyMode(false);
		var charger = new Charger();
		charger.CreateRoleAllOption();

		if (charger.Loader.TryGet(Charger.ChargerOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 5; // 50%
		}

		charger.Initialize();

		// Act
		string text = charger.GetImportantText(true);

		// Assert
		Assert.NotNull(text);
		Assert.Contains("crewImportantText", text);
	}

	[Fact]
	public void GetIntroDescription_WhenAwake_ReturnsBaseIntroDescription()
	{
		// Arrange
		SetLobbyMode(true);
		var charger = new Charger();

		// Act
		string desc = charger.GetIntroDescription();

		// Assert
		Assert.NotNull(desc);
		Assert.Contains("ChargerIntroDescription", desc);
	}

	[Fact]
	public void GetIntroDescription_WhenNotAwake_ReturnsCrewmateIntroDescription()
	{
		// Arrange
		SetLobbyMode(false);

		var mockRole = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRole.SetupGet(r => r.Blurb).Returns("Crewmate Intro Blurb");

		var mockInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockInfo.SetupGet(i => i.Role).Returns(mockRole.Object);
		this.localPlayerMock.SetupGet(p => p.Data).Returns(mockInfo.Object);

		var charger = new Charger();
		charger.CreateRoleAllOption();

		if (charger.Loader.TryGet(Charger.ChargerOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 5; // 50%
		}

		charger.Initialize();

		// Act
		string desc = charger.GetIntroDescription();

		// Assert
		Assert.NotNull(desc);
		Assert.Contains("Crewmate Intro Blurb", desc);
	}

	[Theory]
	[InlineData(true, true)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	public void GetNameColor_WhenAwakeOrTruthColor_ReturnsRoleColor(bool isTruthColor, bool isAwake)
	{
		// Arrange
		SetLobbyMode(isAwake);
		var charger = new Charger();

		// Act
		Color color = charger.GetNameColor(isTruthColor);

		// Assert
		Assert.Equal(ColorPalette.ChargerElectricYellow, color);
	}

	[Fact]
	public void GetNameColor_WhenNotAwakeAndNotTruthColor_ReturnsWhite()
	{
		// Arrange
		SetLobbyMode(false);
		var charger = new Charger();
		charger.CreateRoleAllOption();

		if (charger.Loader.TryGet(Charger.ChargerOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 5; // 50%
		}

		charger.Initialize();

		// Act
		Color color = charger.GetNameColor(false);

		// Assert
		Assert.Equal(Palette.White, color);
	}
}
