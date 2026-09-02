using ExtremeRoles.GameMode.IntroRunner;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode;

public class ClassicIntroRunnerTests
{
    [Fact]
    public void CoRunModeIntro_ReturnsIEnumerator()
    {
        IIntroRunner runner = new ClassicIntroRunner();

        var enumerator = runner.CoRunModeIntro(null!, null!);

        Assert.NotNull(enumerator);
    }
}
