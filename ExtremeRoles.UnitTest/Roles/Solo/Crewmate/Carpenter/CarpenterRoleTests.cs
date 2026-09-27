using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using ExtremeRoles.Compat;
using ExtremeRoles.Extension.Vector;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.Ability.ModeSwitcher;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using MonoMod.RuntimeDetour;
using Moq;
using TMPro;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.CarpenterTests;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class CarpenterRoleTests : IDisposable
{
	private delegate RPCOperator.RpcCaller CreateCallerOrig(uint netId, RPCOperator.Command ops);
	private delegate RPCOperator.RpcCaller CreateCallerHook(CreateCallerOrig orig, uint netId, RPCOperator.Command ops);

	private delegate void RpcWriteByteOrig(RPCOperator.RpcCaller self, byte value);
	private delegate void RpcWriteByteHook(RpcWriteByteOrig orig, RPCOperator.RpcCaller self, byte value);

	private delegate void RpcWriteFloatOrig(RPCOperator.RpcCaller self, float value);
	private delegate void RpcWriteFloatHook(RpcWriteFloatOrig orig, RPCOperator.RpcCaller self, float value);

	private delegate void RpcWriteIntOrig(RPCOperator.RpcCaller self, int value);
	private delegate void RpcWriteIntHook(RpcWriteIntOrig orig, RPCOperator.RpcCaller self, int value);

	private delegate void RpcDisposeOrig(RPCOperator.RpcCaller self);
	private delegate void RpcDisposeHook(RpcDisposeOrig orig, RPCOperator.RpcCaller self);

	private delegate void SetButtonShowOrig(ExtremeAbilityButton self, bool isShow);
	private delegate void SetButtonShowHook(SetButtonShowOrig orig, ExtremeAbilityButton self, bool isShow);

	private delegate void RemoveVentOrig(int ventId);
	private delegate void RemoveVentHook(RemoveVentOrig orig, int ventId);

	private delegate void SetCameraOrig(float x, float y, SystemTypes roomType);
	private delegate void SetCameraHook(SetCameraOrig orig, float x, float y, SystemTypes roomType);

	private delegate float Vector2DistanceOrig(Vector2 a, Vector2 b);
	private delegate float Vector2DistanceHook(Vector2DistanceOrig orig, Vector2 a, Vector2 b);

	private delegate float Vector2SqrMagOrig(ref Vector2 self);
	private delegate float Vector2SqrMagHook(Vector2SqrMagOrig orig, ref Vector2 self);

	private delegate void Vector2CtorOrig(ref Vector2 self, float x, float y);
	private delegate void Vector2CtorHook(Vector2CtorOrig orig, ref Vector2 self, float x, float y);

	private readonly Hook createCallerHook;
	private readonly Hook rpcWriteByteHook;
	private readonly Hook rpcWriteFloatHook;
	private readonly Hook rpcWriteIntHook;
	private readonly Hook rpcDisposeHook;
	private readonly Hook setButtonShowHook;
	private readonly Hook vector2DistanceHook;
	private readonly Hook sqrMagHook;
	private readonly Hook v2CtorHook;

	private readonly Mock<AmongUsClient> clientMock;
	private readonly Mock<PlayerControl> localPlayerMock;

	private static int? lastRemoveVentId;
	private static (float x, float y, SystemTypes roomType)? lastSetCameraArgs;

	public CarpenterRoleTests()
	{
		lastRemoveVentId = null;
		lastSetCameraArgs = null;

		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();

		if (MockVector3get_oneHelper.Instance == null)
		{
			var mockOne = new Mock<MockVector3get_oneHelper>();
			mockOne.Setup(x => x.Invoke()).Returns(new Vector3(1f, 1f, 1f));
			MockVector3get_oneHelper.Instance = mockOne.Object;
		}

		if (MockVector2op_SubtractionHelper.Instance == null)
		{
			var mockSub = new Mock<MockVector2op_SubtractionHelper>();
			mockSub.Setup(x => x.Invoke(It.IsAny<Vector2>(), It.IsAny<Vector2>()))
				.Returns((Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y));
			MockVector2op_SubtractionHelper.Instance = mockSub.Object;
		}

		if (MockVector2op_ImplicitHelper.Instance == null)
		{
			var mockImplicit = new Mock<MockVector2op_ImplicitHelper>();
			mockImplicit.Setup(x => x.Invoke(It.IsAny<Vector3>())).Returns((Vector3 v) => new Vector2(v.x, v.y));
			MockVector2op_ImplicitHelper.Instance = mockImplicit.Object;
		}

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns((ShipStatus)null!);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		this.localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		this.localPlayerMock.SetupGet(p => p.CanMove).Returns(true);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(this.localPlayerMock.Object);
		this.localPlayerMock.SetupGet(p => p.Data).Returns(mockData.Object);

		var mockPlayerTransform = new Mock<Transform>(IntPtr.Zero);
		mockPlayerTransform.SetupGet(t => t.position).Returns(new Vector3(0f, 0f, 0f));
		this.localPlayerMock.SetupGet(p => p.transform).Returns(mockPlayerTransform.Object);

		MockSetupHelper.SetupGameOptionsManagerMock();
		Mock.Get(GameOptionsManager.Instance.CurrentGameOptions).Setup(o => o.GetByte(ByteOptionNames.MapId)).Returns((byte)0); // Skeld default

		MockSetupHelper.SetupOptionManager();

		clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
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

		var rpcWriteFloatTarget = typeof(RPCOperator.RpcCaller).GetMethod(nameof(RPCOperator.RpcCaller.WriteFloat))!;
		var rpcWriteFloatHookDelegate = new RpcWriteFloatHook(getRpcWriteFloatHook);
		this.rpcWriteFloatHook = new Hook(rpcWriteFloatTarget, rpcWriteFloatHookDelegate);

		var rpcWriteIntTarget = typeof(RPCOperator.RpcCaller).GetMethod(nameof(RPCOperator.RpcCaller.WriteInt))!;
		var rpcWriteIntHookDelegate = new RpcWriteIntHook(getRpcWriteIntHook);
		this.rpcWriteIntHook = new Hook(rpcWriteIntTarget, rpcWriteIntHookDelegate);

		var rpcDisposeTarget = typeof(RPCOperator.RpcCaller).GetMethod(nameof(RPCOperator.RpcCaller.Dispose))!;
		var rpcDisposeHookDelegate = new RpcDisposeHook(getRpcDisposeHook);
		this.rpcDisposeHook = new Hook(rpcDisposeTarget, rpcDisposeHookDelegate);

		var setButtonShowTarget = typeof(ExtremeAbilityButton)
			.GetMethod(nameof(ExtremeAbilityButton.SetButtonShow), new[] { typeof(bool) })!;
		var setButtonShowHookDelegate = new SetButtonShowHook(getSetButtonShowHook);
		this.setButtonShowHook = new Hook(setButtonShowTarget, setButtonShowHookDelegate);

		var distanceTarget = typeof(Vector2).GetMethod(nameof(Vector2.Distance), new[] { typeof(Vector2), typeof(Vector2) })!;
		var distanceHookDelegate = new Vector2DistanceHook(getVector2DistanceHook);
		this.vector2DistanceHook = new Hook(distanceTarget, distanceHookDelegate);

		var sqrMagTarget = typeof(Vector2).GetProperty("sqrMagnitude")!.GetGetMethod()!;
		var sqrMagHookDelegate = new Vector2SqrMagHook(getSqrMagnitudeHook);
		this.sqrMagHook = new Hook(sqrMagTarget, sqrMagHookDelegate);

		var v2CtorTarget = typeof(Vector2).GetConstructor(new[] { typeof(float), typeof(float) })!;
		var v2CtorHookDelegate = new Vector2CtorHook(getV2CtorHook);
		this.v2CtorHook = new Hook(v2CtorTarget, v2CtorHookDelegate);

		SetLobbyMode(false);
	}

	public void Dispose()
	{
		this.createCallerHook.Dispose();
		this.rpcWriteByteHook.Dispose();
		this.rpcWriteFloatHook.Dispose();
		this.rpcWriteIntHook.Dispose();
		this.rpcDisposeHook.Dispose();
		this.setButtonShowHook.Dispose();
		this.vector2DistanceHook.Dispose();
		this.sqrMagHook.Dispose();
		this.v2CtorHook.Dispose();
	}

	private static RPCOperator.RpcCaller getCreateCallerHook(CreateCallerOrig orig, uint netId, RPCOperator.Command ops)
	{
		return (RPCOperator.RpcCaller)RuntimeHelpers.GetUninitializedObject(typeof(RPCOperator.RpcCaller));
	}

	private static void getRpcWriteByteHook(RpcWriteByteOrig orig, RPCOperator.RpcCaller self, byte value) { }
	private static void getRpcWriteFloatHook(RpcWriteFloatOrig orig, RPCOperator.RpcCaller self, float value) { }
	private static void getRpcWriteIntHook(RpcWriteIntOrig orig, RPCOperator.RpcCaller self, int value) { }
	private static void getRpcDisposeHook(RpcDisposeOrig orig, RPCOperator.RpcCaller self) { }

	private static void getSetButtonShowHook(SetButtonShowOrig orig, ExtremeAbilityButton self, bool isShow) { }

	private static void getRemoveVentHook(RemoveVentOrig orig, int ventId)
	{
		lastRemoveVentId = ventId;
	}

	private static void getSetCameraHook(SetCameraOrig orig, float x, float y, SystemTypes roomType)
	{
		lastSetCameraArgs = (x, y, roomType);
	}

	private static float getVector2DistanceHook(Vector2DistanceOrig orig, Vector2 a, Vector2 b)
	{
		float dx = a.x - b.x;
		float dy = a.y - b.y;
		return (float)Math.Sqrt(dx * dx + dy * dy);
	}

	private static float getSqrMagnitudeHook(Vector2SqrMagOrig orig, ref Vector2 self)
	{
		return self.x * self.x + self.y * self.y;
	}

	private static void getV2CtorHook(Vector2CtorOrig orig, ref Vector2 self, float x, float y)
	{
		self.x = x;
		self.y = y;
	}

	private static void SetupInstantiateFor(UnityEngine.Object result)
	{
		var m5 = new Mock<MockObjectInstantiateHelper5>();
		m5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>())).Returns(result);
		MockObjectInstantiateHelper5.Instance = m5.Object;

		var m10 = new Mock<MockObjectInstantiateHelper10>();
		m10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>())).Returns(result);
		MockObjectInstantiateHelper10.Instance = m10.Object;
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

	[Theory]
	[InlineData(true, false, true)]
	[InlineData(false, true, true)]
	[InlineData(false, false, false)]
	public void IsAwake_ReturnsExpectedValue(bool isLobby, bool awakeRoleState, bool expectedIsAwake)
	{
		// Arrange
		SetLobbyMode(isLobby);
		var role = new Carpenter();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Carpenter.CarpenterOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = awakeRoleState ? 0 : 7; // 0% -> awakeRole = true, 70% -> awakeRole = false
		}

		role.Initialize();

		// Act
		bool isAwake = role.IsAwake;

		// Assert
		Assert.Equal(expectedIsAwake, isAwake);
	}

	[Fact]
	public void RoleSpecificInit_WhenAwakeTaskGageIsZero_SetsAwakeRoleTrue()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Carpenter();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Carpenter.CarpenterOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 0; // 0%
		}

		// Act
		role.Initialize();

		// Assert
		Assert.True(role.IsAwake);
	}

	[Fact]
	public void RoleSpecificInit_WhenAwakeTaskGageIsGreaterThanZero_SetsAwakeRoleFalse()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Carpenter();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Carpenter.CarpenterOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		// Act
		role.Initialize();

		// Assert
		Assert.False(role.IsAwake);
	}

	[Fact]
	public void Update_WhenTaskGageReached_AwakesRole()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Carpenter();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Carpenter.CarpenterOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 5; // 50%
		}

		role.Initialize();

		role.Button = (ExtremeAbilityButton)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeAbilityButton));

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
		role.Update(mockPlayer.Object);

		// Assert
		Assert.True(role.IsAwake);
	}

	[Fact]
	public void GetFakeOptionString_ReturnsEmptyString()
	{
		// Arrange
		var role = new Carpenter();

		// Act
		string result = role.GetFakeOptionString();

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
		var role = new Carpenter();

		// Act
		string roleName = role.GetColoredRoleName(isTruthColor);

		// Assert
		Assert.NotNull(roleName);
		Assert.Contains("Carpenter", roleName);
	}

	[Fact]
	public void GetColoredRoleName_WhenNotAwakeAndNotTruthColor_ReturnsWhiteCrewmateName()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Carpenter();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Carpenter.CarpenterOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		string roleName = role.GetColoredRoleName(false);

		// Assert
		Assert.NotNull(roleName);
		Assert.Contains("Crewmate", roleName);
	}

	[Fact]
	public void GetFullDescription_WhenAwake_ReturnsCarpenterFullDescription()
	{
		// Arrange
		SetLobbyMode(true);
		var role = new Carpenter();

		// Act
		string description = role.GetFullDescription();

		// Assert
		Assert.NotNull(description);
		Assert.Equal("CarpenterFullDescription", description);
	}

	[Fact]
	public void GetFullDescription_WhenNotAwake_ReturnsCrewmateFullDescription()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Carpenter();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Carpenter.CarpenterOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		string description = role.GetFullDescription();

		// Assert
		Assert.NotNull(description);
		Assert.Equal("CrewmateFullDescription", description);
	}

	[Fact]
	public void GetImportantText_WhenAwake_ReturnsBaseImportantText()
	{
		// Arrange
		SetLobbyMode(true);
		var role = new Carpenter();

		// Act
		string text = role.GetImportantText(true);

		// Assert
		Assert.NotNull(text);
		Assert.Contains("Carpenter", text);
	}

	[Fact]
	public void GetImportantText_WhenNotAwake_ReturnsCrewmateImportantText()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Carpenter();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Carpenter.CarpenterOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		string text = role.GetImportantText(true);

		// Assert
		Assert.NotNull(text);
		Assert.Contains("crewImportantText", text);
	}

	[Fact]
	public void GetIntroDescription_WhenAwake_ReturnsBaseIntroDescription()
	{
		// Arrange
		SetLobbyMode(true);
		var role = new Carpenter();

		// Act
		string desc = role.GetIntroDescription();

		// Assert
		Assert.NotNull(desc);
		Assert.Contains("CarpenterIntroDescription", desc);
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

		var role = new Carpenter();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Carpenter.CarpenterOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		string desc = role.GetIntroDescription();

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
		var role = new Carpenter();

		// Act
		Color color = role.GetNameColor(isTruthColor);

		// Assert
		Assert.Equal(ColorPalette.CarpenterBrown, color);
	}

	[Fact]
	public void GetNameColor_WhenNotAwakeAndNotTruthColor_ReturnsWhite()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Carpenter();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Carpenter.CarpenterOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70%
		}

		role.Initialize();

		// Act
		Color color = role.GetNameColor(false);

		// Assert
		Assert.Equal(Palette.White, color);
	}

	[Fact]
	public void CreateRoleAllOption_CreatesExpectedOptions()
	{
		// Arrange
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Carpenter);
		var role = new Carpenter();

		// Act
		role.CreateRoleAllOption();

		// Assert
		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);
	}

	[Fact]
	public void ResetOnMeetingEnd_ResetsTargetVentToNull()
	{
		// Arrange
		var role = new Carpenter();
		var mockVent = new Mock<Vent>(IntPtr.Zero);
		var field = typeof(Carpenter).GetField("targetVent", BindingFlags.NonPublic | BindingFlags.Instance)!;
		field.SetValue(role, mockVent.Object);

		// Act
		role.ResetOnMeetingStart();
		role.ResetOnMeetingEnd(null);

		// Assert
		var ventVal = field.GetValue(role);
		Assert.Null(ventVal);
	}

	[Fact]
	public void RoleAbilityInit_WhenButtonNull_DoesNotThrow()
	{
		// Arrange
		var role = new Carpenter();

		// Act & Assert
		role.RoleAbilityInit();
	}

	[Fact]
	public void RoleAbilityInit_WhenButtonNotNull_InitializesBehaviorCoolTimeAndCount()
	{
		// Arrange
		var role = new Carpenter();
		role.CreateRoleAllOption();
		role.Initialize();

		var ventMode = new GraphicAndActiveTimeMode<Carpenter.CarpenterAbilityMode>(
			Carpenter.CarpenterAbilityMode.RemoveVent,
			new ButtonGraphic("VentSeal", null!),
			5.0f);

		var cameraMode = new GraphicAndActiveTimeMode<Carpenter.CarpenterAbilityMode>(
			Carpenter.CarpenterAbilityMode.SetCamera,
			new ButtonGraphic("CameraSet", null!),
			2.5f);

		var behavior = new Carpenter.CarpenterAbilityBehavior(
			ventMode, cameraMode, 10, 5,
			() => true, () => true, () => true, () => { }, () => true);

		var mockCoolTimerText = new Mock<TextMeshPro>(IntPtr.Zero);
		var mockParent = new Mock<Transform>(IntPtr.Zero);
		mockCoolTimerText.SetupGet(t => t.transform.parent).Returns(mockParent.Object);

		var mockText = new Mock<TextMeshPro>(IntPtr.Zero);
		var mockTextTransform = new Mock<Transform>(IntPtr.Zero);
		mockText.SetupGet(t => t.transform).Returns(mockTextTransform.Object);

		SetupInstantiateFor(mockText.Object);

		var mockButton = new Mock<ActionButton>(IntPtr.Zero);
		mockButton.SetupGet(b => b.cooldownTimerText).Returns(mockCoolTimerText.Object);

		behavior.Initialize(mockButton.Object);

		var uninitializedButton = (ExtremeAbilityButton)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeAbilityButton));
		typeof(ExtremeAbilityButton).GetProperty("Behavior")!.SetValue(uninitializedButton, behavior);

		role.Button = uninitializedButton;

		// Act
		role.RoleAbilityInit();

		// Assert
		Assert.Equal(15, behavior.AbilityCount);
		Assert.Equal(15.0f, role.Button.Behavior.CoolTime);
	}

	[Fact]
	public void IsAbilityCheck_WhenPlayerNotMoved_ReturnsTrue()
	{
		// Arrange
		var role = new Carpenter();
		var prevPosField = typeof(Carpenter).GetField("prevPos", BindingFlags.NonPublic | BindingFlags.Instance)!;
		prevPosField.SetValue(role, new Vector2(0.0f, 0.0f));

		// Act
		bool checkResult = role.IsAbilityCheck();

		// Assert
		Assert.True(checkResult);
	}

	[Fact]
	public void IsAbilityCheck_WhenPlayerMoved_ReturnsFalse()
	{
		// Arrange
		var role = new Carpenter();
		var prevPosField = typeof(Carpenter).GetField("prevPos", BindingFlags.NonPublic | BindingFlags.Instance)!;
		prevPosField.SetValue(role, new Vector2(100.0f, 100.0f));

		// Act
		bool checkResult = role.IsAbilityCheck();

		// Assert
		Assert.False(checkResult);
	}

	[Fact]
	public void CleanUp_WhenTargetVentNotNull_RemovesVentAndResetsTargetVent()
	{
		// Arrange
		var role = new Carpenter();
		var mockVent = new Mock<Vent>(IntPtr.Zero);
		mockVent.SetupGet(v => v.Id).Returns(123);

		var field = typeof(Carpenter).GetField("targetVent", BindingFlags.NonPublic | BindingFlags.Instance)!;
		field.SetValue(role, mockVent.Object);

		// Act
		role.CleanUp();

		// Assert
		var ventVal = field.GetValue(role);
		Assert.Null(ventVal);
	}

	[Fact]
	public void IsVentMode_WhenVentInUsableDistance_ReturnsTrueAndSetsTargetVent()
	{
		// Arrange
		var role = new Carpenter();

		var mockVentTransform = new Mock<Transform>(IntPtr.Zero);

		var mockSprite = new Mock<Sprite>(IntPtr.Zero);
		var mockRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockRenderer.SetupGet(r => r.sprite).Returns(mockSprite.Object);

		var mockVentGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockVentGameObject.SetupGet(g => g.active).Returns(true);

		var mockVent = new Mock<Vent>(IntPtr.Zero);
		mockVent.SetupGet(v => v.transform).Returns(mockVentTransform.Object);
		mockVent.SetupGet(v => v.UsableDistance).Returns(2.0f);
		mockVent.SetupGet(v => v.myRend).Returns(mockRenderer.Object);
		mockVent.SetupGet(v => v.gameObject).Returns(mockVentGameObject.Object);

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		mockShip.SetupGet(s => s.enabled).Returns(true);
		mockShip.SetupGet(s => s.AllVents).Returns(new Il2CppReferenceArray<Vent>([mockVent.Object]));

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		// Act
		bool isVentMode = role.IsVentMode();

		// Assert
		Assert.True(isVentMode);
		var targetVentField = typeof(Carpenter).GetField("targetVent", BindingFlags.NonPublic | BindingFlags.Instance)!;
		Assert.Equal(mockVent.Object, targetVentField.GetValue(role));
	}

	[Fact]
	public void IsVentMode_WhenNoVentInUsableDistance_ReturnsFalseAndClearsTargetVent()
	{
		// Arrange
		var role = new Carpenter();

		var mockVentTransform = new Mock<Transform>(IntPtr.Zero);

		var mockSprite = new Mock<Sprite>(IntPtr.Zero);
		var mockRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockRenderer.SetupGet(r => r.sprite).Returns(mockSprite.Object);

		var mockVentGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockVentGameObject.SetupGet(g => g.active).Returns(true);

		var mockVent = new Mock<Vent>(IntPtr.Zero);
		mockVent.SetupGet(v => v.transform).Returns(mockVentTransform.Object);
		mockVent.SetupGet(v => v.UsableDistance).Returns(-1.0f); // UsableDistance is -1, so 0 <= UsableDistance is false
		mockVent.SetupGet(v => v.myRend).Returns(mockRenderer.Object);
		mockVent.SetupGet(v => v.gameObject).Returns(mockVentGameObject.Object);

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		mockShip.SetupGet(s => s.enabled).Returns(true);
		mockShip.SetupGet(s => s.AllVents).Returns(new Il2CppReferenceArray<Vent>([mockVent.Object]));

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		// Act
		bool isVentMode = role.IsVentMode();

		// Assert
		Assert.False(isVentMode);
		var targetVentField = typeof(Carpenter).GetField("targetVent", BindingFlags.NonPublic | BindingFlags.Instance)!;
		Assert.Null(targetVentField.GetValue(role));
	}

	[Fact]
	public void IsVentMode_WhenShipStatusNull_ReturnsFalse()
	{
		// Arrange
		var role = new Carpenter();

		// Act
		bool isVentMode = role.IsVentMode();

		// Assert
		Assert.False(isVentMode);
	}

	[Fact]
	public void IsAbilityUse_WhenAwakeAndSkeld_ReturnsTrue()
	{
		// Arrange
		SetLobbyMode(false);
		Mock.Get(GameOptionsManager.Instance.CurrentGameOptions).Setup(o => o.GetByte(ByteOptionNames.MapId)).Returns((byte)0); // Skeld

		this.localPlayerMock.SetupGet(p => p.CanMove).Returns(true);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(this.localPlayerMock.Object);
		this.localPlayerMock.SetupGet(p => p.Data).Returns(mockData.Object);

		var role = new Carpenter();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Carpenter.CarpenterOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 0; // 0% -> awake
		}

		role.Initialize();

		// Act
		bool canUse = role.IsAbilityUse();

		// Assert
		Assert.True(canUse);
	}

	[Fact]
	public void IsAbilityUse_WhenNotAwake_ReturnsFalse()
	{
		// Arrange
		SetLobbyMode(false);
		var role = new Carpenter();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Carpenter.CarpenterOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 7; // 70% -> not awake
		}

		role.Initialize();

		// Act
		bool canUse = role.IsAbilityUse();

		// Assert
		Assert.False(canUse);
	}

	[Fact]
	public void IsAbilityUse_WhenTargetVentNullAndMapIsMiraHQ_ReturnsFalse()
	{
		// Arrange
		SetLobbyMode(false);
		Mock.Get(GameOptionsManager.Instance.CurrentGameOptions).Setup(o => o.GetByte(ByteOptionNames.MapId)).Returns((byte)1); // Mira HQ

		var role = new Carpenter();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(Carpenter.CarpenterOption.AwakeTaskGage, out var option) && option != null)
		{
			option.Selection = 0; // 0% -> awake
		}

		role.Initialize();

		// Act
		bool canUse = role.IsAbilityUse();

		// Assert
		Assert.False(canUse);
	}

	[Fact]
	public void UpdateMapObject_RemoveVent_InvokesRemoveVentMethod()
	{
		// Arrange
		var removeVentTarget = typeof(Carpenter).GetMethod("removeVent", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(int) }, null)!;
		var removeVentHookDelegate = new RemoveVentHook(getRemoveVentHook);
		using var removeVentHook = new Hook(removeVentTarget, removeVentHookDelegate);

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)Carpenter.AbilityType.RemoveVent);
		mockReader.SetupSequence(r => r.ReadInt32())
			.Returns(789);

		var readerObj = mockReader.Object;

		// Act
		Carpenter.UpdateMapObject(ref readerObj);

		// Assert
		Assert.Equal(789, lastRemoveVentId);
	}

	[Fact]
	public void UpdateMapObject_SetCamera_InvokesSetCameraMethod()
	{
		// Arrange
		var setCameraTarget = typeof(Carpenter).GetMethod("setCamera", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(float), typeof(float), typeof(SystemTypes) }, null)!;
		var setCameraHookDelegate = new SetCameraHook(getSetCameraHook);
		using var setCameraHook = new Hook(setCameraTarget, setCameraHookDelegate);

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)Carpenter.AbilityType.SetCamera)
			.Returns((byte)SystemTypes.Cafeteria);

		mockReader.SetupSequence(r => r.ReadSingle())
			.Returns(3.5f)
			.Returns(4.5f);

		var readerObj = mockReader.Object;

		// Act
		Carpenter.UpdateMapObject(ref readerObj);

		// Assert
		Assert.NotNull(lastSetCameraArgs);
		Assert.Equal(3.5f, lastSetCameraArgs.Value.x);
		Assert.Equal(4.5f, lastSetCameraArgs.Value.y);
		Assert.Equal(SystemTypes.Cafeteria, lastSetCameraArgs.Value.roomType);
	}

	[Fact]
	public void RemoveVent_UnlinksAdjacentVentAndClearsSprite()
	{
		// Arrange
		int targetVentId = 100;
		int rightVentId = 200;

		var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		var mockSprite = new Mock<Sprite>(IntPtr.Zero);
		mockSpriteRenderer.SetupProperty(r => r.sprite, mockSprite.Object);

		var rightVentMock = new Mock<Vent>(IntPtr.Zero);
		rightVentMock.SetupGet(v => v.Id).Returns(rightVentId);

		var targetVentMock = new Mock<Vent>(IntPtr.Zero);
		targetVentMock.SetupGet(v => v.Id).Returns(targetVentId);
		targetVentMock.SetupProperty(v => v.Right, rightVentMock.Object);
		targetVentMock.SetupGet(v => v.myRend).Returns(mockSpriteRenderer.Object);

		rightVentMock.SetupProperty(v => v.Right, targetVentMock.Object);

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		mockShip.SetupGet(s => s.AllVents).Returns(new Il2CppReferenceArray<Vent>([targetVentMock.Object, rightVentMock.Object]));

		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		var removeVentMethod = typeof(Carpenter).GetMethod("removeVent", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(int) }, null)!;

		// Act
		removeVentMethod.Invoke(null, new object[] { targetVentId });

		// Assert
		Assert.Null(rightVentMock.Object.Right);
		Assert.Null(targetVentMock.Object.myRend.sprite);
	}

	[Fact]
	public void CarpenterAbilityBehavior_ConstructorAndProperties_InitializesCorrectly()
	{
		// Arrange
		var ventMode = new GraphicAndActiveTimeMode<Carpenter.CarpenterAbilityMode>(
			Carpenter.CarpenterAbilityMode.RemoveVent,
			new ButtonGraphic("VentSeal", null!),
			5.0f);

		var cameraMode = new GraphicAndActiveTimeMode<Carpenter.CarpenterAbilityMode>(
			Carpenter.CarpenterAbilityMode.SetCamera,
			new ButtonGraphic("CameraSet", null!),
			2.5f);

		// Act
		var behavior = new Carpenter.CarpenterAbilityBehavior(
			ventMode,
			cameraMode,
			10,
			5,
			() => true,
			() => true,
			() => true,
			() => { },
			() => true);

		// Assert
		Assert.NotNull(behavior);
		Assert.Equal(0, behavior.AbilityCount);
		Assert.Equal(0.0f, behavior.ActiveTime);
	}

	[Fact]
	public void CarpenterAbilityBehavior_SetAbilityCount_UpdatesCountAndFlag()
	{
		// Arrange
		var ventMode = new GraphicAndActiveTimeMode<Carpenter.CarpenterAbilityMode>(
			Carpenter.CarpenterAbilityMode.RemoveVent,
			new ButtonGraphic("VentSeal", null!),
			5.0f);

		var cameraMode = new GraphicAndActiveTimeMode<Carpenter.CarpenterAbilityMode>(
			Carpenter.CarpenterAbilityMode.SetCamera,
			new ButtonGraphic("CameraSet", null!),
			2.5f);

		var behavior = new Carpenter.CarpenterAbilityBehavior(
			ventMode, cameraMode, 10, 5,
			() => true, () => true, () => true, () => { }, () => true);

		var mockCoolTimerText = new Mock<TextMeshPro>(IntPtr.Zero);
		var mockParent = new Mock<Transform>(IntPtr.Zero);
		mockCoolTimerText.SetupGet(t => t.transform.parent).Returns(mockParent.Object);

		var mockText = new Mock<TextMeshPro>(IntPtr.Zero);
		var mockTextTransform = new Mock<Transform>(IntPtr.Zero);
		mockText.SetupGet(t => t.transform).Returns(mockTextTransform.Object);

		SetupInstantiateFor(mockText.Object);

		var mockButton = new Mock<ActionButton>(IntPtr.Zero);
		mockButton.SetupGet(b => b.cooldownTimerText).Returns(mockCoolTimerText.Object);

		behavior.Initialize(mockButton.Object);

		// Act
		behavior.SetAbilityCount(15);

		// Assert
		Assert.Equal(15, behavior.AbilityCount);

		// Updating behavior in non-activating state should return CoolDown because isUpdate was set to true
		var newState = behavior.Update(AbilityState.Ready);
		Assert.Equal(AbilityState.CoolDown, newState);
	}

	[Fact]
	public void CarpenterAbilityBehavior_AbilityOff_DeductsScrewsAndInvokesUpdateMapObj()
	{
		// Arrange
		bool updateMapObjCalled = false;
		var ventMode = new GraphicAndActiveTimeMode<Carpenter.CarpenterAbilityMode>(
			Carpenter.CarpenterAbilityMode.RemoveVent,
			new ButtonGraphic("VentSeal", null!),
			5.0f);

		var cameraMode = new GraphicAndActiveTimeMode<Carpenter.CarpenterAbilityMode>(
			Carpenter.CarpenterAbilityMode.SetCamera,
			new ButtonGraphic("CameraSet", null!),
			2.5f);

		var behavior = new Carpenter.CarpenterAbilityBehavior(
			ventMode, cameraMode, 10, 5,
			() => true, () => true, () => true, () => { updateMapObjCalled = true; }, () => true);

		var mockCoolTimerText = new Mock<TextMeshPro>(IntPtr.Zero);
		var mockParent = new Mock<Transform>(IntPtr.Zero);
		mockCoolTimerText.SetupGet(t => t.transform.parent).Returns(mockParent.Object);

		var mockText = new Mock<TextMeshPro>(IntPtr.Zero);
		var mockTextTransform = new Mock<Transform>(IntPtr.Zero);
		mockText.SetupGet(t => t.transform).Returns(mockTextTransform.Object);

		SetupInstantiateFor(mockText.Object);

		var mockButton = new Mock<ActionButton>(IntPtr.Zero);
		mockButton.SetupGet(b => b.cooldownTimerText).Returns(mockCoolTimerText.Object);

		behavior.Initialize(mockButton.Object);

		behavior.SetAbilityCount(15);

		// Act
		behavior.AbilityOff();

		// Assert
		Assert.Equal(5, behavior.AbilityCount); // 15 - 10 (ventRemoveScrewNum)
		Assert.True(updateMapObjCalled);
	}

	[Fact]
	public void CarpenterAbilityBehavior_IsUse_ReturnsExpectedBasedOnScrewsAndCanUse()
	{
		// Arrange
		bool canUse = true;
		bool isVentMode = true;

		var ventMode = new GraphicAndActiveTimeMode<Carpenter.CarpenterAbilityMode>(
			Carpenter.CarpenterAbilityMode.RemoveVent,
			new ButtonGraphic("VentSeal", null!),
			5.0f);

		var cameraMode = new GraphicAndActiveTimeMode<Carpenter.CarpenterAbilityMode>(
			Carpenter.CarpenterAbilityMode.SetCamera,
			new ButtonGraphic("CameraSet", null!),
			2.5f);

		var behavior = new Carpenter.CarpenterAbilityBehavior(
			ventMode, cameraMode, 10, 5,
			() => true, () => canUse, () => true, () => { }, () => isVentMode);

		var mockCoolTimerText = new Mock<TextMeshPro>(IntPtr.Zero);
		var mockParent = new Mock<Transform>(IntPtr.Zero);
		mockCoolTimerText.SetupGet(t => t.transform.parent).Returns(mockParent.Object);

		var mockText = new Mock<TextMeshPro>(IntPtr.Zero);
		var mockTextTransform = new Mock<Transform>(IntPtr.Zero);
		mockText.SetupGet(t => t.transform).Returns(mockTextTransform.Object);

		SetupInstantiateFor(mockText.Object);

		var mockButton = new Mock<ActionButton>(IntPtr.Zero);
		mockButton.SetupGet(b => b.cooldownTimerText).Returns(mockCoolTimerText.Object);

		behavior.Initialize(mockButton.Object);

		// Act & Assert 1: AbilityCount = 0 -> false
		behavior.SetAbilityCount(0);
		Assert.False(behavior.IsUse());

		// Act & Assert 2: AbilityCount = 8, ventRemoveScrewNum = 10 -> false (not enough screws for vent)
		behavior.SetAbilityCount(8);
		Assert.False(behavior.IsUse());

		// Act & Assert 3: AbilityCount = 10, ventRemoveScrewNum = 10 -> true
		behavior.SetAbilityCount(10);
		Assert.True(behavior.IsUse());

		// Act & Assert 4: canUse = false -> false
		canUse = false;
		Assert.False(behavior.IsUse());
	}

	[Fact]
	public void CarpenterAbilityBehavior_TryUseAbility_ReturnsExpected()
	{
		// Arrange
		bool setCountStartResult = true;
		var ventMode = new GraphicAndActiveTimeMode<Carpenter.CarpenterAbilityMode>(
			Carpenter.CarpenterAbilityMode.RemoveVent,
			new ButtonGraphic("VentSeal", null!),
			5.0f);

		var cameraMode = new GraphicAndActiveTimeMode<Carpenter.CarpenterAbilityMode>(
			Carpenter.CarpenterAbilityMode.SetCamera,
			new ButtonGraphic("CameraSet", null!),
			2.5f);

		var behavior = new Carpenter.CarpenterAbilityBehavior(
			ventMode, cameraMode, 10, 5,
			() => setCountStartResult, () => true, () => true, () => { }, () => true);

		var mockCoolTimerText = new Mock<TextMeshPro>(IntPtr.Zero);
		var mockParent = new Mock<Transform>(IntPtr.Zero);
		mockCoolTimerText.SetupGet(t => t.transform.parent).Returns(mockParent.Object);

		var mockText = new Mock<TextMeshPro>(IntPtr.Zero);
		var mockTextTransform = new Mock<Transform>(IntPtr.Zero);
		mockText.SetupGet(t => t.transform).Returns(mockTextTransform.Object);

		SetupInstantiateFor(mockText.Object);

		var mockButton = new Mock<ActionButton>(IntPtr.Zero);
		mockButton.SetupGet(b => b.cooldownTimerText).Returns(mockCoolTimerText.Object);

		behavior.Initialize(mockButton.Object);
		behavior.SetAbilityCount(10);

		// Act 1: Timer > 0 -> returns false
		bool success1 = behavior.TryUseAbility(1.0f, AbilityState.Ready, out var newState1);
		Assert.False(success1);
		Assert.Equal(AbilityState.Ready, newState1);

		// Act 2: ActiveTime > 0 -> transitions to Activating
		behavior.ActiveTime = 5.0f;
		bool success2 = behavior.TryUseAbility(0.0f, AbilityState.Ready, out var newState2);
		Assert.True(success2);
		Assert.Equal(AbilityState.Activating, newState2);

		// Act 3: ActiveTime <= 0 -> transitions to CoolDown
		behavior.ActiveTime = 0.0f;
		bool success3 = behavior.TryUseAbility(0.0f, AbilityState.Ready, out var newState3);
		Assert.True(success3);
		Assert.Equal(AbilityState.CoolDown, newState3);

		// Act 4: setCountStart returns false -> returns false
		setCountStartResult = false;
		bool success4 = behavior.TryUseAbility(0.0f, AbilityState.Ready, out var newState4);
		Assert.False(success4);
	}
}
