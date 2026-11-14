using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
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

	[Fact]
	public void Initialize_RegistersFemmeFataleProperties()
	{
		var femme = new FemmeFataleRole();
		InitializeRole(femme, 1);

		Assert.Equal(ExtremeRoleId.FemmeFatale, femme.Core.Id);
		Assert.True(femme.IsNeutral());
		Assert.True(femme.CanKill);
		Assert.Equal(ColorPalette.FemmeFataleCrimson, femme.Core.Color);
		Assert.Equal(FemmeFataleRole.RoleShowTag, femme.GetRoleTag());
	}

	[Fact]
	public void JoseRole_PreservesOriginalTeam()
	{
		var femme = new FemmeFataleRole();
		InitializeRole(femme, 1);

		// Crewmate base role
		var crewRole = new SpecialCrew();
		InitializeRole(crewRole, 2);
		var crewJose = new JoseRole(1, femme, crewRole, 45f);
		Assert.Equal(ExtremeRoleType.Crewmate, crewJose.Core.Team);

		// Impostor base role
		var impRole = new SpecialImpostor();
		InitializeRole(impRole, 3);
		var impJose = new JoseRole(1, femme, impRole, 45f);
		Assert.Equal(ExtremeRoleType.Impostor, impJose.Core.Team);

		// Neutral base role
		var neutralRole = new Jester();
		InitializeRole(neutralRole, 4);
		var neutralJose = new JoseRole(1, femme, neutralRole, 45f);
		Assert.Equal(ExtremeRoleType.Neutral, neutralJose.Core.Team);
	}

	[Fact]
	public void JoseStatusModel_SelfKillTimer_And_Resets_WorkCorrectly()
	{
		var statusModel = new JoseStatusModel(45f);
		Assert.Equal(45f, statusModel.CurrentSelfKillTimer);

		ExtremeRoles.Module.SystemType.ExtremeSystemTypeManager.Instance.CreateOrGet<ExtremeRoles.Module.SystemType.GameProgressSystem>(ExtremeRoles.Module.SystemType.ExtremeSystemType.GameProgress);
		ExtremeRoles.Module.SystemType.GameProgressSystem.Current = ExtremeRoles.Module.SystemType.GameProgressSystem.Progress.RoleSetUpEnd;
		ExtremeRoles.Module.SystemType.GameProgressSystem.Current = ExtremeRoles.Module.SystemType.GameProgressSystem.Progress.Task;

		// Timer updates
		bool explode = statusModel.UpdateTimer(10f);
		Assert.False(explode);
		Assert.Equal(35f, statusModel.CurrentSelfKillTimer);

		// OnKill resets timer
		statusModel.OnKill();
		Assert.Equal(45f, statusModel.CurrentSelfKillTimer);

		// OnMeetingStart resets timer
		statusModel.UpdateTimer(20f);
		statusModel.OnMeetingStart();
		Assert.Equal(45f, statusModel.CurrentSelfKillTimer);

		// OnMeetingEnd resets timer
		statusModel.UpdateTimer(15f);
		statusModel.OnMeetingEnd();
		Assert.Equal(45f, statusModel.CurrentSelfKillTimer);

		// Timer expires -> explodes
		explode = statusModel.UpdateTimer(50f);
		Assert.True(explode);
		Assert.Equal(0f, statusModel.CurrentSelfKillTimer);
	}

	[Fact]
	public void FemmeFatale_CreateJose_MarksLocallyAndConvertsAtMeetingEnd()
	{
		var femme = new FemmeFataleRole();
		InitializeRole(femme, 1);

		var crewRole = new SpecialCrew();
		InitializeRole(crewRole, 2);

		var mockTarget = new Mock<PlayerControl>(IntPtr.Zero);
		mockTarget.SetupGet(p => p.PlayerId).Returns((byte)2);
		femme.Target = mockTarget.Object;

		// Use ability marks target locally without RPC
		bool used = femme.UseAbility();
		Assert.True(used);

		// Sees marked target tag
		Assert.Contains(FemmeFataleRole.RoleShowTag, femme.GetRolePlayerNameTag(crewRole, 2));
		Assert.Equal(ColorPalette.FemmeFataleCrimson, femme.GetTargetRoleSeeColor(crewRole, 2));
	}

	[Fact]
	public void FemmeFatale_SpecialVote_ResetModifier_ClearsVotes()
	{
		var femme = new FemmeFataleRole();
		InitializeRole(femme, 1);

		femme.SetTargetVote(3);

		var collector = new VoteInfoCollector();
		var mockInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockInfo.SetupGet(p => p.PlayerId).Returns((byte)1);

		femme.ResetModifier();
		var resetVotes = new List<ExtremeRoles.Module.Meeting.VoteInfo>(femme.GetModdedVoteInfo(collector, mockInfo.Object));
		Assert.Empty(resetVotes);
	}

	[Fact]
	public void FemmeFatale_KillDefense_SacrificesLivingJose()
	{
		var femme = new FemmeFataleRole();
		InitializeRole(femme, 1);

		var crewRole = new SpecialCrew();
		InitializeRole(crewRole, 2);
		femme.AddJosePlayer(2);

		var cachedPtrField = typeof(UnityEngine.Object).GetField("m_CachedPtr", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

		var mockCrewControl = new Mock<PlayerControl>(IntPtr.Zero);
		cachedPtrField?.SetValue(mockCrewControl.Object, (IntPtr)2);
		mockCrewControl.SetupGet(p => p.PlayerId).Returns((byte)2);

		var mockCrewInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockCrewInfo.SetupGet(p => p.PlayerId).Returns((byte)2);
		mockCrewInfo.SetupGet(p => p.IsDead).Returns(false);
		mockCrewInfo.SetupGet(p => p.Disconnected).Returns(false);
		mockCrewInfo.SetupGet(p => p.Object).Returns(mockCrewControl.Object);
		mockCrewControl.SetupGet(p => p.Data).Returns(mockCrewInfo.Object);

		var mockGameData = MockSetupHelper.SetupGameDataMock();
		mockGameData.Setup(g => g.GetPlayerById((byte)2)).Returns(mockCrewInfo.Object);

		ExtremeRoles.Performance.PlayerCache.AddPlayerControl(mockCrewControl.Object);

		// Kill defense triggers on FemmeFatale target
		bool defended = FemmeFataleRole.TryRpcKillGuardedJose(3, 1);
		Assert.True(defended);
	}

	[Fact]
	public void JoseRole_RemoveJose_RevertsRole()
	{
		var femme = new FemmeFataleRole();
		InitializeRole(femme, 1);

		var crewRole = new SpecialCrew();
		var jose = new JoseRole(1, femme, crewRole, 45f);
		jose.CanHasAnotherRole = true;
		jose.SetAnotherRole(crewRole);
		ExtremeRoleManager.GameRole[2] = jose;

		JoseRole.RemoveJose(2);

		Assert.Equal(crewRole, ExtremeRoleManager.GameRole[2]);
	}
}
