using System;
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
        this.mockTranslation.Setup(x => x.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object>>()))
            .Returns((string id, string defaultStr, Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Il2CppSystem.Object> parts) => defaultStr ?? id);
    }

    [Fact]
    public void GetImportantText_ReturnsFormattedStringForCrewmateAndImpostor()
    {
        var crewWrapper = new VanillaGhostRoleWrapper(RoleTypes.GuardianAngel);
        var impWrapper = new VanillaGhostRoleWrapper(RoleTypes.ImpostorGhost);

        string crewText = crewWrapper.GetImportantText();
        string impText = impWrapper.GetImportantText();

        Assert.NotNull(crewText);
        Assert.NotNull(impText);
    }

    [Fact]
    public void GetRoleFilter_ReturnsEmptyHashSet()
    {
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.GuardianAngel);

        var filter = wrapper.GetRoleFilter();

        Assert.NotNull(filter);
        Assert.Empty(filter);
    }

    [Fact]
    public void DisabledMethods_ThrowException()
    {
        var wrapper = new VanillaGhostRoleWrapper(RoleTypes.GuardianAngel);

        Assert.Throws<Exception>(() => wrapper.CreateAbility());
    }
}
