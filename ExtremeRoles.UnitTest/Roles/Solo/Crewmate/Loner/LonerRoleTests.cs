using System;
using System.Reflection;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate.Loner;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Loner;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class LonerRoleTests
{
	public LonerRoleTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupExtremeSystemTypeManagerMock();
		MockSetupHelper.SetupAmongUsClientMock();
		MockSetupHelper.SetupLobbyMock();
		MockSetupHelper.SetupGameDataMock();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupOptionManager();

		var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
		mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
	}

	[Fact]
	public void RoleSpecificInit_InitializesStatusAndAbilityClass()
	{
		var role = new LonerRole();
		role.CreateRoleAllOption();

		var initMethod = typeof(LonerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
		initMethod?.Invoke(role, null);

		Assert.NotNull(role.Status);
		Assert.IsType<LonerStatusModel>(role.Status);
		Assert.NotNull(role.AbilityClass);
		Assert.IsType<LonerAbilityHandler>(role.AbilityClass);
	}

	[Fact]
	public void CreateSpecificOption_CreatesExpectedOptions()
	{
		int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Loner);
		using AutoParentSetOptionCategoryFactory factory = OptionCategoryAssembler.CreateAutoParentSetOptionCategory(
			groupId,
			"LonerTestCategory",
			OptionTab.CrewmateTab,
			Color.white);

		var role = new LonerRole();

		var createOptionMethod = typeof(LonerRole).GetMethod("CreateSpecificOption", BindingFlags.NonPublic | BindingFlags.Instance);
		createOptionMethod?.Invoke(role, new object[] { factory });

		Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
		Assert.NotNull(category);
	}

	[Fact]
	public void Update_WhenAbilityNotNull_ExecutesWithoutError()
	{
		var role = new LonerRole();
		role.CreateRoleAllOption();

		var initMethod = typeof(LonerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
		initMethod?.Invoke(role, null);

		var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		role.Update(mockPlayer.Object);
	}

	[Fact]
	public void ResetOnMeetingStart_ExecutesWithoutError()
	{
		var role = new LonerRole();
		role.CreateRoleAllOption();

		var initMethod = typeof(LonerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
		initMethod?.Invoke(role, null);

		role.ResetOnMeetingStart();
	}

	[Fact]
	public void ResetOnMeetingEnd_ExecutesWithoutError()
	{
		var role = new LonerRole();

		role.ResetOnMeetingEnd(null);
	}

	[Fact]
	public void GetRolePlayerNameTag_ForLocalPlayer_ReturnsFormattedTag()
	{
		var role = new LonerRole();
		role.CreateRoleAllOption();

		var initMethod = typeof(LonerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
		initMethod?.Invoke(role, null);

		string tag = role.GetRolePlayerNameTag(role, 1);
		Assert.Equal("(0/10)", tag);
	}

	[Fact]
	public void GetRolePlayerNameTag_ForOtherPlayer_ReturnsDefaultTag()
	{
		var role = new LonerRole();
		role.CreateRoleAllOption();

		var initMethod = typeof(LonerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
		initMethod?.Invoke(role, null);

		string tag = role.GetRolePlayerNameTag(role, 2);
		Assert.NotEqual("(0/10)", tag);
	}
}