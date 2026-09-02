using System.Collections;
using ExtremeRoles.GameMode.IntroRunner;
using ExtremeRoles.Helper;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode.IntroRunner;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ClassicIntroRunnerTests
{
    public ClassicIntroRunnerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void ClassicIntroRunner_CanBeInstantiated()
    {
        // Act
        var runner = new ClassicIntroRunner();

        // Assert
        Assert.NotNull(runner);
        Assert.IsAssignableFrom<IIntroRunner>(runner);
    }
}
