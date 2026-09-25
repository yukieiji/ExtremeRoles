using System;
using AmongUs.GameOptions;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Liberal;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Liberal;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class EmbezzleTests
{
	private readonly Mock<AmongUsClient> mockClient;
	private readonly Mock<PlayerControl> mockLocalPlayer;

	public EmbezzleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
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

		MockSetupHelper.SetupGameOptionsManagerMock();
		MockSetupHelper.SetupOptionManager();
	}

	private static void SetupLobbyBehaviourMock()
	{
		var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
		var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
		mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
		MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
	}

	private static void InitializeRole(Embezzle role, byte playerId = 1)
	{
		role.CreateRoleAllOption();
		role.Initialize();

		ExtremeRoleManager.GameRole.Clear();
		ExtremeRoleManager.GameRole[playerId] = role;
	}

	[Fact]
	public void Initialize_RegistersOptionsAndProperties()
	{
		// Arrange
		var role = new Embezzle();

		// Act
		InitializeRole(role);

		// Assert
		Assert.True(role.CanSeeTaskBar);
	}

	[Fact]
	public void GetRolePlayerNameTag_AppendsStarWhenTargeted()
	{
		// Arrange
		var role = new Embezzle();
		InitializeRole(role, 1);

		var currentTargetField = typeof(Embezzle).GetField("currentTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		currentTargetField?.SetValue(role, (byte)2);

		// Act - Before targeting
		string nameTagBefore = role.GetRolePlayerNameTag(role, 2);

		// Act - Target
		role.UseAbility();

		// Assert
		Assert.DoesNotContain("★", nameTagBefore);
		Assert.Contains("★", role.GetRolePlayerNameTag(role, 2));
	}

	[Fact]
	public void UseAbility_AddsTargetToTargetedPlayers()
	{
		// Arrange
		var role = new Embezzle();
		InitializeRole(role, 1);

		var currentTargetField = typeof(Embezzle).GetField("currentTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		currentTargetField?.SetValue(role, (byte)2);

		// Act
		bool used = role.UseAbility();

		// Assert
		Assert.True(used);
	}

	[Fact]
	public void ResetOnMeetingStart_ClearsTargetedPlayers()
	{
		// Arrange
		var role = new Embezzle();
		InitializeRole(role, 1);

		var currentTargetField = typeof(Embezzle).GetField("currentTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		currentTargetField?.SetValue(role, (byte)2);

		role.UseAbility();

		// Act
		role.ResetOnMeetingStart();

		// Assert - After meeting start reset, ability can be used again
		currentTargetField?.SetValue(role, (byte)2);
		bool canUseAgain = role.UseAbility();
		Assert.True(canUseAgain);
	}
}
