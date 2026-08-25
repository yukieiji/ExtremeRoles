using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Roles;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.GameEnd;

[Collection("UnityMock")]
public sealed class NeutralSpecialWinCheckerTests
{
    public NeutralSpecialWinCheckerTests()
    {
        MockSetupHelper.SetupCommonMocks();
    }

    [Fact]
    public void TryCheckGameEnd_NoNeutralWinRoles_ReturnsFalse()
    {
        ExtremeRoleManager.GameRole.Clear();
        NeutralSpecialWinChecker checker = new NeutralSpecialWinChecker();

        bool result = checker.TryCheckGameEnd(out GameOverReason reason);

        Assert.False(result);
    }
}
