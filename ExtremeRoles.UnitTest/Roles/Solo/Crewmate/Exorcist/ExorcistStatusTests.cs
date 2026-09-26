using System;
using System.Reflection;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate.Exorcist;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Exorcist;

public class ExorcistStatusTests
{
    public ExorcistStatusTests()
    {
        if (UnityEngine.MockLayerMaskop_ImplicitHelper.Instance == null)
        {
            var mockImplicit = new Mock<UnityEngine.MockLayerMaskop_ImplicitHelper>();
            mockImplicit.Setup(m => m.Invoke(It.IsAny<LayerMask>())).Returns(1);
            UnityEngine.MockLayerMaskop_ImplicitHelper.Instance = mockImplicit.Object;
        }

        if (MockConstantsget_PlayersOnlyMaskHelper.Instance == null)
        {
            var mockMask = new Mock<MockConstantsget_PlayersOnlyMaskHelper>();
            mockMask.Setup(m => m.Invoke()).Returns(new LayerMask());
            MockConstantsget_PlayersOnlyMaskHelper.Instance = mockMask.Object;
        }

        if (UnityEngine.MockPhysics2DOverlapCircleAllHelper.Instance == null)
        {
            var mockOverlap = new Mock<UnityEngine.MockPhysics2DOverlapCircleAllHelper>();
            mockOverlap.Setup(m => m.Invoke(It.IsAny<Vector2>(), It.IsAny<float>(), It.IsAny<int>()))
                .Returns(Array.Empty<Collider2D>());
            UnityEngine.MockPhysics2DOverlapCircleAllHelper.Instance = mockOverlap.Object;
        }
    }

    [Fact]
    public void UpdateToFakeImpostor_ChangesFakeTeamToImpostorAndIsFakeImpostorToTrue()
    {
        // Arrange
        var status = new ExorcistStatus(0.5f, 2.0f);

        Assert.False(status.IsFakeImpostor);
        Assert.Equal(ExtremeRoleType.Crewmate, status.FakeTeam);

        // Act
        status.UpdateToFakeImpostor();

        // Assert
        Assert.True(status.IsFakeImpostor);
        Assert.Equal(ExtremeRoleType.Impostor, status.FakeTeam);

        var gageField = typeof(ExorcistStatus).GetField("awakeFakeImpTaskGage", BindingFlags.NonPublic | BindingFlags.Instance);
        var gageVal = (float)(gageField?.GetValue(status) ?? 0.0f);
        Assert.Equal(-1.0f, gageVal);
    }

    [Fact]
    public void CurTarget_WhenNoDeadBodyInGame_ReturnsNull()
    {
        // Arrange
        MockSetupHelper.SetupPlayerControlMocks();
        var status = new ExorcistStatus(0.5f, 2.0f);

        // Act
        var target = status.CurTarget;

        // Assert
        Assert.Null(target);
    }

    [Fact]
    public void FrameUpdate_WhenAwakeTaskGageIsZeroOrLess_DoesNotSendRpcNorChangeGage()
    {
        // Arrange
        var clientMock = MockSetupHelper.SetupAmongUsClientMock();
        var status = new ExorcistStatus(0.0f, 2.0f);
        var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

        // Act
        status.FrameUpdate(mockPlayer.Object);

        // Assert
        Assert.True(status.IsFakeImpostor);
        clientMock.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void FrameUpdate_WhenTaskGageIsLessThanAwakeGage_DoesNotSendRpcAndIsFakeImpostorRemainsFalse()
    {
        // Arrange
        var clientMock = MockSetupHelper.SetupAmongUsClientMock();
        var status = new ExorcistStatus(0.5f, 2.0f);

        var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

        var task1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
        task1.SetupGet(t => t.Complete).Returns(false);
        var task2 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
        task2.SetupGet(t => t.Complete).Returns(false);

        mockTasksList.SetupGet(l => l.Count).Returns(2);
        mockTasksList.Setup(l => l[0]).Returns(task1.Object);
        mockTasksList.Setup(l => l[1]).Returns(task2.Object);

        mockPlayerInfo.SetupGet(p => p.Tasks).Returns(mockTasksList.Object);

        var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockPlayer.SetupGet(p => p.Data).Returns(mockPlayerInfo.Object);

        // Act
        status.FrameUpdate(mockPlayer.Object);

        // Assert
        Assert.False(status.IsFakeImpostor);
        clientMock.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void FrameUpdate_WhenTaskGageIsGreaterOrEqualToAwakeGage_SendsRpcAndAwakesToFakeImpostor()
    {
        // Arrange
        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.NetId).Returns(55u);

        var clientMock = MockSetupHelper.SetupAmongUsClientMock();
        var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
        clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
            .Returns(mockWriter.Object);

        var status = new ExorcistStatus(0.5f, 2.0f);

        var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

        var task1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
        task1.SetupGet(t => t.Complete).Returns(true);

        mockTasksList.SetupGet(l => l.Count).Returns(1);
        mockTasksList.Setup(l => l[0]).Returns(task1.Object);

        mockPlayerInfo.SetupGet(p => p.Tasks).Returns(mockTasksList.Object);

        var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)7);
        mockPlayer.SetupGet(p => p.Data).Returns(mockPlayerInfo.Object);

        // Act
        status.FrameUpdate(mockPlayer.Object);

        // Assert
        Assert.True(status.IsFakeImpostor);
        Assert.Equal(ExtremeRoleType.Impostor, status.FakeTeam);
        clientMock.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), (byte)RPCOperator.Command.ExorcistOps, It.IsAny<SendOption>(), It.IsAny<int>()), Times.Once);
    }
}
