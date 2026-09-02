using System.Collections.Generic;
using AmongUs.GameOptions;
using Moq;
using Xunit;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.UnitTest.GhostRoles;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class VanillaGhostRoleWrapperTests
{
    private readonly Mock<TranslationController> mockTranslation;

    public VanillaGhostRoleWrapperTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        this.mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptyHashSet()
    {
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.GuardianAngel);

        var filter = wrapper.GetRoleFilter();

        Assert.Empty(filter);
    }

    [Fact]
    public void DisabledMethods_ThrowException()
    {
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.GuardianAngel);

        Assert.Throws<System.Exception>(() => wrapper.CreateAbility());
    }
}
