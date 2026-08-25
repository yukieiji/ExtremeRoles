using System;
using ExtremeRoles.Module.Meeting;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Meeting;

[Collection("UnityMock")]
public class PlayerVoteAreaButtonPostionComputerTests
{
    public PlayerVoteAreaButtonPostionComputerTests()
    {
        MockSetupHelper.SetupCommonMocks();

        var mockActionImplicit = new Mock<Il2CppSystem.MockActionop_ImplicitHelper<float>>();
        mockActionImplicit.Setup(x => x.Invoke(It.IsAny<Action<float>>()))
            .Returns((Action<float> act) => act != null ? new Il2CppSystem.Action<float>(IntPtr.Zero) : null!);
        Il2CppSystem.MockActionop_ImplicitHelper<float>.Instance = mockActionImplicit.Object;
    }

    [Fact]
    public void Properties_SetAndGet_Correctly()
    {
        var mockUiElement = new Mock<UiElement>(IntPtr.Zero);
        mockUiElement.SetupGet(x => x.name).Returns("TestElement");

        float time = 0.5f;
        float endOffset = 0.65f;

        var computer = new PlayerVoteAreaButtonPostionComputer(time, mockUiElement.Object, endOffset);

        Assert.Equal(mockUiElement.Object, computer.Element);

        computer.StartOffset = 0.3f;

        var anchor = new Vector2(); anchor.x = 0f; anchor.y = 1f;
        var offset = new Vector2(); offset.x = 1f; offset.y = 2f;

        computer.Anchor = anchor;
        computer.Offset = offset;

        Assert.Equal(0.3f, computer.StartOffset);
        Assert.Equal(anchor, computer.Anchor);
        Assert.Equal(offset, computer.Offset);

        string expectedStr = "TestElement, Start:0.3, Anchor:(0, 1), Offset:(1, 2)";
        Assert.Equal(expectedStr, computer.ToString());
    }

    [Fact]
    public void Compute_ReturnsIEnumerator()
    {
        var mockUiElement = new Mock<UiElement>(IntPtr.Zero);
        mockUiElement.SetupGet(x => x.name).Returns("TestElement");

        var mockLerpHelper = new Mock<MockEffectsLerpHelper>();
        MockEffectsLerpHelper.Instance = mockLerpHelper.Object;

        var computer = new PlayerVoteAreaButtonPostionComputer(0.5f, mockUiElement.Object, 0.65f);

        var result = computer.Compute();

        mockLerpHelper.Verify(h => h.Invoke(0.5f, It.IsAny<Il2CppSystem.Action<float>>()), Times.Once);
    }
}
