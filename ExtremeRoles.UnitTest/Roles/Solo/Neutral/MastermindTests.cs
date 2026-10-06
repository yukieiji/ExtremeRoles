using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Impostor;
using ExtremeRoles.Roles.Solo.Liberal;
using ExtremeRoles.Roles.Solo.Neutral;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Neutral;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class MastermindTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public MastermindTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupObjectImplicitHelpers();
		MockSetupHelper.SetupPlayerVoteAreaMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();

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
	public void Initialize_RegistersMastermindRoleProperties()
	{
		var mastermind = new Mastermind();
		InitializeRole(mastermind, 1);

		Assert.Equal(ExtremeRoleId.Mastermind, mastermind.Core.Id);
		Assert.True(mastermind.IsNeutral());
		Assert.False(mastermind.CanKill);
	}

	[Fact]
	public void SetTargetVote_ModifiesVoteResult()
	{
		var mastermind = new Mastermind();
		InitializeRole(mastermind, 1);

		mastermind.SetTargetVote(2);

		var voteTargets = new Dictionary<byte, byte> { { 1, 2 } };
		var voteResults = new Dictionary<byte, int>();

		mastermind.ModifiedVote(1, ref voteTargets, ref voteResults);

		Assert.True(voteResults.ContainsKey(2));
	}

	[Fact]
	public void GetTargetRoleSeeColor_ReturnsTargetColorsWhenEnabled()
	{
		var mastermind = new Mastermind();
		InitializeRole(mastermind, 1);

		var impRole = new SpecialImpostor();
		InitializeRole(impRole, 2);

		var neutralRole = new Jester();
		InitializeRole(neutralRole, 3);

		var liberalRole = new SpecialDove();
		InitializeRole(liberalRole, 4);

		Assert.Equal(Palette.ImpostorRed, mastermind.GetTargetRoleSeeColor(impRole, 2));
		Assert.Equal(Palette.White, mastermind.GetTargetRoleSeeColor(neutralRole, 3));
		Assert.Equal(Palette.White, mastermind.GetTargetRoleSeeColor(liberalRole, 4));
	}

	[Fact]
	public void ModifiedWinPlayer_TriggersWinWhenThreeOrFewerAlive()
	{
		var mastermind = new Mastermind();
		InitializeRole(mastermind, 1);

		var mockMastermindInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockMastermindInfo.SetupGet(p => p.PlayerId).Returns((byte)1);
		mockMastermindInfo.SetupGet(p => p.IsDead).Returns(false);

		var winners = new WinnerContainer();

		mastermind.ModifiedWinPlayer(mockMastermindInfo.Object, GameOverReason.CrewmatesByTask, winners);

		Assert.Equal(RoleGameOverReason.MastermindAlive, (RoleGameOverReason)ExtremeRolesPlugin.ShipState.EndReason);
	}
}
