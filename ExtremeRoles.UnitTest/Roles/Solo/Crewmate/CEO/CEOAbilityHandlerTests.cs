using ExtremeRoles.Roles.Solo.Crewmate;
using Moq;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.CEOTests;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class CEOAbilityHandlerTests
{
	public CEOAbilityHandlerTests()
	{
		MockSetupHelper.SetupUnityCommonMocks();
		var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
		mockTranslation
			.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
			.Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => !string.IsNullOrEmpty(defaultStr) ? defaultStr : id);
	}

	[Fact]
	public void OverrideInfo_WhenStatusNotAwake_ReturnsNull()
	{
		var status = new CEOStatus { IsAwake = false };
		var handler = new CEOAbilityHandler(status);

		Assert.Null(handler.GetOverrideInfo(null));
	}

	[Fact]
	public void OverrideInfo_WhenStatusIsAwake_ReturnsOverrideInfo()
	{
		var status = new CEOStatus { IsAwake = true };
		var handler = new CEOAbilityHandler(status);

		var info = handler.GetOverrideInfo(null);
		Assert.NotNull(info);
		Assert.Equal("CEOExiledOverride", info.AnimationText);
	}
}
