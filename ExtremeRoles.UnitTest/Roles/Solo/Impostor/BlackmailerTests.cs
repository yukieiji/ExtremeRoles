using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using MonoMod.RuntimeDetour;
using Moq;
using UnityEngine;
using UnityEngine.Events;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class BlackmailerTests : IDisposable
{
	private delegate float Vector2MagOrig(ref Vector2 self);
	private delegate float Vector2MagHook(Vector2MagOrig orig, ref Vector2 self);

	private delegate Vector2 Vector2NormOrig(ref Vector2 self);
	private delegate Vector2 NormHook(Vector2NormOrig orig, ref Vector2 self);

	private delegate bool AnyNonTriggersOrig(Vector2 start, Vector2 dir, float distance, int layerMask);
	private delegate bool AnyNonTriggersHook(AnyNonTriggersOrig orig, Vector2 start, Vector2 dir, float distance, int layerMask);

	private delegate void PlayerOutLineSetOutlineOrig(PlayerControl? target, Color color);
	private delegate void PlayerOutLineSetOutlineHook(PlayerOutLineSetOutlineOrig orig, PlayerControl? target, Color color);

	private readonly Hook magHook;
	private readonly Hook normHook;
	private readonly Hook physicsHook;
	private readonly Hook? setOutlineHook;

	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;
	private readonly List<NetworkedPlayerInfo> playerInfoList = new();

	public BlackmailerTests()
	{
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupVector3Helpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		if (ExtremeGameModeManager.Instance == null)
		{
			ExtremeGameModeManager.Create(GameModes.Normal);
		}

		SetupLobbyBehaviourMock();
		SetupTranslationControllerMock();
		SetupShipStatusMock();
		SetupConstantsMock();
		SetupSpriteCacheMock();
		SetupHudManagerMock();

		var magTarget = typeof(Vector2).GetProperty("magnitude")!.GetGetMethod()!;
		var magHookDelegate = new Vector2MagHook(getMagnitudeHook);
		this.magHook = new Hook(magTarget, magHookDelegate);

		var normTarget = typeof(Vector2).GetProperty("normalized")!.GetGetMethod()!;
		var normHookDelegate = new NormHook(getNormalizedHook);
		this.normHook = new Hook(normTarget, normHookDelegate);

		var physicsTarget = typeof(PhysicsHelpers).GetMethod(nameof(PhysicsHelpers.AnyNonTriggersBetween), new[] { typeof(Vector2), typeof(Vector2), typeof(float), typeof(int) })!;
		var physicsHookDelegate = new AnyNonTriggersHook((orig, start, dir, dist, mask) => false);
		this.physicsHook = new Hook(physicsTarget, physicsHookDelegate);

		var setOutlineTarget = typeof(PlayerOutLine).GetMethod(nameof(PlayerOutLine.SetOutline), BindingFlags.Public | BindingFlags.Static);
		if (setOutlineTarget != null)
		{
			var setOutlineDelegate = new PlayerOutLineSetOutlineHook((orig, target, color) => { });
			this.setOutlineHook = new Hook(setOutlineTarget, setOutlineDelegate);
		}

		this.mockClient = MockSetupHelper.SetupAmongUsClientMock();
		this.mockClient.SetupGet(c => c.AmHost).Returns(true);

		var writtenBytes = new List<byte>();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockWriter.Setup(w => w.Write(It.IsAny<byte>())).Callback((byte b) => writtenBytes.Add(b));
		mockWriter.Setup(w => w.ToByteArray(It.IsAny<bool>())).Returns(new Mock<Il2CppStructArray<byte>>(IntPtr.Zero).Object);

		this.mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		var cachedPtrField = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);

		this.mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		cachedPtrField?.SetValue(this.mockLocalPlayer.Object, (IntPtr)1);
		this.mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		this.mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);
		this.mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(10f, 20f));

		var mockLocalData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockLocalData.Object, (IntPtr)1);
		mockLocalData.SetupGet(d => d.PlayerId).Returns((byte)1);
		mockLocalData.SetupGet(d => d.IsDead).Returns(false);
		mockLocalData.SetupGet(d => d.Disconnected).Returns(false);
		mockLocalData.SetupGet(d => d.Object).Returns(this.mockLocalPlayer.Object);
		this.mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalData.Object);

		this.playerInfoList.Add(mockLocalData.Object);

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		var mockList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
		mockList.SetupGet(l => l.Count).Returns(() => this.playerInfoList.Count);
		mockList.Setup(l => l[It.IsAny<int>()]).Returns((int i) => this.playerInfoList[i]);
		mockGameData.SetupGet(g => g.AllPlayers).Returns(mockList.Object);

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

		var mockExileControllerHelper = new Mock<MockExileControllerget_InstanceHelper>();
		mockExileControllerHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockExileControllerget_InstanceHelper.Instance = mockExileControllerHelper.Object;

		var mockMinigameHelper = new Mock<MockMinigameget_InstanceHelper>();
		mockMinigameHelper.Setup(h => h.Invoke()).Returns((Minigame)null!);
		MockMinigameget_InstanceHelper.Instance = mockMinigameHelper.Object;

		ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
		GameProgressSystem.Current = GameProgressSystem.Progress.Task;

		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		var mockGet = new Mock<Hazel.MockMessageWriterGetHelper>();
		mockGet.Setup(g => g.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
		Hazel.MockMessageWriterGetHelper.Instance = mockGet.Object;

		var mockReaderHelper = new Mock<Hazel.MockMessageReaderGetHelper>();
		mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppStructArray<byte>>()))
			.Returns(() =>
			{
				var mockReader = new Mock<MessageReader>();
				if (writtenBytes.Count > 0)
				{
					var bytesCopy = new List<byte>(writtenBytes);
					writtenBytes.Clear();
					var seq = mockReader.SetupSequence(r => r.ReadByte());
					foreach (var b in bytesCopy)
					{
						seq.Returns(b);
					}
				}
				return mockReader.Object;
			});
		Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;

		var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
		mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNetObject>()));
		InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;

		if (ExtremeRolesPlugin.ShipState == null)
		{
			var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(
				nameof(ExtremeRolesPlugin.ShipState),
				BindingFlags.Public | BindingFlags.Static);
			shipStateProp?.SetValue(null, new ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus());
		}
	}

	public void Dispose()
	{
		this.magHook.Dispose();
		this.normHook.Dispose();
		this.physicsHook.Dispose();
		this.setOutlineHook?.Dispose();
	}

	private static float getMagnitudeHook(Vector2MagOrig orig, ref Vector2 self)
	{
		return (float)Math.Sqrt(self.x * self.x + self.y * self.y);
	}

	private static Vector2 getNormalizedHook(Vector2NormOrig orig, ref Vector2 self)
	{
		float mag = (float)Math.Sqrt(self.x * self.x + self.y * self.y);
		if (mag < 1e-5f) return Vector2.zero;
		return new Vector2(self.x / mag, self.y / mag);
	}

	private static void SetupSpriteCacheMock()
	{
		string spriteKey = $"{ObjectPath.TestButton}115";
		if (!LruCache<string, Sprite>.TryGetValue(spriteKey, out _))
		{
			var mockSprite = new Mock<Sprite>(IntPtr.Zero);
			LruCache<string, Sprite>.Add(spriteKey, mockSprite.Object);
		}
	}

	private static void SetupConstantsMock()
	{
		var mockMask1 = new Mock<MockConstantsget_PlayersOnlyMaskHelper>();
		mockMask1.Setup(m => m.Invoke()).Returns(0);
		MockConstantsget_PlayersOnlyMaskHelper.Instance = mockMask1.Object;

		var mockMask2 = new Mock<MockConstantsget_ShipAndObjectsMaskHelper>();
		mockMask2.Setup(m => m.Invoke()).Returns(0);
		MockConstantsget_ShipAndObjectsMaskHelper.Instance = mockMask2.Object;
	}

	private static void SetupShipStatusMock()
	{
		var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(x => x.Invoke()).Returns(mockShipStatus.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;
	}

	private static void SetupLobbyBehaviourMock()
	{
		var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
		var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
		mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
		MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
	}

	private static void SetupTranslationControllerMock()
	{
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
	}

	private static void SetupHudManagerMock()
	{
		if (MockVector3get_oneHelper.Instance == null)
		{
			var mockOne = new Mock<MockVector3get_oneHelper>();
			mockOne.Setup(x => x.Invoke()).Returns(new Vector3(1f, 1f, 1f));
			MockVector3get_oneHelper.Instance = mockOne.Object;
		}

		var mockObjectImplicitInt = new Mock<Il2CppSystem.MockObjectop_ImplicitHelper6>();
		mockObjectImplicitInt.Setup(x => x.Invoke(It.IsAny<int>())).Returns(new Mock<Il2CppSystem.Object>(IntPtr.Zero).Object);
		Il2CppSystem.MockObjectop_ImplicitHelper6.Instance = mockObjectImplicitInt.Object;

		var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
		mockGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));

		var mockGridArrange = new Mock<GridArrange>(IntPtr.Zero);
		mockGridArrange.Setup(g => g.ArrangeChilds());

		var mockParentGameObject = new Mock<GameObject>(IntPtr.Zero);
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
		mockLabelText.SetupGet(t => t.transform).Returns(mockTransform.Object);

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

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

		var mockHud = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		mockHud.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
		mockHud.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);
	}

	private static void InitializeRole(SingleRoleBase role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole[playerId] = role;
	}

	private PlayerControl CreateTargetPlayer(byte id = 2)
	{
		var cachedPtrField = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);

		var targetMock = new Mock<PlayerControl>(IntPtr.Zero);
		cachedPtrField?.SetValue(targetMock.Object, (IntPtr)id);
		targetMock.SetupGet(p => p.PlayerId).Returns(id);
		targetMock.Setup(p => p.GetTruePosition()).Returns(new Vector2(10f, 20f)); // same pos as local player

		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockData.Object, (IntPtr)id);
		mockData.SetupGet(d => d.PlayerId).Returns(id);
		mockData.SetupGet(d => d.IsDead).Returns(false);
		mockData.SetupGet(d => d.Disconnected).Returns(false);
		mockData.SetupGet(d => d.Object).Returns(targetMock.Object);
		targetMock.SetupGet(p => p.Data).Returns(mockData.Object);

		this.playerInfoList.Add(mockData.Object);

		// Register target role as Crewmate
		var crewRole = new SpecialCrew();
		InitializeRole(crewRole, id);

		return targetMock.Object;
	}

	[Fact]
	public void Initialize_RegistersBlackmailerRoleAndImpostorTeam()
	{
		// Arrange
		var role = new Blackmailer();

		// Act
		InitializeRole(role);

		// Assert
		Assert.Equal(ExtremeRoleId.Blackmailer, role.Core.Id);
		Assert.True(role.IsImpostor());
	}

	[Fact]
	public void CreateAbility_InitializesButton()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role);

		// Act
		role.CreateAbility();

		// Assert
		Assert.NotNull(role.Button);
	}

	[Fact]
	public void UseAbility_SetsTargetFromTmpTarget()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role);

		var target = CreateTargetPlayer(2);

		var tmpTargetField = typeof(Blackmailer).GetField("tmpTarget", BindingFlags.NonPublic | BindingFlags.Instance);
		tmpTargetField!.SetValue(role, target);

		// Act
		bool used = role.UseAbility();

		// Assert
		Assert.True(used);
		var targetField = typeof(Blackmailer).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.Same(target, targetField!.GetValue(role));
	}

	[Fact]
	public void IsAbilityCheck_WhenTargetNull_ReturnsFalse()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role);

		// Act
		bool check = role.IsAbilityCheck();

		// Assert
		Assert.False(check);
	}

	[Fact]
	public void IsAbilityCheck_WhenTargetSetAndInRange_ReturnsTrue()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role, playerId: 1);

		var target = CreateTargetPlayer(2);

		var targetField = typeof(Blackmailer).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
		targetField!.SetValue(role, target);

		// Act
		bool check = role.IsAbilityCheck();

		// Assert
		Assert.True(check);
	}

	[Fact]
	public void IsAbilityUse_WhenNoPlayerInRange_ReturnsFalse()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role);

		// Act
		bool canUse = role.IsAbilityUse();

		// Assert
		Assert.False(canUse);
	}

	[Fact]
	public void IsAbilityUse_WhenPlayerInRangeAndMultipleBlackmailDisabledAndAlreadyBlackmailed_ReturnsFalse()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role, playerId: 1);

		var target = CreateTargetPlayer(2);

		var hasBlackmailedField = typeof(Blackmailer).GetField("hasBlackmailedThisRound", BindingFlags.NonPublic | BindingFlags.Instance);
		hasBlackmailedField!.SetValue(role, true);

		// Act
		bool canUse = role.IsAbilityUse();

		// Assert
		Assert.False(canUse);
	}

	[Fact]
	public void IsAbilityUse_WhenPlayerInRangeAndNotInBlackmailedMap_ReturnsTrue()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role, playerId: 1);

		var target = CreateTargetPlayer(2);

		// Act
		bool canUse = role.IsAbilityUse();

		// Assert
		Assert.True(canUse);
	}

	[Fact]
	public void CleanUp_WhenMultipleBlackmailDisabled_ClearsPreviousAndAddsNewBlackmail()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role, playerId: 1);

		var system = BlackmailerSystem.GetOrRegister();
		system.Reset(ResetTiming.MeetingEnd);
		system.RpcAddBlackmail(1, 3); // 既存のブラックメールを追加

		var target = CreateTargetPlayer(2);

		var targetField = typeof(Blackmailer).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
		targetField!.SetValue(role, target);

		// Act
		role.CleanUp();

		// Assert
		var hasBlackmailedField = typeof(Blackmailer).GetField("hasBlackmailedThisRound", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.Null(targetField.GetValue(role));
		Assert.False(system.IsBlackmailedBy(1, 3)); // 前回分が削除されている
		Assert.True(system.IsBlackmailedBy(1, 2)); // 今回分が追加されている
		Assert.True((bool)hasBlackmailedField!.GetValue(role)!);
	}

	[Fact]
	public void CleanUp_WhenMultipleBlackmailEnabled_AddsNewBlackmailWithoutClearingPrevious()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role, playerId: 1);

		var multipleBlackmailField = typeof(Blackmailer).GetField("multipleBlackmail", BindingFlags.NonPublic | BindingFlags.Instance);
		multipleBlackmailField!.SetValue(role, true);

		var system = BlackmailerSystem.GetOrRegister();
		system.Reset(ResetTiming.MeetingEnd);
		system.RpcAddBlackmail(1, 3); // 既存のブラックメールを追加

		var target = CreateTargetPlayer(2);

		var targetField = typeof(Blackmailer).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
		targetField!.SetValue(role, target);

		// Act
		role.CleanUp();

		// Assert
		var hasBlackmailedField = typeof(Blackmailer).GetField("hasBlackmailedThisRound", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.Null(targetField.GetValue(role));
		Assert.True(system.IsBlackmailedBy(1, 3)); // 前回分が保持されている
		Assert.True(system.IsBlackmailedBy(1, 2)); // 今回分が追加されている
		Assert.True((bool)hasBlackmailedField!.GetValue(role)!);
	}

	[Fact]
	public void ForceCleanUp_ClearsTarget()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role);

		var targetMock = new Mock<PlayerControl>();
		var targetField = typeof(Blackmailer).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
		targetField!.SetValue(role, targetMock.Object);

		// Act
		role.ForceCleanUp();

		// Assert
		Assert.Null(targetField.GetValue(role));
	}

	[Fact]
	public void ResetOnMeetingEnd_ResetsHasBlackmailedThisRound()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role);

		var hasBlackmailedField = typeof(Blackmailer).GetField("hasBlackmailedThisRound", BindingFlags.NonPublic | BindingFlags.Instance);
		hasBlackmailedField!.SetValue(role, true);

		// Act
		role.ResetOnMeetingEnd(null);

		// Assert
		Assert.False((bool)hasBlackmailedField.GetValue(role)!);
	}

	[Fact]
	public void ExiledAction_ClearsBlackmailInSystem()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role, playerId: 1);

		var system = BlackmailerSystem.GetOrRegister();
		system.Reset(ResetTiming.MeetingEnd);
		system.RpcAddBlackmail(1, 2);

		var rolePlayerMock = new Mock<PlayerControl>();
		rolePlayerMock.SetupGet(p => p.PlayerId).Returns((byte)1);

		// Act
		role.ExiledAction(rolePlayerMock.Object);

		// Assert
		Assert.False(system.IsBlackmailedBy(1, 2));
	}

	[Fact]
	public void RolePlayerKilledAction_ClearsBlackmailInSystem()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role, playerId: 1);

		var system = BlackmailerSystem.GetOrRegister();
		system.Reset(ResetTiming.MeetingEnd);
		system.RpcAddBlackmail(1, 2);

		var rolePlayerMock = new Mock<PlayerControl>();
		rolePlayerMock.SetupGet(p => p.PlayerId).Returns((byte)1);
		var killerMock = new Mock<PlayerControl>();

		// Act
		role.RolePlayerKilledAction(rolePlayerMock.Object, killerMock.Object);

		// Assert
		Assert.False(system.IsBlackmailedBy(1, 2));
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenTargetIsBlackmailedBySelf_ReturnsTagWithBlackmailMark()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role, playerId: 1);

		var system = BlackmailerSystem.GetOrRegister();
		system.Reset(ResetTiming.MeetingEnd);
		system.RpcAddBlackmail(1, 2);

		var targetRole = new SpecialCrew();

		// Act
		string tag = role.GetRolePlayerNameTag(targetRole, 2);

		// Assert
		Assert.Contains("◆", tag);
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenTargetNotBlackmailedBySelf_ReturnsBaseTag()
	{
		// Arrange
		var role = new Blackmailer();
		InitializeRole(role, playerId: 1);

		var system = BlackmailerSystem.GetOrRegister();
		system.Reset(ResetTiming.MeetingEnd);

		var targetRole = new SpecialCrew();

		// Act
		string normalTag = role.GetRolePlayerNameTag(targetRole, 2);

		// Assert
		Assert.DoesNotContain("◆", normalTag);
	}
}
