using System;
using ExtremeRoles.Module.CustomOption;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.CustomOption;

[Collection("UnityMock")]
public class ModOptionMenuTests
{
    public ModOptionMenuTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(ExtremeRolesPlugin.Instance);
        ClientOption.Create();
    }

    [Fact]
    public void ModOptionMenu_Dependencies_ShouldBeMockedWithMoq()
    {
        var mockGo = new Mock<GameObject>();
        var mockTrans = new Mock<Transform>();
        var mockButton = new Mock<ToggleButtonBehaviour>();
        var mockOptionMenu = new Mock<OptionsMenuBehaviour>();

        mockOptionMenu.SetupGet(x => x.gameObject).Returns(mockGo.Object);
        mockOptionMenu.SetupGet(x => x.transform).Returns(mockTrans.Object);
        mockOptionMenu.SetupGet(x => x.CensorChatButton).Returns(mockButton.Object);

        Assert.NotNull(mockOptionMenu.Object);
        Assert.NotNull(mockOptionMenu.Object.CensorChatButton);
        Assert.NotNull(mockOptionMenu.Object.gameObject);
        Assert.NotNull(mockOptionMenu.Object.transform);
    }
}
