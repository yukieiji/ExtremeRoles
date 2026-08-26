#nullable enable

using System;
using System.Collections.Generic;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType;
using Moq;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.RoleAssign;

[Collection("UnityMock")]
public class ExtremeRoleAssigneeTests
{
    public ExtremeRoleAssigneeTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupLogger();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        var mockLocalPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)0);
        mockLocalPlayer.SetupGet(p => p.NetId).Returns((uint)10);

        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(h => h.Invoke()).Returns(mockLocalPlayer.Object);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;
    }

    [Fact]
    public void CoRpcAssign_ExecutesBuilderBuild_AndYieldsNull()
    {
        var mockBuilder = new Mock<IRoleAssignDataBuilder>();
        mockBuilder.Setup(b => b.Build()).Returns(new List<IPlayerToExRoleAssignData>
        {
            new PlayerToSingleRoleAssignData(0, 100, 0)
        });

        var assignee = new ExtremeRoleAssignee(mockBuilder.Object);
        var enumerator = assignee.CoRpcAssign();

        Assert.NotNull(enumerator);
        mockBuilder.Verify(b => b.Build(), Times.Never);

        // First MoveNext invokes Build() and yields null
        bool hasNext = enumerator.MoveNext();
        Assert.True(hasNext);
        Assert.Null(enumerator.Current);
        mockBuilder.Verify(b => b.Build(), Times.Once);
    }
}
