using ExtremeRoles.GameMode.IntroRunner;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode;

public class HideNSeekIntroRunnerTests
{
    [Fact]
    public void CoRunModeIntro_ReturnsIEnumerator()
    {
        IIntroRunner runner = new HideNSeekIntroRunner();

        var enumerator = runner.CoRunModeIntro(null!, null!);

        Assert.NotNull(enumerator);
    }
}
