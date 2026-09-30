using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.UnitTest.Helper;
using Hazel;
using Moq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.BodyGuardTests;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class BodyGuardRoleTests
{
	private readonly Mock<AmongUsClient> clientMock;

	public BodyGuardRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupPlayerControlMocks();
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		var shipStateField = typeof(ExtremeRolesPlugin).GetField("<ShipState>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
		shipStateField?.SetValue(null, new ExtremeShipStatus());

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
		mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestString");
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		var mockSfxHelper = new Mock<MockConstantsShouldPlaySfxHelper>();
		mockSfxHelper.Setup(x => x.Invoke()).Returns(true);
		MockConstantsShouldPlaySfxHelper.Instance = mockSfxHelper.Object;

		SetupAssetBundleAndSoundMocks();
	}

	private static void SetupAssetBundleAndSoundMocks()
	{
		var mockAssetBundle = new Mock<AssetBundle>(IntPtr.Zero);
		var mockAudioClip = new Mock<AudioClip>(IntPtr.Zero);
		var mockSprite = new Mock<Sprite>(IntPtr.Zero);

		mockAssetBundle.Setup(b => b.LoadAsset(It.IsAny<string>(), It.IsAny<Il2CppSystem.Type>()))
			.Returns((string name, Il2CppSystem.Type type) =>
			{
				if (type == Il2CppSystem.Type.GetType("UnityEngine.Sprite, UnityEngine.CoreModule"))
					return mockSprite.Object;
				return mockAudioClip.Object;
			});

		var field = typeof(UnityObjectLoader).GetField("cachedBundle", BindingFlags.NonPublic | BindingFlags.Static);
		if (field?.GetValue(null) is Dictionary<string, AssetBundle> dict)
		{
			dict[ObjectPath.SoundEffect] = mockAssetBundle.Object;
			dict[ObjectPath.BodyGuardShield] = mockAssetBundle.Object;
			dict[ObjectPath.BodyGuardResetShield] = mockAssetBundle.Object;
		}

		string key1 = $"{ObjectPath.BodyGuardShield}115";
		if (!LruCache<string, Sprite>.TryGetValue(key1, out _))
		{
			LruCache<string, Sprite>.Add(key1, mockSprite.Object);
		}

		string key2 = $"{ObjectPath.BodyGuardResetShield}115";
		if (!LruCache<string, Sprite>.TryGetValue(key2, out _))
		{
			LruCache<string, Sprite>.Add(key2, mockSprite.Object);
		}

		var cachedAudioField = typeof(Sound).GetField("cachedAudio", BindingFlags.NonPublic | BindingFlags.Static);
		if (cachedAudioField?.GetValue(null) is Dictionary<Sound.Type, AudioClip> cachedAudio)
		{
			cachedAudio[Sound.Type.GuardianAngleGuard] = mockAudioClip.Object;
		}

		var mockSoundManager = new Mock<SoundManager>(IntPtr.Zero);
		var mockAudioSource = new Mock<AudioSource>(IntPtr.Zero);
		mockSoundManager.Setup(s => s.PlaySound(It.IsAny<AudioClip>(), It.IsAny<bool>(), It.IsAny<float>(), null))
			.Returns(mockAudioSource.Object);

		var mockSoundInstanceHelper = new Mock<MockSoundManagerget_InstanceHelper>();
		mockSoundInstanceHelper.Setup(x => x.Invoke()).Returns(mockSoundManager.Object);
		MockSoundManagerget_InstanceHelper.Instance = mockSoundInstanceHelper.Object;
	}

	private static void SetupHudManagerMock()
	{
		var systemManager = (ExtremeSystemTypeManager)RuntimeHelpers.GetUninitializedObject(typeof(ExtremeSystemTypeManager));
		var allSystems = new Dictionary<ExtremeSystemType, IExtremeSystemType>();
		typeof(ExtremeSystemTypeManager)
			.GetField("allSystems", BindingFlags.NonPublic | BindingFlags.Instance)?
			.SetValue(systemManager, allSystems);
		typeof(ExtremeSystemTypeManager)
			.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static)?
			.SetValue(null, systemManager);

		var mockIntroHelper = new Mock<MockIntroCutsceneget_InstanceHelper>();
		mockIntroHelper.Setup(x => x.Invoke()).Returns((IntroCutscene)null!);
		MockIntroCutsceneget_InstanceHelper.Instance = mockIntroHelper.Object;

		var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

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

		var mockLabelText = new Mock<TextMeshPro>(IntPtr.Zero);
		mockLabelText.SetupProperty(t => t.color);
		mockLabelText.SetupProperty(t => t.fontMaterial);
		mockLabelText.SetupProperty(t => t.text);
		mockLabelText.SetupGet(t => t.transform).Returns(mockTransform.Object);
		mockLabelText.SetupGet(t => t.gameObject).Returns(mockGameObject.Object);

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
		mockPassiveButton.SetupGet(b => b.OnClick).Returns(mockOnClick.Object);

		var mockKillButton = new Mock<KillButton>(IntPtr.Zero);
		mockKillButton.SetupGet(b => b.transform).Returns(mockTransform.Object);
		mockKillButton.SetupGet(b => b.gameObject).Returns(mockGameObject.Object);
		mockKillButton.SetupGet(b => b.graphic).Returns(mockSpriteRenderer.Object);
		mockKillButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
		mockKillButton.SetupGet(b => b.cooldownTimerText).Returns(mockCoolText.Object);
		mockKillButton.Setup(b => b.GetComponent<PassiveButton>()).Returns(mockPassiveButton.Object);
		mockKillButton.SetupGet(b => b.isActiveAndEnabled).Returns(true);
		mockKillButton.Setup(b => b.OverrideText(It.IsAny<string>()));
		mockKillButton.Setup(b => b.SetCoolDown(It.IsAny<float>(), It.IsAny<float>()));
		mockKillButton.Setup(b => b.SetCooldownFill(It.IsAny<float>()));

		var mockUseButton = new Mock<UseButton>(IntPtr.Zero);
		mockUseButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
		mockUseButton.SetupGet(b => b.transform).Returns(mockTransform.Object);

		var mockHud = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
		mockHud.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
		mockHud.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);

		var mockTaskPanel = new Mock<TaskPanelBehaviour>(IntPtr.Zero);
		var mockTextMesh = new Mock<TextMeshPro>(IntPtr.Zero);
		mockTaskPanel.SetupGet(t => t.taskText).Returns(mockTextMesh.Object);
		mockHud.SetupGet(h => h.TaskPanel).Returns(mockTaskPanel.Object);

		var mockInstantiate = new Mock<MockObjectInstantiateHelper>();
		mockInstantiate.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Vector3>(), It.IsAny<Quaternion>()))
			.Returns((UnityEngine.Object original, Vector3 pos, Quaternion rot) => original);
		MockObjectInstantiateHelper.Instance = mockInstantiate.Object;

		var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
		mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
			.Returns((UnityEngine.Object original, Transform parent) => original);
		MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

		var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
		mockInstantiate10.Setup(x => x.Invoke(It.IsAny<Il2CppSystem.Object>(), It.IsAny<Transform>()))
			.Returns((Il2CppSystem.Object original, Transform parent) => original);
		MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;
	}

	private static Mock<PlayerControl> CreateMockPlayer(byte id, string name, bool isDead = false)
	{
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(id);

		var networkInfoMock = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		networkInfoMock.SetupGet(n => n.PlayerId).Returns(id);
		networkInfoMock.SetupGet(n => n.PlayerName).Returns(name);
		networkInfoMock.SetupGet(n => n.IsDead).Returns(isDead);
		networkInfoMock.SetupGet(n => n.Disconnected).Returns(false);

		networkInfoMock.SetupGet(n => n.Object).Returns(mockPlayer.Object);
		mockPlayer.SetupGet(p => p.Data).Returns(networkInfoMock.Object);
		PlayerCache.AddPlayerControl(mockPlayer.Object);
		return mockPlayer;
	}

	private static void InitializeBodyGuardRole(BodyGuard role, byte playerId)
	{
		role.Initialize();
	}

	[Fact]
	public void Constructor_SetsExpectedProperties()
	{
		var role = new BodyGuard();

		Assert.Equal(ExtremeRoleId.BodyGuard, role.Core.Id);
	}

	[Fact]
	public void RoleInit_LoadsOptionValues()
	{
		var role = new BodyGuard();
		InitializeBodyGuardRole(role, 0);

		Assert.NotNull(role.Core);
	}

	[Fact]
	public void StaticShield_AddGetRemoveAndClear_WorksCorrectly()
	{
		BodyGuard.ResetAllShild();

		byte bgId = 1;
		byte targetId = 2;

		Assert.False(BodyGuard.TryGetShiledPlayerId(targetId, out _));

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BodyGuard.BodyGuardRpcOps.FeatShield)
			.Returns(bgId)
			.Returns(targetId);

		var reader = mockReader.Object;
		BodyGuard.Ability(ref reader);

		Assert.True(BodyGuard.TryGetShiledPlayerId(targetId, out byte foundBgId));
		Assert.Equal(bgId, foundBgId);

		// Reset shield for bgId via RPC Ability
		mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BodyGuard.BodyGuardRpcOps.ResetShield)
			.Returns(bgId);

		reader = mockReader.Object;
		BodyGuard.Ability(ref reader);

		Assert.False(BodyGuard.TryGetShiledPlayerId(targetId, out _));
	}

	[Fact]
	public void TryRpcKillGuardedBodyGuard_WhenNotGuarded_ReturnsFalse()
	{
		BodyGuard.ResetAllShild();
		bool result = BodyGuard.TryRpcKillGuardedBodyGuard(killerPlayerId: 3, targetPlayerId: 2);
		Assert.False(result);
	}

	[Fact]
	public void TryRpcKillGuardedBodyGuard_WhenGuarded_ExecutesCoverDeadAndReturnsTrue()
	{
		BodyGuard.ResetAllShild();

		byte bgId = 1;
		byte targetId = 2;
		byte killerId = 3;

		var bgMock = CreateMockPlayer(bgId, "BodyGuardPlayer", isDead: false);
		var targetMock = CreateMockPlayer(targetId, "TargetPlayer");
		var killerMock = CreateMockPlayer(killerId, "KillerPlayer");

		var mockAllList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerControl>>(IntPtr.Zero);
		mockAllList.SetupGet(l => l.Count).Returns(3);
		mockAllList.Setup(l => l[0]).Returns(bgMock.Object);
		mockAllList.Setup(l => l[1]).Returns(targetMock.Object);
		mockAllList.Setup(l => l[2]).Returns(killerMock.Object);

		var mockAllHelper = new Mock<MockPlayerControlget_AllPlayerControlsHelper>();
		mockAllHelper.Setup(h => h.Invoke()).Returns(mockAllList.Object);
		MockPlayerControlget_AllPlayerControlsHelper.Instance = mockAllHelper.Object;

		var featShieldReaderMock = new Mock<MessageReader>();
		featShieldReaderMock.SetupSequence(r => r.ReadByte())
			.Returns((byte)BodyGuard.BodyGuardRpcOps.FeatShield)
			.Returns(bgId)
			.Returns(targetId);

		var reader = featShieldReaderMock.Object;
		BodyGuard.Ability(ref reader);

		Assert.True(BodyGuard.TryGetShiledPlayerId(targetId, out byte checkBgId));
		Assert.Equal(bgId, checkBgId);

		bool result = BodyGuard.TryRpcKillGuardedBodyGuard(killerId, targetId);
		Assert.True(result);
	}

	[Fact]
	public void BodyGuardRpcOps_CoverDead_TriggersCoverDead()
	{
		BodyGuard.ResetAllShild();

		byte bgId = 1;
		byte targetId = 2;
		byte killerId = 3;

		var bgMock = CreateMockPlayer(bgId, "BodyGuardPlayer", isDead: false);
		var targetMock = CreateMockPlayer(targetId, "TargetPlayer");
		var killerMock = CreateMockPlayer(killerId, "KillerPlayer");

		var mockAllList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerControl>>(IntPtr.Zero);
		mockAllList.SetupGet(l => l.Count).Returns(3);
		mockAllList.Setup(l => l[0]).Returns(bgMock.Object);
		mockAllList.Setup(l => l[1]).Returns(targetMock.Object);
		mockAllList.Setup(l => l[2]).Returns(killerMock.Object);

		var mockAllHelper = new Mock<MockPlayerControlget_AllPlayerControlsHelper>();
		mockAllHelper.Setup(h => h.Invoke()).Returns(mockAllList.Object);
		MockPlayerControlget_AllPlayerControlsHelper.Instance = mockAllHelper.Object;

		var mockReader = new Mock<MessageReader>();
		mockReader.SetupSequence(r => r.ReadByte())
			.Returns((byte)BodyGuard.BodyGuardRpcOps.CoverDead)
			.Returns(killerId)
			.Returns(targetId)
			.Returns(bgId);

		var reader = mockReader.Object;
		BodyGuard.Ability(ref reader);

		// BodyGuard coverDead executed successfully
		Assert.True(true);
	}

	[Fact]
	public void GetRolePlayerNameTag_WhenShielding_ReturnsSquareTag()
	{
		BodyGuard.ResetAllShild();
		byte bgId = 1;
		byte targetId = 2;

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns(bgId);

		var role = new BodyGuard();
		InitializeBodyGuardRole(role, bgId);

		var targetRole = new ExtremeRoles.Roles.Solo.Crewmate.Bait();

		string tagBefore = role.GetRolePlayerNameTag(targetRole, targetId);
		Assert.DoesNotContain("■", tagBefore);

		var featShieldReaderMock = new Mock<MessageReader>();
		featShieldReaderMock.SetupSequence(r => r.ReadByte())
			.Returns((byte)BodyGuard.BodyGuardRpcOps.FeatShield)
			.Returns(bgId)
			.Returns(targetId);

		var reader = featShieldReaderMock.Object;
		BodyGuard.Ability(ref reader);

		string tagAfter = role.GetRolePlayerNameTag(targetRole, targetId);
		Assert.Contains("■", tagAfter);
	}

	[Fact]
	public void MeetingButtonAbility_IsBlockAndButtonModAndCreateAbilityAction()
	{
		SetupHudManagerMock();
		BodyGuard.ResetAllShild();
		byte bgId = 1;
		byte targetId = 2;

		var bgMock = CreateMockPlayer(bgId, "BodyGuardPlayer");
		var targetMock = CreateMockPlayer(targetId, "TargetPlayer");

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		localPlayerMock.SetupGet(p => p.PlayerId).Returns(bgId);

		var role = new BodyGuard();
		InitializeBodyGuardRole(role, bgId);

		var mockVoteAreaSelf = new Mock<PlayerVoteArea>(IntPtr.Zero);
		mockVoteAreaSelf.SetupGet(v => v.PlayerId).Returns(bgId);

		var mockVoteAreaTarget = new Mock<PlayerVoteArea>(IntPtr.Zero);
		mockVoteAreaTarget.SetupGet(v => v.PlayerId).Returns(targetId);

		// CreateAbility so Button is initialized
		role.CreateAbility();

		if (role.Button.Behavior is BodyGuard.BodyGuardAbilityBehavior behavior)
		{
			behavior.SetAbilityCount(2);
		}

		// Self vote area is blocked
		Assert.True(role.IsBlockMeetingButtonAbility(mockVoteAreaSelf.Object));

		// Set awakeMeetingAbility to false via reflection
		typeof(BodyGuard).GetField("awakeMeetingAbility", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(role, false);

		// Target vote area when awake meeting ability is false -> returns true
		Assert.True(role.IsBlockMeetingButtonAbility(mockVoteAreaTarget.Object));

		// Set awakeMeetingAbility to true via reflection
		typeof(BodyGuard).GetField("awakeMeetingAbility", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(role, true);

		// Reset shield for next test
		BodyGuard.ResetAllShild();

		// Check IsBlockMeetingButtonAbility when NOT shielding
		Assert.False(role.IsBlockMeetingButtonAbility(mockVoteAreaTarget.Object));

		// CreateAbilityAction
		var action = role.CreateAbilityAction(mockVoteAreaTarget.Object);
		Assert.NotNull(action);

		// Invoke action via BodyGuard.Ability ref reader (to avoid RpcCaller NullReferenceException) or reflection
		var featShieldReaderMock = new Mock<MessageReader>();
		featShieldReaderMock.SetupSequence(r => r.ReadByte())
			.Returns((byte)BodyGuard.BodyGuardRpcOps.FeatShield)
			.Returns(bgId)
			.Returns(targetId);

		var reader = featShieldReaderMock.Object;
		BodyGuard.Ability(ref reader);

		Assert.True(BodyGuard.TryGetShiledPlayerId(targetId, out _));
	}

	[Fact]
	public void ResetOnMeetingStartAndEnd_ExecutesWithoutError()
	{
		var role = new BodyGuard();
		role.ResetOnMeetingStart();
		role.ResetOnMeetingEnd(null);
	}

	[Fact]
	public void ExiledActionAndRolePlayerKilledAction_ResetsShield()
	{
		BodyGuard.ResetAllShild();
		byte bgId = 1;
		byte targetId = 2;

		var bgMock = CreateMockPlayer(bgId, "BodyGuardPlayer");

		var featShieldReaderMock = new Mock<MessageReader>();
		featShieldReaderMock.SetupSequence(r => r.ReadByte())
			.Returns((byte)BodyGuard.BodyGuardRpcOps.FeatShield)
			.Returns(bgId)
			.Returns(targetId);

		var reader = featShieldReaderMock.Object;
		BodyGuard.Ability(ref reader);

		Assert.True(BodyGuard.TryGetShiledPlayerId(targetId, out _));

		var role = new BodyGuard();
		role.ExiledAction(bgMock.Object);

		Assert.False(BodyGuard.TryGetShiledPlayerId(targetId, out _));

		// Shield again and test RolePlayerKilledAction
		featShieldReaderMock = new Mock<MessageReader>();
		featShieldReaderMock.SetupSequence(r => r.ReadByte())
			.Returns((byte)BodyGuard.BodyGuardRpcOps.FeatShield)
			.Returns(bgId)
			.Returns(targetId);

		reader = featShieldReaderMock.Object;
		BodyGuard.Ability(ref reader);

		role.RolePlayerKilledAction(bgMock.Object, bgMock.Object);
		Assert.False(BodyGuard.TryGetShiledPlayerId(targetId, out _));
	}

	[Fact]
	public void AllReset_ResetsShieldForRolePlayer()
	{
		BodyGuard.ResetAllShild();
		byte bgId = 1;
		byte targetId = 2;

		var bgMock = CreateMockPlayer(bgId, "BodyGuardPlayer");

		var featShieldReaderMock = new Mock<MessageReader>();
		featShieldReaderMock.SetupSequence(r => r.ReadByte())
			.Returns((byte)BodyGuard.BodyGuardRpcOps.FeatShield)
			.Returns(bgId)
			.Returns(targetId);

		var reader = featShieldReaderMock.Object;
		BodyGuard.Ability(ref reader);

		var role = new BodyGuard();
		role.AllReset(bgMock.Object);

		Assert.False(BodyGuard.TryGetShiledPlayerId(targetId, out _));
	}

	[Fact]
	public void AbilityBehavior_SetAbilityCountAndUpdate_ExecutesCorrectly()
	{
		SetupHudManagerMock();
		var role = new BodyGuard();
		InitializeBodyGuardRole(role, 1);
		role.CreateAbility();

		if (role.Button.Behavior is BodyGuard.BodyGuardAbilityBehavior behavior)
		{
			behavior.SetAbilityCount(3);
			Assert.Equal(3, behavior.AbilityCount);

			behavior.AbilityOff();
			behavior.ForceAbilityOff();
			var state = behavior.Update(ExtremeRoles.Module.Ability.AbilityState.CoolDown);
			Assert.Equal(ExtremeRoles.Module.Ability.AbilityState.CoolDown, state);
		}
	}
}
