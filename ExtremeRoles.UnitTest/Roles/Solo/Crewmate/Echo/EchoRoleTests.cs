using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using UnityEngine.Events;
using Xunit;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;

using EchoRole = ExtremeRoles.Roles.Solo.Crewmate.Echo;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.EchoTests;

public static class MonoBehaviourExtensionsStartCoroutinePatch
{
	public static bool Prefix(ref Coroutine? __result)
	{
		__result = null;
		return false;
	}
}

public static class UnityObjectLoaderLoadFromResourcesPatch
{
	public static bool Prefix(ref Sprite __result)
	{
		var mockSprite = new Mock<Sprite>(IntPtr.Zero);
		__result = mockSprite.Object;
		return false;
	}
}

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class EchoRoleTests
{
	private readonly Mock<AmongUsClient> clientMock;
	private readonly Mock<HudManager> hudMock;
	private static bool isHarmonyPatched = false;

	public EchoRoleTests()
	{
		SetupHarmony();
		SetupIl2CppClassPointers();

		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.CanMove).Returns(true);

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(localPlayerMock.Object);
		localPlayerMock.SetupGet(p => p.Data).Returns(mockData.Object);

		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		hudMock = SetupHudManagerMock();

		clientMock = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		if (Hazel.MockMessageWriterGetHelper.Instance == null)
		{
			var mockGet = new Mock<Hazel.MockMessageWriterGetHelper>();
			mockGet.Setup(g => g.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
			Hazel.MockMessageWriterGetHelper.Instance = mockGet.Object;
		}

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation
			.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);

		SetupGameManagerCreatorMock();
		SetupAssetBundleAndResourceMocks();
	}

	private static void SetupIl2CppClassPointers()
	{
		try
		{
			if (Il2CppInterop.Runtime.Il2CppClassPointerStore<Sprite>.NativeClassPtr == IntPtr.Zero)
			{
				Il2CppInterop.Runtime.Il2CppClassPointerStore<Sprite>.NativeClassPtr = (IntPtr)12345;
			}
			if (Il2CppInterop.Runtime.Il2CppClassPointerStore<GameObject>.NativeClassPtr == IntPtr.Zero)
			{
				Il2CppInterop.Runtime.Il2CppClassPointerStore<GameObject>.NativeClassPtr = (IntPtr)12345;
			}
		}
		catch { }
	}

	private static void SetupHarmony()
	{
		if (!isHarmonyPatched)
		{
			try
			{
				var harmony = new Harmony("test.echo.patches");

				var targetCoroutineMethod = typeof(BepInEx.Unity.IL2CPP.Utils.MonoBehaviourExtensions)
					.GetMethod("StartCoroutine", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(MonoBehaviour), typeof(IEnumerator) }, null);
				var prefixCoroutineMethod = typeof(MonoBehaviourExtensionsStartCoroutinePatch)
					.GetMethod(nameof(MonoBehaviourExtensionsStartCoroutinePatch.Prefix), BindingFlags.Public | BindingFlags.Static);

				if (targetCoroutineMethod != null && prefixCoroutineMethod != null)
				{
					harmony.Patch(targetCoroutineMethod, prefix: new HarmonyMethod(prefixCoroutineMethod));
				}

				var targetLoaderMethod = typeof(UnityObjectLoader)
					.GetMethod("LoadFromResources", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(string), typeof(Assembly) }, null)
					?.MakeGenericMethod(typeof(Sprite));
				var prefixLoaderMethod = typeof(UnityObjectLoaderLoadFromResourcesPatch)
					.GetMethod(nameof(UnityObjectLoaderLoadFromResourcesPatch.Prefix), BindingFlags.Public | BindingFlags.Static);

				if (targetLoaderMethod != null && prefixLoaderMethod != null)
				{
					harmony.Patch(targetLoaderMethod, prefix: new HarmonyMethod(prefixLoaderMethod));
				}
			}
			catch { }
			isHarmonyPatched = true;
		}
	}

	private static Mock<HudManager> SetupHudManagerMock()
	{
		MockSetupHelper.SetupVector3Helpers();

		var mockOne = new Mock<MockVector3get_oneHelper>();
		mockOne.Setup(x => x.Invoke()).Returns(new Vector3(1f, 1f, 1f));
		MockVector3get_oneHelper.Instance = mockOne.Object;

		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));

		var mockGridArrange = new Mock<GridArrange>(IntPtr.Zero);
		mockGridArrange.Setup(g => g.ArrangeChilds());

		var mockParentGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockParentGameObject.SetupGet(t => t.gameObject).Returns(mockParentGameObject.Object);
		mockParentGameObject.Setup(g => g.GetComponent<GridArrange>()).Returns(mockGridArrange.Object);

		var mockParentTransform = new Mock<Transform>(IntPtr.Zero);
		mockParentTransform.SetupGet(t => t.gameObject).Returns(mockParentGameObject.Object);

		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		mockTransform.SetupGet(t => t.parent).Returns(mockParentTransform.Object);
		mockTransform.Setup(t => t.FindChild(It.IsAny<string>())).Returns((Transform)null!);

		var mockMaterial = new Mock<Material>(IntPtr.Zero);
		mockMaterial.Setup(m => m.SetFloat(It.IsAny<string>(), It.IsAny<float>()));

		var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
		mockSpriteRenderer.SetupProperty(s => s.sprite);
		mockSpriteRenderer.SetupProperty(s => s.color);
		mockSpriteRenderer.SetupProperty(s => s.enabled);
		mockSpriteRenderer.SetupGet(s => s.material).Returns(mockMaterial.Object);

		var mockLabelText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
		mockLabelText.SetupProperty(t => t.color);
		mockLabelText.SetupProperty(t => t.fontMaterial);
		mockLabelText.SetupProperty(t => t.text);

		var mockCoolText = new Mock<TMPro.TextMeshPro>(IntPtr.Zero);
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

		var fieldCalls = typeof(UnityEventBase).GetField("m_PersistentCalls", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		fieldCalls?.SetValue(mockOnClick.Object, mockPersistentCallGroup.Object);

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

		typeof(ExtremeRoles.Extension.Manager.HudManagerExtension)
			.GetField("cachedArrange", BindingFlags.NonPublic | BindingFlags.Static)
			?.SetValue(null, null);

		var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		hudMock.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
		hudMock.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);
		hudMock.Setup(h => h.StartCoroutine(It.IsAny<Il2CppSystem.Collections.IEnumerator>()))
			.Returns((Coroutine)null!);

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

		return hudMock;
	}

	private static void SetupAssetBundleAndResourceMocks()
	{
		var mockAssetBundle = new Mock<AssetBundle>(IntPtr.Zero);
		var mockSprite = new Mock<Sprite>(IntPtr.Zero);

		mockAssetBundle.Setup(b => b.LoadAsset(It.IsAny<string>(), It.IsAny<Il2CppSystem.Type>()))
			.Returns(mockSprite.Object);

		var field = typeof(UnityObjectLoader).GetField("cachedBundle", BindingFlags.NonPublic | BindingFlags.Static);
		if (field?.GetValue(null) is Dictionary<string, AssetBundle> dict)
		{
			dict["ExtremeRoles.Resources.echo.asset"] = mockAssetBundle.Object;
			dict["echo"] = mockAssetBundle.Object;
			dict[ObjectPath.SoundEffect] = mockAssetBundle.Object;
		}

		string spriteKey = "echo115";
		if (!LruCache<string, Sprite>.TryGetValue(spriteKey, out _))
		{
			LruCache<string, Sprite>.Add(spriteKey, mockSprite.Object);
		}
	}

	private static void SetupGameManagerCreatorMock()
	{
		var mockGameManagerCreator = new Mock<GameManagerCreator>(IntPtr.Zero);
		var mockHideAndSeek = new Mock<HideAndSeekManager>(IntPtr.Zero);
		var mockPool = new Mock<ObjectPoolBehavior>(IntPtr.Zero);
		mockHideAndSeek.SetupGet(h => h.PingPool).Returns(mockPool.Object);
		mockGameManagerCreator.SetupGet(g => g.HideAndSeekManagerPrefab).Returns(mockHideAndSeek.Object);

		var mockHelper = new Mock<MockGameManagerCreatorget_InstanceHelper>();
		mockHelper.Setup(x => x.Invoke()).Returns(mockGameManagerCreator.Object);
		MockGameManagerCreatorget_InstanceHelper.Instance = mockHelper.Object;
	}

	private static EchoRole CreateInitializedEcho(
		float range = 10.0f,
		float showTime = 2.0f,
		EchoRole.EmitAttentionMode mode = EchoRole.EmitAttentionMode.EmitAll,
		bool isDetectDeadBody = false,
		bool canSeparatePlayer = false)
	{
		var role = new EchoRole();
		role.CreateRoleAllOption();

		if (role.Loader.TryGet(EchoRole.Option.Range, out var rangeOpt) && rangeOpt != null)
		{
			rangeOpt.Selection = (int)((range - 5.0f) / 0.5f);
		}
		if (role.Loader.TryGet(EchoRole.Option.ShowTime, out var timeOpt) && timeOpt != null)
		{
			timeOpt.Selection = (int)((showTime - 0.5f) / 0.25f);
		}
		if (role.Loader.TryGet(EchoRole.Option.AttentionMode, out var modeOpt) && modeOpt != null)
		{
			modeOpt.Selection = (int)mode;
		}
		if (role.Loader.TryGet(EchoRole.Option.IsDetectDeadBody, out var deadOpt) && deadOpt != null)
		{
			deadOpt.Selection = isDetectDeadBody ? 1 : 0;
		}
		if (role.Loader.TryGet(EchoRole.Option.CanSeparatePlayer, out var sepOpt) && sepOpt != null)
		{
			sepOpt.Selection = canSeparatePlayer ? 1 : 0;
		}

		role.Initialize();
		SetupMockPool(role);
		return role;
	}

	private static Mock<ObjectPoolBehavior> SetupMockPool(EchoRole role)
	{
		var mockPool = new Mock<ObjectPoolBehavior>(IntPtr.Zero);
		var mockList = new Mock<Il2CppSystem.Collections.Generic.List<PoolableBehavior>>(IntPtr.Zero);
		mockList.SetupGet(l => l.Count).Returns(0);

		var mockEnumerator = new Mock<Il2CppSystem.Collections.Generic.List<PoolableBehavior>.Enumerator>(IntPtr.Zero);
		mockEnumerator.Setup(e => e.MoveNext()).Returns(false);
		mockList.Setup(l => l.GetEnumerator()).Returns(mockEnumerator.Object);

		mockPool.SetupGet(p => p.activeChildren).Returns(mockList.Object);

		var field = typeof(EchoRole).GetField("innerPool", BindingFlags.NonPublic | BindingFlags.Instance);
		field?.SetValue(role, mockPool.Object);

		return mockPool;
	}

	[Fact]
	public void RoleSpecificInit_SetsExpectedPropertiesFromLoader()
	{
		// Arrange & Act
		float expectedRange = 15.0f;
		float expectedShowTime = 4.0f;
		EchoRole.EmitAttentionMode expectedMode = EchoRole.EmitAttentionMode.EmitNotCrewmate;

		var role = CreateInitializedEcho(
			range: expectedRange,
			showTime: expectedShowTime,
			mode: expectedMode,
			isDetectDeadBody: true,
			canSeparatePlayer: true);

		// Assert
		var rangeSqrField = typeof(EchoRole).GetField("echoLocationRangeSquare", BindingFlags.NonPublic | BindingFlags.Instance);
		var timeField = typeof(EchoRole).GetField("pingTime", BindingFlags.NonPublic | BindingFlags.Instance);
		var deadBodyField = typeof(EchoRole).GetField("isDetectDeadBody", BindingFlags.NonPublic | BindingFlags.Instance);
		var separateField = typeof(EchoRole).GetField("canSeparatePlayer", BindingFlags.NonPublic | BindingFlags.Instance);
		var modeField = typeof(EchoRole).GetField("mode", BindingFlags.NonPublic | BindingFlags.Instance);

		float actualRangeSqr = (float)(rangeSqrField?.GetValue(role) ?? 0f);
		float actualShowTime = (float)(timeField?.GetValue(role) ?? 0f);
		bool actualIsDetectDeadBody = (bool)(deadBodyField?.GetValue(role) ?? false);
		bool actualCanSeparatePlayer = (bool)(separateField?.GetValue(role) ?? false);
		EchoRole.EmitAttentionMode actualMode = (EchoRole.EmitAttentionMode)(modeField?.GetValue(role) ?? EchoRole.EmitAttentionMode.EmitAll);

		Assert.Equal(expectedRange * expectedRange, actualRangeSqr);
		Assert.Equal(expectedShowTime, actualShowTime);
		Assert.True(actualIsDetectDeadBody);
		Assert.True(actualCanSeparatePlayer);
		Assert.Equal(expectedMode, actualMode);
	}

	[Fact]
	public void CreateAbility_CreatesAbilityCountButton()
	{
		// Arrange
		var role = CreateInitializedEcho();

		// Act
		role.CreateAbility();

		// Assert
		Assert.NotNull(role.Button);
	}

	[Fact]
	public void IsAbilityUse_ReturnsCommonUseValue()
	{
		// Arrange
		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
		GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;

		var role = CreateInitializedEcho();

		// Act
		bool isUse = role.IsAbilityUse();

		// Assert
		Assert.True(isUse);
	}

	[Fact]
	public void UseAbility_WhenAttentionModeIsNotDisableKey_SendsRpcAndStartsCoroutineAndReturnsTrue()
	{
		// Arrange
		byte localPlayerId = 1;
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns(localPlayerId);
		localPlayerMock.SetupGet(p => p.NetId).Returns(100u);

		var role = CreateInitializedEcho(mode: EchoRole.EmitAttentionMode.EmitAll);
		clientMock.Invocations.Clear();

		// Act
		bool result = role.UseAbility();

		// Assert
		Assert.True(result);
		clientMock.Verify(c => c.StartRpcImmediately(100u, (byte)RPCOperator.Command.EchoOps, It.IsAny<SendOption>(), It.IsAny<int>()), Times.Once());

		var coroutineField = typeof(EchoRole).GetField("coroutine", BindingFlags.NonPublic | BindingFlags.Instance);
		var coroutineList = coroutineField?.GetValue(role) as IList;
		Assert.NotNull(coroutineList);
		Assert.Single(coroutineList);
	}

	[Fact]
	public void UseAbility_WhenAttentionModeIsDisableKey_DoesNotSendRpcAndStartsCoroutineAndReturnsTrue()
	{
		// Arrange
		byte localPlayerId = 1;
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns(localPlayerId);
		localPlayerMock.SetupGet(p => p.NetId).Returns(100u);

		var role = CreateInitializedEcho(mode: EchoRole.EmitAttentionMode.DisableKey);
		clientMock.Invocations.Clear();

		// Act
		bool result = role.UseAbility();

		// Assert
		Assert.True(result);
		clientMock.Verify(c => c.StartRpcImmediately(100u, (byte)RPCOperator.Command.EchoOps, It.IsAny<SendOption>(), It.IsAny<int>()), Times.Never());

		var coroutineField = typeof(EchoRole).GetField("coroutine", BindingFlags.NonPublic | BindingFlags.Instance);
		var coroutineList = coroutineField?.GetValue(role) as IList;
		Assert.NotNull(coroutineList);
		Assert.Single(coroutineList);
	}

	[Fact]
	public void ResetOnMeetingStart_WhenAttentionModeIsNotDisableKey_SendsResetRpcAndResetsState()
	{
		// Arrange
		byte localPlayerId = 1;
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns(localPlayerId);
		localPlayerMock.SetupGet(p => p.NetId).Returns(100u);

		var role = CreateInitializedEcho(mode: EchoRole.EmitAttentionMode.EmitAll);
		role.UseAbility();

		var coroutineField = typeof(EchoRole).GetField("coroutine", BindingFlags.NonPublic | BindingFlags.Instance);
		var coroutineListBefore = coroutineField?.GetValue(role) as IList;
		Assert.NotNull(coroutineListBefore);
		Assert.NotEmpty(coroutineListBefore);

		clientMock.Invocations.Clear();

		// Act
		role.ResetOnMeetingStart();

		// Assert
		clientMock.Verify(c => c.StartRpcImmediately(100u, (byte)RPCOperator.Command.EchoOps, It.IsAny<SendOption>(), It.IsAny<int>()), Times.Once());

		var coroutineListAfter = coroutineField?.GetValue(role) as IList;
		Assert.NotNull(coroutineListAfter);
		Assert.Empty(coroutineListAfter);
	}

	[Fact]
	public void ResetOnMeetingStart_WhenAttentionModeIsDisableKey_DoesNotSendResetRpcAndResetsState()
	{
		// Arrange
		byte localPlayerId = 1;
		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns(localPlayerId);
		localPlayerMock.SetupGet(p => p.NetId).Returns(100u);

		var role = CreateInitializedEcho(mode: EchoRole.EmitAttentionMode.DisableKey);
		role.UseAbility();

		clientMock.Invocations.Clear();

		// Act
		role.ResetOnMeetingStart();

		// Assert
		clientMock.Verify(c => c.StartRpcImmediately(100u, (byte)RPCOperator.Command.EchoOps, It.IsAny<SendOption>(), It.IsAny<int>()), Times.Never());

		var coroutineField = typeof(EchoRole).GetField("coroutine", BindingFlags.NonPublic | BindingFlags.Instance);
		var coroutineListAfter = coroutineField?.GetValue(role) as IList;
		Assert.NotNull(coroutineListAfter);
		Assert.Empty(coroutineListAfter);
	}

	[Fact]
	public void ResetOnMeetingEnd_DoesNotThrow()
	{
		// Arrange
		var role = CreateInitializedEcho();

		// Act & Assert
		role.ResetOnMeetingEnd(null);
	}

	[Fact]
	public void Rpc_WhenEchoRoleNotFound_DoesNotChangeAnyState()
	{
		// Arrange
		ExtremeRoleManager.GameRole.Clear();

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)99) // playerId without Echo role
			.Returns((byte)EchoRole.RpcOps.Emit);

		// Act
		EchoRole.Rpc(mockReader.Object);

		// Assert
		Assert.False(ExtremeRoleManager.TryGetRole(99, out _));
	}

	[Theory]
	[InlineData(EchoRole.EmitAttentionMode.EmitAll, true, true)]
	[InlineData(EchoRole.EmitAttentionMode.EmitAll, false, true)]
	[InlineData(EchoRole.EmitAttentionMode.EmitNotCrewmate, true, false)] // Local is Crewmate -> false
	[InlineData(EchoRole.EmitAttentionMode.EmitNotCrewmate, false, true)] // Local is Impostor -> true
	[InlineData(EchoRole.EmitAttentionMode.DisableKey, true, false)]
	[InlineData(EchoRole.EmitAttentionMode.DisableKey, false, false)]
	public void Rpc_Emit_EvaluatesVisibilityAndStartsCoroutineWhenVisible(
		EchoRole.EmitAttentionMode mode,
		bool isLocalCrewmate,
		bool expectedShowPing)
	{
		// Arrange
		byte echoPlayerId = 1;
		byte localPlayerId = 2;

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns(localPlayerId);

		var echoRole = CreateInitializedEcho(mode: mode);

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[echoPlayerId] = echoRole;

		if (isLocalCrewmate)
		{
			var crewRole = new SpecialCrew();
			crewRole.CreateRoleAllOption();
			crewRole.Initialize();
			ExtremeRoleManager.GameRole[localPlayerId] = crewRole;
		}
		else
		{
			var impRole = new SpecialImpostor();
			impRole.CreateRoleAllOption();
			impRole.Initialize();
			ExtremeRoleManager.GameRole[localPlayerId] = impRole;
		}

		var mockPool = SetupMockPool(echoRole);
		var mockPing = new Mock<PingBehaviour>(IntPtr.Zero);
		var mockTransform = new Mock<Transform>(IntPtr.Zero);
		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);

		mockPing.SetupGet(p => p.transform).Returns(mockTransform.Object);
		mockPing.SetupGet(p => p.gameObject).Returns(mockGameObject.Object);

		mockPool.Setup(p => p.Get<PingBehaviour>()).Returns(mockPing.Object);

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns(echoPlayerId)
			.Returns((byte)EchoRole.RpcOps.Emit);
		mockReader.SetupSequence(r => r.ReadSingle())
			.Returns(5.0f)
			.Returns(10.0f);

		var coroutineField = typeof(EchoRole).GetField("coroutine", BindingFlags.NonPublic | BindingFlags.Instance);
		var coroutinesBefore = (coroutineField?.GetValue(echoRole) as IList)?.Count ?? 0;

		// Act
		EchoRole.Rpc(mockReader.Object);

		// Assert
		var coroutinesAfter = (coroutineField?.GetValue(echoRole) as IList)?.Count ?? 0;
		if (expectedShowPing)
		{
			Assert.Equal(coroutinesBefore + 1, coroutinesAfter);
		}
		else
		{
			Assert.Equal(coroutinesBefore, coroutinesAfter);
		}
	}

	[Fact]
	public void Rpc_Reset_CallsEchoResetAndClearsCoroutines()
	{
		// Arrange
		byte echoPlayerId = 1;
		var echoRole = CreateInitializedEcho();

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[echoPlayerId] = echoRole;

		echoRole.UseAbility();

		var coroutineField = typeof(EchoRole).GetField("coroutine", BindingFlags.NonPublic | BindingFlags.Instance);
		var coroutineListBefore = coroutineField?.GetValue(echoRole) as IList;
		Assert.NotNull(coroutineListBefore);
		Assert.NotEmpty(coroutineListBefore);

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns(echoPlayerId)
			.Returns((byte)EchoRole.RpcOps.Reset);

		// Act
		EchoRole.Rpc(mockReader.Object);

		// Assert
		var coroutineListAfter = coroutineField?.GetValue(echoRole) as IList;
		Assert.NotNull(coroutineListAfter);
		Assert.Empty(coroutineListAfter);
	}

	[Fact]
	public void AddPlayerLocationInfo_FiltersNearbyPlayersAndExcludesSelfAndInvalidPlayers()
	{
		// Arrange
		PlayerCache.RemovePlayerControl(_ => true);

		byte sourceId = 1;
		byte targetNearbyId = 2;
		byte targetFarId = 3;

		var sourceTransform = new Mock<Transform>(IntPtr.Zero);
		sourceTransform.SetupGet(t => t.position).Returns(new Vector3(0f, 0f, 0f));

		var nearbyTransform = new Mock<Transform>(IntPtr.Zero);
		nearbyTransform.SetupGet(t => t.position).Returns(new Vector3(1f, 1f, 0f));

		var farTransform = new Mock<Transform>(IntPtr.Zero);
		farTransform.SetupGet(t => t.position).Returns(new Vector3(100f, 100f, 0f));

		var sourceData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		sourceData.SetupGet(d => d.IsDead).Returns(false);
		sourceData.SetupGet(d => d.Disconnected).Returns(false);

		var nearbyData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		nearbyData.SetupGet(d => d.IsDead).Returns(false);
		nearbyData.SetupGet(d => d.Disconnected).Returns(false);

		var farData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		farData.SetupGet(d => d.IsDead).Returns(false);
		farData.SetupGet(d => d.Disconnected).Returns(false);

		var sourceMock = new Mock<PlayerControl>(IntPtr.Zero);
		sourceMock.SetupGet(p => p.PlayerId).Returns(sourceId);
		sourceMock.SetupGet(p => p.transform).Returns(sourceTransform.Object);
		sourceMock.SetupGet(p => p.Data).Returns(sourceData.Object);
		sourceMock.Setup(p => p.GetTruePosition()).Returns(new Vector2(0f, 0f));

		var targetNearbyMock = new Mock<PlayerControl>(IntPtr.Zero);
		targetNearbyMock.SetupGet(p => p.PlayerId).Returns(targetNearbyId);
		targetNearbyMock.SetupGet(p => p.transform).Returns(nearbyTransform.Object);
		targetNearbyMock.SetupGet(p => p.Data).Returns(nearbyData.Object);
		targetNearbyMock.Setup(p => p.GetTruePosition()).Returns(new Vector2(1f, 1f));

		var targetFarMock = new Mock<PlayerControl>(IntPtr.Zero);
		targetFarMock.SetupGet(p => p.PlayerId).Returns(targetFarId);
		targetFarMock.SetupGet(p => p.transform).Returns(farTransform.Object);
		targetFarMock.SetupGet(p => p.Data).Returns(farData.Object);
		targetFarMock.Setup(p => p.GetTruePosition()).Returns(new Vector2(100f, 100f));

		sourceData.SetupGet(d => d.Object).Returns(sourceMock.Object);
		nearbyData.SetupGet(d => d.Object).Returns(targetNearbyMock.Object);
		farData.SetupGet(d => d.Object).Returns(targetFarMock.Object);

		PlayerCache.AddPlayerControl(sourceMock.Object);
		PlayerCache.AddPlayerControl(targetNearbyMock.Object);
		PlayerCache.AddPlayerControl(targetFarMock.Object);

		var echoRole = CreateInitializedEcho(range: 10.0f);

		var addPlayerLocationInfoMethod = typeof(EchoRole).GetMethod("addPlayerLocationInfo", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(addPlayerLocationInfoMethod);

		// Act
		var resultListType = typeof(List<>).MakeGenericType(typeof(EchoRole).GetNestedType("LocationInfo", BindingFlags.NonPublic)!);
		var resultList = Activator.CreateInstance(resultListType)!;

		addPlayerLocationInfoMethod.Invoke(echoRole, new object[] { sourceMock.Object, resultList });

		// Assert
		var countProp = resultListType.GetProperty("Count");
		int count = (int)(countProp?.GetValue(resultList) ?? 0);
		Assert.Equal(1, count);
	}
}
