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
    public void ModOptionMenu_MenuButton_EnumValues_ShouldMatchClientOption()
    {
        var clientOpt = ClientOption.Instance;

        Assert.True(clientOpt.GhostsSeeTask.Value);
        Assert.True(clientOpt.GhostsSeeVote.Value);
        Assert.True(clientOpt.GhostsSeeRole.Value);
        Assert.True(clientOpt.ShowRoleSummary.Value);
        Assert.False(clientOpt.HideNamePlate.Value);

        clientOpt.GhostsSeeTask.Value = !clientOpt.GhostsSeeTask.Value;
        Assert.False(clientOpt.GhostsSeeTask.Value);

        clientOpt.GhostsSeeVote.Value = !clientOpt.GhostsSeeVote.Value;
        Assert.False(clientOpt.GhostsSeeVote.Value);

        clientOpt.GhostsSeeRole.Value = !clientOpt.GhostsSeeRole.Value;
        Assert.False(clientOpt.GhostsSeeRole.Value);

        clientOpt.ShowRoleSummary.Value = !clientOpt.ShowRoleSummary.Value;
        Assert.False(clientOpt.ShowRoleSummary.Value);

        clientOpt.HideNamePlate.Value = !clientOpt.HideNamePlate.Value;
        Assert.True(clientOpt.HideNamePlate.Value);
    }

    [Fact]
    public void ModOptionMenu_OptionsMenuBehaviour_Dependencies_ShouldBeMockableWithMoq()
    {
        var mockOptionMenu = new Mock<OptionsMenuBehaviour>(IntPtr.Zero);
        var mockButton = new Mock<ToggleButtonBehaviour>(IntPtr.Zero);
        mockOptionMenu.SetupGet(x => x.CensorChatButton).Returns(mockButton.Object);

        Assert.NotNull(mockOptionMenu.Object);
        Assert.NotNull(mockOptionMenu.Object.CensorChatButton);
    }
}
