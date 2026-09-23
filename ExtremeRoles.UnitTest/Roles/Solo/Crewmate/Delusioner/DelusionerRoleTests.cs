using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate.Delusioner;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Delusioner;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class DelusionerRoleTests
{
	private readonly Mock<AmongUsClient> mockClient;

	public DelusionerRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupGameDataMock();

		var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
		var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		var mockRole = new Mock<RoleBehaviour>(IntPtr.Zero);
		mockRole.SetupGet(r => r.Blurb).Returns("Crewmate Blurb");
		mockData.SetupGet(d => d.Role).Returns(mockRole.Object);
		localPlayerMock.SetupGet(p => p.Data).Returns(mockData.Object);

		mockClient = MockSetupHelper.SetupAmongUsClientMock();

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>()))
			.Returns((string id, string defaultStr, Il2CppReferenceArray<Il2CppSystem.Object> parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
	}

	[Fact]
	public void Constructor_InitializesCorrectly()
	{
		var role = new DelusionerRole();
		Assert.NotNull(role);
		Assert.Equal(ExtremeRoleId.Delusioner, role.Core.Id);
		Assert.Equal((int)ExtremeRoles.Roles.API.Interface.IRoleVoteModifier.ModOrder.DelusionerCheckVote, role.Order);
		Assert.Equal(RoleTypes.Crewmate, role.NoneAwakeRole);
		Assert.Equal("", role.GetFakeOptionString());
	}

	[Fact]
	public void IsAwake_WhenInLobby_ReturnsTrue()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.NotJoined);
		var role = new DelusionerRole();
		Assert.True(role.IsAwake);
	}

	[Fact]
	public void IsAwake_WhenInGameAndNotAwake_ReturnsFalse()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
		var role = new DelusionerRole();
		Assert.False(role.IsAwake);
	}

	[Fact]
	public void DescriptionsAndNames_WhenAwakeOrTruthColor()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.NotJoined); // Makes IsAwake = true
		var role = new DelusionerRole();

		Assert.NotNull(role.GetColoredRoleName(true));
		Assert.NotNull(role.GetColoredRoleName(false));
		Assert.NotNull(role.GetFullDescription());
		Assert.NotNull(role.GetImportantText(true));
		Assert.NotNull(role.GetIntroDescription());
		Assert.Equal(role.Core.Color, role.GetNameColor(true));
		Assert.Equal(role.Core.Color, role.GetNameColor(false));
	}

	[Fact]
	public void DescriptionsAndNames_WhenNotAwakeAndNotTruthColor()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started); // IsAwake = false
		var role = new DelusionerRole();

		Assert.NotNull(role.GetColoredRoleName(false));
		Assert.NotNull(role.GetFullDescription());
		Assert.NotNull(role.GetImportantText(true));
		Assert.NotNull(role.GetIntroDescription());
		Assert.Equal(Palette.White, role.GetNameColor(false));
	}

	[Fact]
	public void ButtonProperty_GetterAndSetter()
	{
		var role = new DelusionerRole();
		Assert.Null(role.Button);

		// DelusionerRole.Button setter does nothing when ability is null
		role.Button = null;
		Assert.Null(role.Button);
	}

	[Fact]
	public void IsAbilityUse_WhenNotAwake_ReturnsFalse()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
		var role = new DelusionerRole();
		Assert.False(role.IsAbilityUse());
	}

	[Fact]
	public void ResetOnMeetingStart_And_ResetOnMeetingEnd_And_ResetModifier()
	{
		var role = new DelusionerRole();
		role.ResetOnMeetingStart();
		role.ResetOnMeetingEnd(null);
		role.ResetModifier();
	}

	[Fact]
	public void HookVoteEnd_AwakeConditionAndCoolTimeReduce()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
		var role = new DelusionerRole();

		var mockHud = new Mock<MeetingHud>(IntPtr.Zero);
		var mockNetInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockNetInfo.SetupGet(p => p.PlayerId).Returns((byte)1);

		var voteDict = new Dictionary<byte, int> { { (byte)1, 3 } };

		// Default awakeVoteCount = 3, so 3 votes will awaken
		role.HookVoteEnd(mockHud.Object, mockNetInfo.Object, voteDict);

		Assert.True(role.IsAwake);
	}

	[Fact]
	public void HookVoteEnd_WhenPlayerNotInVoteDict_DoesNotAwake()
	{
		mockClient.SetupGet(c => c.GameState).Returns(InnerNetClient.GameStates.Started);
		var role = new DelusionerRole();

		var mockHud = new Mock<MeetingHud>(IntPtr.Zero);
		var mockNetInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
		mockNetInfo.SetupGet(p => p.PlayerId).Returns((byte)1);

		var voteDict = new Dictionary<byte, int> { { (byte)2, 5 } };

		role.HookVoteEnd(mockHud.Object, mockNetInfo.Object, voteDict);

		Assert.False(role.IsAwake);
	}

	[Fact]
	public void UseAbility_WhenAbilityIsNull_ReturnsFalse()
	{
		var role = new DelusionerRole();
		Assert.False(role.UseAbility());
	}
}
