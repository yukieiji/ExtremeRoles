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
	public void ScreamerAbilityHandler_GetOverrideInfo_SetsExiledPlayerAndText()
	{
		// Arrange
		var handler = new ScreamerAbilityHandler();
		var mockPlayer = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);

		// Act
		var info = handler.GetOverrideInfo(mockPlayer.Object);

		// Assert
		Assert.NotNull(info);
		Assert.Same(mockPlayer.Object, info.ExiledPlayer);
		Assert.False(string.IsNullOrEmpty(info.AnimationText));
	}

	[Fact]
	public void GetRandomScreamIndex_ReturnsValidRange()
	{
		// Act & Assert
		for (int i = 0; i < 100; i++)
		{
			int index = ScreamerAbilityHandler.GetRandomScreamIndex();
			Assert.True(index >= 0 && index <= 4);
		}
	}

	[Fact]
	public void RoleSpecificInit_LoadsCustomOptions()
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
}
