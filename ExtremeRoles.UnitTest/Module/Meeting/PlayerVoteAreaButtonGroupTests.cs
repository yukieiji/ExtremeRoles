using System;
using System.Linq;
using ExtremeRoles.Module.Meeting;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Meeting;

[Collection("UnityMock")]
public class PlayerVoteAreaButtonGroupTests
{
    public PlayerVoteAreaButtonGroupTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();
    }

    private PlayerVoteArea CreateMockPlayerVoteArea(bool voteComplete)
    {
        var mockPva = new Mock<PlayerVoteArea>(IntPtr.Zero);
        var mockCancelBtn = new Mock<UiElement>(IntPtr.Zero);
        mockCancelBtn.SetupGet(x => x.name).Returns("CancelButton");

        var mockConfirmBtn = new Mock<UiElement>(IntPtr.Zero);
        mockConfirmBtn.SetupGet(x => x.name).Returns("ConfirmButton");

        mockPva.SetupGet(p => p.CancelButton).Returns(mockCancelBtn.Object);
        mockPva.SetupGet(p => p.ConfirmButton).Returns(mockConfirmBtn.Object);
        mockPva.SetupGet(p => p.VoteComplete).Returns(voteComplete);

        return mockPva.Object;
    }

    [Fact]
    public void AddFirstRow_And_AddSecondRow_CalculatesEndOffsetAndBranchCorrectly()
    {
        var pva = CreateMockPlayerVoteArea(voteComplete: true);
        var group = new PlayerVoteAreaButtonGroup(pva);

        // Current count in first: 1 (CancelButton)
        var elem2 = new Mock<UiElement>(IntPtr.Zero);
        elem2.SetupGet(x => x.name).Returns("Elem2");

        var elem3 = new Mock<UiElement>(IntPtr.Zero);
        elem3.SetupGet(x => x.name).Returns("Elem3"); // size=2, endOffset=1.3 - 2*0.65 = 0.0 -> sets -0.01f

        group.AddFirstRow(elem2.Object);
        group.AddFirstRow(elem3.Object);

        var elemSec1 = new Mock<UiElement>(IntPtr.Zero);
        elemSec1.SetupGet(x => x.name).Returns("SecElem1");
        group.AddSecondRow(elemSec1.Object);

        var flattened = group.Flatten(0.0f).ToList();
        Assert.Equal(4, flattened.Count);
        Assert.Equal("CancelButton", flattened[0].Element.name);
        Assert.Equal("Elem2", flattened[1].Element.name);
        Assert.Equal("Elem3", flattened[2].Element.name);
        Assert.Equal("SecElem1", flattened[3].Element.name);
    }

    [Fact]
    public void ResetFirst_WhenCountGreaterThanTwo_CallsRemoveRange()
    {
        var pva = CreateMockPlayerVoteArea(voteComplete: false);
        var group = new PlayerVoteAreaButtonGroup(pva); // Initial size: 2

        var elem3 = new Mock<UiElement>(IntPtr.Zero);
        elem3.SetupGet(x => x.name).Returns("Elem3");

        var elem4 = new Mock<UiElement>(IntPtr.Zero);
        elem4.SetupGet(x => x.name).Returns("Elem4");

        group.AddFirstRow(elem3.Object);
        group.AddFirstRow(elem4.Object); // Total 4 elements

        Assert.Equal(4, group.DefaultFlatten(0.0f).Count());

        Assert.Throws<ArgumentException>(() => group.ResetFirst());
    }

    [Fact]
    public void ResetFirst_WhenCountLessOrEqualToTwo_DoesNothing()
    {
        var pva = CreateMockPlayerVoteArea(voteComplete: false);
        var group = new PlayerVoteAreaButtonGroup(pva); // Initial size: 2

        group.ResetFirst();

        Assert.Equal(2, group.DefaultFlatten(0.0f).Count());
    }

    [Fact]
    public void ResetSecond_ClearsSecondRow()
    {
        var pva = CreateMockPlayerVoteArea(voteComplete: true);
        var group = new PlayerVoteAreaButtonGroup(pva);

        var sec1 = new Mock<UiElement>(IntPtr.Zero);
        sec1.SetupGet(x => x.name).Returns("Sec1");
        group.AddSecondRow(sec1.Object);

        Assert.Equal(2, group.Flatten(0.0f).Count());

        group.ResetSecond();

        Assert.Single(group.Flatten(0.0f));
    }
}
