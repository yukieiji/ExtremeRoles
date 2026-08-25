using System;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;
using Moq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Xunit;

namespace ExtremeRoles.UnitTest.Module.Meeting;

[Collection("UnityMock")]
public class PlayerVoteAreaButtonContainerTests
{
    public PlayerVoteAreaButtonContainerTests()
    {
        MockSetupHelper.SetupCommonMocks();
        MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();
    }

    private (PlayerVoteArea pva, Mock<UiElement> mockCacheBtn, Mock<GameObject> mockGameObject, Mock<PassiveButton> mockPassiveBtn, Mock<SpriteRenderer> mockRenderer) CreateMocks()
    {
        var mockPva = new Mock<PlayerVoteArea>(IntPtr.Zero);
        var mockCancelBtn = new Mock<UiElement>(IntPtr.Zero);
        var mockConfirmBtn = new Mock<UiElement>(IntPtr.Zero);
        var mockParentTransform = new Mock<Transform>(IntPtr.Zero);

        mockConfirmBtn.SetupGet(c => c.transform).Returns(mockParentTransform.Object);
        mockPva.SetupGet(p => p.CancelButton).Returns(mockCancelBtn.Object);
        mockPva.SetupGet(p => p.ConfirmButton).Returns(mockConfirmBtn.Object);

        var mockCacheBtn = new Mock<UiElement>(IntPtr.Zero);
        var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
        mockCacheBtn.SetupGet(c => c.gameObject).Returns(mockGameObject.Object);

        var mockPassiveBtn = new Mock<PassiveButton>(IntPtr.Zero);
        var mockOnClick = new Mock<Button.ButtonClickedEvent>(IntPtr.Zero);
        var mockPersistentCallGroup = new Mock<PersistentCallGroup>(IntPtr.Zero);
        mockOnClick.SetupGet(e => e.m_PersistentCalls).Returns(mockPersistentCallGroup.Object);

        mockPassiveBtn.SetupGet(p => p.OnClick).Returns(mockOnClick.Object);
        mockCacheBtn.Setup(c => c.GetComponent<PassiveButton>()).Returns(mockPassiveBtn.Object);

        var mockRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
        var mockRendererTransform = new Mock<Transform>(IntPtr.Zero);
        mockRenderer.SetupGet(r => r.transform).Returns(mockRendererTransform.Object);

        SpriteRenderer rendererObj = mockRenderer.Object;
        mockCacheBtn.Setup(c => c.TryGetComponent<SpriteRenderer>(out rendererObj)).Returns(true);

        var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
        mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns(mockCacheBtn.Object);
        MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

        var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
        mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns(mockCacheBtn.Object);
        MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

        return (mockPva.Object, mockCacheBtn, mockGameObject, mockPassiveBtn, mockRenderer);
    }

    [Fact]
    public void IsRecreateButtn_WhenNotCached_InstantiatesAndCachesButton()
    {
        var (pva, mockCacheBtn, mockGameObject, mockPassiveBtn, mockRenderer) = CreateMocks();
        var container = new PlayerVoteAreaButtonContainer(pva);

        var mockButtonRole = new Mock<IRoleMeetingButtonAbility>();
        mockButtonRole.Setup(r => r.CreateAbilityAction(It.IsAny<PlayerVoteArea>())).Returns((Action)null!);
        mockButtonRole.SetupGet(r => r.AbilityImage).Returns((Sprite)null!);

        bool created = container.IsRecreateButtn(ExtremeRoleId.Sheriff, mockButtonRole.Object, out var button);

        Assert.True(created);
        Assert.Equal(mockCacheBtn.Object, button);
        mockGameObject.Verify(g => g.SetActive(true), Times.Once());
        mockButtonRole.Verify(r => r.ButtonMod(pva, mockCacheBtn.Object), Times.Once());
    }

    [Fact]
    public void IsRecreateButtn_WhenAlreadyCached_ReturnsCachedButtonWithoutRecreating()
    {
        var (pva, mockCacheBtn, mockGameObject, _, _) = CreateMocks();
        var container = new PlayerVoteAreaButtonContainer(pva);

        var mockButtonRole = new Mock<IRoleMeetingButtonAbility>();
        mockButtonRole.Setup(r => r.CreateAbilityAction(It.IsAny<PlayerVoteArea>())).Returns((Action)null!);

        // First call creates and caches
        container.IsRecreateButtn(ExtremeRoleId.Sheriff, mockButtonRole.Object, out _);

        // Second call uses cache
        bool created = container.IsRecreateButtn(ExtremeRoleId.Sheriff, mockButtonRole.Object, out var secondButton);

        Assert.False(created);
        Assert.Equal(mockCacheBtn.Object, secondButton);
        mockButtonRole.Verify(r => r.ButtonMod(pva, mockCacheBtn.Object), Times.Once()); // Only from 1st call
    }

    [Fact]
    public void HideAllButton_HidesAllCachedButtons()
    {
        var (pva, _, mockGameObject, _, _) = CreateMocks();
        var container = new PlayerVoteAreaButtonContainer(pva);

        var mockButtonRole = new Mock<IRoleMeetingButtonAbility>();
        mockButtonRole.Setup(r => r.CreateAbilityAction(It.IsAny<PlayerVoteArea>())).Returns((Action)null!);

        container.IsRecreateButtn(ExtremeRoleId.Sheriff, mockButtonRole.Object, out _);

        container.HideAllButton();

        mockGameObject.Verify(g => g.SetActive(false), Times.Once());
    }
}
