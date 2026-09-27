using System;
using System.Reflection;
using AmongUs.GameOptions;
using Moq;
using UnityEngine;
using Xunit;

using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ScreamerTests
{
	public ScreamerTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		MockSetupHelper.SetupPaletteHelpers();
		MockSetupHelper.SetupObjectImplicitHelpers();
		var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
		MockSetupHelper.SetupMockConfig(plugin);
		MockSetupHelper.SetupLobbyMock();
		MockSetupHelper.SetupOptionManager();

		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

		ExtremeRoleManager.GameRole.Clear();
	}

	[Fact]
	public void Constructor_InitializesScreamerRoleCorrectly()
	{
		// Arrange & Act
		var screamer = new Screamer();

		// Assert
		Assert.Equal(ExtremeRoleId.Screamer, screamer.Core.Id);
		Assert.Equal(ExtremeRoleType.Crewmate, screamer.Core.Team);
		Assert.Equal(ColorPalette.ScreamerSaffron, screamer.Core.Color);
	}

	[Fact]
	public void RoleSpecificInit_LoadsOptionValues()
	{
		// Arrange
		var screamer = new Screamer();
		screamer.CreateRoleAllOption();

		// Act
		screamer.Initialize();

		// Assert
		var isScreamField = typeof(Screamer).GetField("isScreamWhenKilled", BindingFlags.NonPublic | BindingFlags.Instance);
		var sizeField = typeof(Screamer).GetField("screamImageSize", BindingFlags.NonPublic | BindingFlags.Instance);

		bool isScream = (bool)isScreamField?.GetValue(screamer)!;
		float size = (float)sizeField?.GetValue(screamer)!;

		Assert.True(isScream);
		Assert.Equal(1.0f, size);
	}

	[Fact]
	public void ScreamerAbilityHandler_GetOverrideInfo_ReturnsValidInfo()
	{
		// Act
		var handler = new ScreamerAbilityHandler();
		var info = handler.OverrideInfo;

		// Assert
		Assert.NotNull(info);
		Assert.Null(info.ExiledPlayer);
		Assert.NotNull(info.AnimationText);
	}

	[Fact]
	public void GetRandomScreamIndex_ReturnsValidRange()
	{
		// Act & Assert
		for (int i = 0; i < 50; i++)
		{
			int index = ScreamerAbilityHandler.GetRandomScreamIndex();
			Assert.True(index >= 0 && index <= 4);
		}
	}

	[Fact]
	public void RolePlayerKilledAction_WhenDisabled_DoesNotThrow()
	{
		// Arrange
		var screamer = new Screamer();
		screamer.CreateRoleAllOption();

		var isScreamField = typeof(Screamer).GetField("isScreamWhenKilled", BindingFlags.NonPublic | BindingFlags.Instance);
		isScreamField?.SetValue(screamer, false);

		var mockRolePlayer = new Mock<PlayerControl>(IntPtr.Zero);
		var mockKillerPlayer = new Mock<PlayerControl>(IntPtr.Zero);

		// Act & Assert (Should return early without error)
		screamer.RolePlayerKilledAction(mockRolePlayer.Object, mockKillerPlayer.Object);
	}
}
