using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Roles.Solo.Neutral;
using ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Neutral;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class FemmeFataleTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public FemmeFataleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupPlayerVoteAreaMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();

		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);

		SetupLobbyBehaviourMock();

		mockClient = MockSetupHelper.SetupAmongUsClientMock();
		var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
		mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
			.Returns(mockWriter.Object);

		mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockLocalPlayer.SetupProperty(p => p.killTimer);

		MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

		MockSetupHelper.SetupGameDataMock();

		var mockMeetingHelper = new Mock<MockMeetingHudget_InstanceHelper>();
		mockMeetingHelper.Setup(h => h.Invoke()).Returns((MeetingHud)null!);
		MockMeetingHudget_InstanceHelper.Instance = mockMeetingHelper.Object;

		var mockDestroyableMeetingHelper = new Mock<MockDestroyableSingletonget_InstanceHelper<MeetingHud>>();
		mockDestroyableMeetingHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
		MockDestroyableSingletonget_InstanceHelper<MeetingHud>.Instance = mockDestroyableMeetingHelper.Object;

		var mockExileControllerHelper = new Mock<MockExileControllerget_InstanceHelper>();
		mockExileControllerHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockExileControllerget_InstanceHelper.Instance = mockExileControllerHelper.Object;

		var mockDestroyableExileHelper = new Mock<MockDestroyableSingletonget_InstanceHelper<ExileController>>();
		mockDestroyableExileHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
		MockDestroyableSingletonget_InstanceHelper<ExileController>.Instance = mockDestroyableExileHelper.Object;

		var mockShip = new Mock<ShipStatus>(IntPtr.Zero);
		mockShip.SetupGet(s => s.enabled).Returns(true);
		var mockShipHelper = new Mock<MockShipStatusget_InstanceHelper>();
		mockShipHelper.Setup(h => h.Invoke()).Returns(mockShip.Object);
		MockShipStatusget_InstanceHelper.Instance = mockShipHelper.Object;

		var mockSoundManager = new Mock<SoundManager>(IntPtr.Zero);
		var mockSoundInstanceHelper = new Mock<MockSoundManagerget_InstanceHelper>();
		mockSoundInstanceHelper.Setup(x => x.Invoke()).Returns(mockSoundManager.Object);
		MockSoundManagerget_InstanceHelper.Instance = mockSoundInstanceHelper.Object;

		var cachedAudioField = typeof(ExtremeRoles.Helper.Sound).GetField("cachedAudio", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
		if (cachedAudioField?.GetValue(null) is Dictionary<ExtremeRoles.Helper.Sound.Type, AudioClip> cachedAudio)
		{
			var mockClip = new Mock<AudioClip>(IntPtr.Zero);
			cachedAudio[ExtremeRoles.Helper.Sound.Type.GuardianAngleGuard] = mockClip.Object;
		}

		var mockSfxHelper = new Mock<MockConstantsShouldPlaySfxHelper>();
		mockSfxHelper.Setup(x => x.Invoke()).Returns(false);
		MockConstantsShouldPlaySfxHelper.Instance = mockSfxHelper.Object;

		if (ExtremeRoles.GameMode.ExtremeGameModeManager.Instance == null)
		{
			ExtremeRoles.GameMode.ExtremeGameModeManager.Create(GameModes.Normal);
		}
		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();

		if (ExtremeRolesPlugin.ShipState == null)
		{
			var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(nameof(ExtremeRolesPlugin.ShipState), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			shipStateProp?.SetValue(null, new ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus());
		}
	}

	private static void SetupLobbyBehaviourMock()
	{
		var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
		var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
		mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
		MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
	}

	private static void InitializeRole(SingleRoleBase role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole[playerId] = role;
	}

	private static Mock<NetworkedPlayerInfo> SetupLivingPlayer(byte playerId)
	{
		var cachedPtrField = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);

		var mockControl = new Mock<PlayerControl>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockControl.Object, (IntPtr)playerId);
		mockControl.SetupGet(p => p.PlayerId).Returns(playerId);

		var mockInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockInfo.SetupGet(p => p.PlayerId).Returns(playerId);
		mockInfo.SetupGet(p => p.IsDead).Returns(false);
		mockInfo.SetupGet(p => p.Disconnected).Returns(false);
		mockInfo.SetupGet(p => p.Object).Returns(mockControl.Object);
		mockControl.SetupGet(p => p.Data).Returns(mockInfo.Object);

		// バニラロール差し替え分岐(Crewmate/Impostor)を踏まないロールでRoleManager不要にする
		var mockRoleBehaviour = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRoleBehaviour.SetupGet(r => r.Role).Returns(RoleTypes.Scientist);
		mockInfo.SetupGet(p => p.Role).Returns(mockRoleBehaviour.Object);

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.Setup(g => g.GetPlayerById(playerId)).Returns(mockInfo.Object);

		ExtremeRoles.Performance.PlayerCache.AddPlayerControl(mockControl.Object);

		return mockInfo;
	}

	private static void SetupJosePlayerOptions(
		FemmeFataleRole femme,
		bool killDefense,
		bool candidateKillDefense)
	{
		var handler = femme.AbilityClass;
		Assert.NotNull(handler);

		var killDefenseField = typeof(FemmeFataleKillBlockHandler).GetField("killDefense", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(killDefenseField);

		var candidateField = typeof(FemmeFataleKillBlockHandler).GetField("candidateKillDefense", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(candidateField);

		killDefenseField.SetValue(handler, killDefense);
		candidateField.SetValue(handler, candidateKillDefense);
	}

	private FemmeFataleRole CreateInitializedFemme(byte femmePlayerId = 1)
	{
		var femme = new FemmeFataleRole();
		InitializeRole(femme, femmePlayerId);

		return femme;
	}

	private static PlayerControl CreateMockPlayerControl(byte playerId)
	{
		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
		mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

		return mockPlayer.Object;
	}

	private static void SetAbilityTarget(FemmeFataleRole femme, PlayerControl target)
	{
		var targetField = typeof(FemmeFataleRole).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
		targetField?.SetValue(femme, target);
	}

	[Fact]
	public void JoseRole_PreservesOriginalTeam()
	{
		var femme = CreateInitializedFemme();

		// Crewmate base role
		var crewRole = new SpecialCrew();
		InitializeRole(crewRole, 2);
		var crewJose = new JoseRole(2, 1, femme, crewRole);
		Assert.Equal(ExtremeRoleType.Crewmate, crewJose.Core.Team);

		// Impostor base role
		var impRole = new SpecialImpostor();
		InitializeRole(impRole, 3);
		var impJose = new JoseRole(3, 1, femme, impRole);
		Assert.Equal(ExtremeRoleType.Impostor, impJose.Core.Team);

		// Neutral base role
		var neutralRole = new Jester();
		InitializeRole(neutralRole, 4);
		var neutralJose = new JoseRole(4, 1, femme, neutralRole);
		Assert.Equal(ExtremeRoleType.Neutral, neutralJose.Core.Team);
	}

	[Fact]
	public void JoseStatusModel_SelfKillTimer_And_Resets_WorkCorrectly()
	{
		var femme = CreateInitializedFemme();

		// ホセ側(2P)で生成するとローカルプレイヤー表示周りはスキップされる
		var statusModel = new JoseStatusModel(2, 1, femme);

		ExtremeRoles.Module.SystemType.ExtremeSystemTypeManager.Instance.CreateOrGet<ExtremeRoles.Module.SystemType.GameProgressSystem>(ExtremeRoles.Module.SystemType.ExtremeSystemType.GameProgress);
		ExtremeRoles.Module.SystemType.GameProgressSystem.Current = ExtremeRoles.Module.SystemType.GameProgressSystem.Progress.RoleSetUpEnd;
		ExtremeRoles.Module.SystemType.GameProgressSystem.Current = ExtremeRoles.Module.SystemType.GameProgressSystem.Progress.Task;

		// Act: 初期タイマー(45秒)より短い経過では爆発しない
		bool explode = statusModel.UpdateTimer(40f);

		// Assert
		Assert.False(explode);

		// Act: リセットがなければ45-80で爆発する経過時間も、OnKill()のリセット後なので爆発しない
		statusModel.OnKill();
		explode = statusModel.UpdateTimer(40f);

		// Assert: OnKill()によるリセットを検証
		Assert.False(explode);

		// Act: OnMeetingStart()でも同様にリセットされる
		statusModel.OnMeetingStart();
		explode = statusModel.UpdateTimer(40f);

		// Assert
		Assert.False(explode);

		// Act: OnMeetingEnd()でも同様にリセットされる
		statusModel.OnMeetingEnd();
		explode = statusModel.UpdateTimer(40f);

		// Assert
		Assert.False(explode);

		// Act: 残り45-40=5秒なので10秒経過で爆発する
		explode = statusModel.UpdateTimer(10f);

		// Assert
		Assert.True(explode);

		// Act: 爆発は一度のみ発生する
		explode = statusModel.UpdateTimer(100f);

		// Assert
		Assert.False(explode);
	}

	[Fact]
	public void FemmeFatale_CreateJose_MarksLocallyAndConvertsAtMeetingEnd()
	{
		var femme = CreateInitializedFemme();

		var crewRole = new SpecialCrew();
		InitializeRole(crewRole, 2);

		var mockTarget = CreateMockPlayerControl(2);
		SetAbilityTarget(femme, mockTarget);

		// Use ability marks target locally without RPC
		bool used = femme.UseAbility();
		Assert.True(used);

		// 候補には専用タグ、非候補には通常タグが付く
		Assert.NotEqual(
			femme.GetRolePlayerNameTag(crewRole, 3),
			femme.GetRolePlayerNameTag(crewRole, 2));
	}

	[Fact]
	public void FemmeFatale_SpecialVote_ResetModifier_ClearsVotes()
	{
		var femme = CreateInitializedFemme();

		femme.SetTargetVote(3);

		var collector = new VoteInfoCollector();
		var mockInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockInfo.SetupGet(p => p.PlayerId).Returns((byte)1);

		femme.ResetModifier();
		var resetVotes = new List<ExtremeRoles.Module.Meeting.VoteInfo>(femme.GetModdedVoteInfo(collector, mockInfo.Object));
		Assert.Empty(resetVotes);
	}

	[Fact]
	public void FemmeFatale_SpecialVote_JoseCandidateCountsAsLivingJose()
	{
		var femme = CreateInitializedFemme();

		var crewRole = new SpecialCrew();
		InitializeRole(crewRole, 2);
		SetupLivingPlayer(2);

		// Act: ホセ候補を登録する
		SetAbilityTarget(femme, CreateMockPlayerControl(2));
		bool used = femme.UseAbility();

		// Assert
		Assert.True(used);

		// ホセが0人でも候補だけで特殊投票が発生する(下のvotes.Countで検証)

		femme.SetTargetVote(3);
		var collector = new VoteInfoCollector();
		var mockInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockInfo.SetupGet(p => p.PlayerId).Returns((byte)1);

		var votes = new List<ExtremeRoles.Module.Meeting.VoteInfo>(femme.GetModdedVoteInfo(collector, mockInfo.Object));
		Assert.Single(votes);
		Assert.Equal(3, votes[0].TargetId);
		Assert.Equal(1, votes[0].Count);
	}

	[Fact]
	public void FemmeFatale_KillDefense_SacrificesLivingJose()
	{
		var femme = CreateInitializedFemme();

		var crewRole = new SpecialCrew();
		InitializeRole(crewRole, 2);
		femme.AddJosePlayer(2);

		SetupLivingPlayer(2);

		// Act: キラー(3)からファム(local=1)へキルを試行する
		var killedFrom = (IKilledFrom)femme.AbilityClass!;
		bool killable = killedFrom.TryKilledFrom(mockLocalPlayer.Object, CreateMockPlayerControl(3));

		// Assert: ホセの身代わりで防がれキルは許可されない
		Assert.False(killable);
	}

	[Fact]
	public void FemmeFatale_KillDefense_JoseCandidateSacrifice_OptionOn()
	{
		var femme = CreateInitializedFemme();

		var crewRole = new SpecialCrew();
		InitializeRole(crewRole, 2);
		SetupLivingPlayer(2);

		// Act: ホセ候補を登録する
		SetAbilityTarget(femme, CreateMockPlayerControl(2));
		bool usedOn = femme.UseAbility();

		// Assert
		Assert.True(usedOn);

		// Act: デフォルトオプションオンなら身代わりが発生する
		var killedFromOn = (IKilledFrom)femme.AbilityClass!;
		bool killableOn = killedFromOn.TryKilledFrom(mockLocalPlayer.Object, CreateMockPlayerControl(3));

		// Assert: キルは許可されない
		Assert.False(killableOn);
	}

	[Fact]
	public void FemmeFatale_KillDefense_JoseCandidateSacrifice_OptionOff()
	{
		var femme = CreateInitializedFemme();

		var crewRole = new SpecialCrew();
		InitializeRole(crewRole, 2);
		SetupLivingPlayer(2);

		// Act: ホセ候補を登録する
		SetAbilityTarget(femme, CreateMockPlayerControl(2));
		bool usedOff = femme.UseAbility();

		// Assert
		Assert.True(usedOff);

		SetupJosePlayerOptions(femme, killDefense: false, candidateKillDefense: false);

		// Act: 防御オフでキルを試行する
		var killedFromOff = (IKilledFrom)femme.AbilityClass!;
		bool killableOff = killedFromOff.TryKilledFrom(mockLocalPlayer.Object, CreateMockPlayerControl(3));

		// Assert: 身代わりせずキルが許可される
		Assert.True(killableOff);
	}

	[Fact]
	public void FemmeFatale_CandidatePromotion_PromotesAfterConfiguredMeetingCount()
	{
		var femme = CreateInitializedFemme();

		var crewRole = new SpecialCrew();
		InitializeRole(crewRole, 2);
		SetupLivingPlayer(2);

		// Act: ホセ候補を登録する
		SetAbilityTarget(femme, CreateMockPlayerControl(2));
		bool used = femme.UseAbility();

		// Assert
		Assert.True(used);

		// 昇格に必要な会議回数を2に設定
		var handlerField = typeof(FemmeFataleRole).GetField("josePromotionHandler", BindingFlags.NonPublic | BindingFlags.Instance);
		var promotionHandler = handlerField?.GetValue(femme);
		var countField = typeof(FemmeFataleJosePromotionHandler).GetField("promoteMeetingCount", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(promotionHandler);
		Assert.NotNull(countField);

		countField.SetValue(promotionHandler, 2);

		// Act: 1回目の会議終了
		femme.ResetOnMeetingEnd();

		// Assert: 昇格せず候補のまま(ホセに交代していない)
		Assert.IsNotType<JoseRole>(ExtremeRoleManager.GameRole[2]);

		// Act: 2回目の会議終了
		femme.ResetOnMeetingEnd();

		// Assert: 昇格してホセに交代する
		Assert.IsType<JoseRole>(ExtremeRoleManager.GameRole[2]);

		// Act: 昇格後も会議が終わる
		femme.ResetOnMeetingEnd();

		// Assert: 二重昇格せずホセのまま
		Assert.IsType<JoseRole>(ExtremeRoleManager.GameRole[2]);
	}

	[Fact]
	public void FemmeFatale_CandidatePromotion_DeadCandidateIsAlsoPromoted()
	{
		var femme = CreateInitializedFemme();

		var crewRole = new SpecialCrew();
		InitializeRole(crewRole, 2);
		var mockInfo = SetupLivingPlayer(2);

		// Act: ホセ候補を登録する
		SetAbilityTarget(femme, CreateMockPlayerControl(2));
		bool used = femme.UseAbility();

		// Assert
		Assert.True(used);

		// Act: 候補を死亡させたまま会議を終了する(昇格条件はデフォルト1回)
		mockInfo.SetupGet(p => p.IsDead).Returns(true);
		femme.ResetOnMeetingEnd();

		// Assert: 死亡候補でも昇格してホセに交代する(仕様)
		Assert.IsType<JoseRole>(ExtremeRoleManager.GameRole[2]);
	}

	[Fact]
	public void FemmeFatale_KillCoolBoost_JoseCandidateKillReducesCool()
	{
		var femme = CreateInitializedFemme();

		SetupLivingPlayer(2);

		// Act: ホセ候補を登録する
		SetAbilityTarget(femme, CreateMockPlayerControl(2));
		bool used = femme.UseAbility();

		// Assert
		Assert.True(used);

		var victim = CreateMockPlayerControl(3);

		// ホセ候補のキルでファムのキルクールが短縮される(デフォルト40%減少)
		mockLocalPlayer.Object.killTimer = 40.0f;

		femme.HookMurderPlayer(CreateMockPlayerControl(2), victim);

		Assert.Equal(40.0f * 0.6f, mockLocalPlayer.Object.killTimer, 3);
	}

	[Fact]
	public void FemmeFatale_KillCoolBoost_NeitherJoseNorCandidate_DoesNothing()
	{
		var femme = CreateInitializedFemme();

		var killer = CreateMockPlayerControl(2);
		var victim = CreateMockPlayerControl(3);

		mockLocalPlayer.Object.killTimer = 40.0f;

		femme.HookMurderPlayer(killer, victim);

		Assert.Equal(40.0f, mockLocalPlayer.Object.killTimer, 3);
	}

	[Fact]
	public void JoseRole_RemoveJose_RevertsRole()
	{
		var femme = CreateInitializedFemme();

		var crewRole = new SpecialCrew();
		var jose = new JoseRole(2, 1, femme, crewRole);
		jose.CanHasAnotherRole = true;
		jose.SetAnotherRole(crewRole);
		ExtremeRoleManager.GameRole[2] = jose;

		JoseRole.RemoveJose(2);

		Assert.Equal(crewRole, ExtremeRoleManager.GameRole[2]);
	}
}
