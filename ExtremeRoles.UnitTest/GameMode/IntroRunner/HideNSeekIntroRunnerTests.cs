using System.Collections;
using ExtremeRoles.GameMode.IntroRunner;
using ExtremeRoles.Helper;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode.IntroRunner;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class HideNSeekIntroRunnerTests
{
    public HideNSeekIntroRunnerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
    }

    [Fact]
    public void HideNSeekIntroRunner_CanBeInstantiated()
    {
        // Act
        var runner = new HideNSeekIntroRunner();

        // Assert
        Assert.NotNull(runner);
        Assert.IsAssignableFrom<IIntroRunner>(runner);
    }
}
