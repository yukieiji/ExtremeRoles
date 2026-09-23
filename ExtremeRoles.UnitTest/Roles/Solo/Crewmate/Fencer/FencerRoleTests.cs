using System;
using System.Reflection;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate.Fencer;
using ExtremeRoles.UnitTest;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Fencer;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class FencerRoleTests
{
    public FencerRoleTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        var mockClient = MockSetupHelper.SetupAmongUsClientMock();
        var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
        mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
                  .Returns(mockWriter.Object);

        SetupLobbyBehaviourMock();
        SetupTranslationControllerMock();
        SetupMapBehaviourMock(null);
        SetupMinigameMock(null);

        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
    }

    private static void SetupLobbyBehaviourMock()
    {
        var mockLobby = new Mock<LobbyBehaviour>(IntPtr.Zero);
        var mockLobbyHelper = new Mock<MockLobbyBehaviourget_InstanceHelper>();
        mockLobbyHelper.Setup(x => x.Invoke()).Returns(mockLobby.Object);
        MockLobbyBehaviourget_InstanceHelper.Instance = mockLobbyHelper.Object;
    }

    private static void SetupTranslationControllerMock()
    {
        var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>())).Returns("TestString");
        mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
        mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>())).Returns("TestString");
        mockTranslation.Setup(t => t.GetString(It.IsAny<StringNames>(), It.IsAny<Il2CppReferenceArray<Il2CppSystem.Object>>())).Returns("TestString");
    }

    private static void SetupMapBehaviourMock(MapBehaviour? map)
    {
        var mockMapHelper = new Mock<MockMapBehaviourget_InstanceHelper>();
        mockMapHelper.Setup(x => x.Invoke()).Returns(map!);
        MockMapBehaviourget_InstanceHelper.Instance = mockMapHelper.Object;
    }

    private static void SetupMinigameMock(Minigame? minigame)
    {
        var mockMinigameHelper = new Mock<MockMinigameget_InstanceHelper>();
        mockMinigameHelper.Setup(x => x.Invoke()).Returns(minigame!);
        MockMinigameget_InstanceHelper.Instance = mockMinigameHelper.Object;
    }

    [Fact]
    public void RoleSpecificInit_InitializesStatusAndAbilityClass()
    {
        // Arrange
        var fencer = new FencerRole();
        fencer.CreateRoleAllOption();

        // Act
        var initMethod = typeof(FencerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
        initMethod?.Invoke(fencer, null);

        // Assert
        Assert.NotNull(fencer.Status);
        Assert.IsType<FencerStatusModel>(fencer.Status);
        Assert.Equal(5.0f, ((FencerStatusModel)fencer.Status).MaxTime);
        Assert.Equal(0.0f, ((FencerStatusModel)fencer.Status).Timer);
        Assert.NotNull(fencer.AbilityClass);
        Assert.IsType<FencerAbilityHandler>(fencer.AbilityClass);
    }

    [Fact]
    public void CanKill_SetAndGet_UpdatesStatusWhenNotNull()
    {
        // Arrange
        var fencer = new FencerRole();
        fencer.CreateRoleAllOption();

        var initMethod = typeof(FencerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
        initMethod?.Invoke(fencer, null);

        // Act
        fencer.CanKill = true;

        // Assert
        Assert.True(fencer.CanKill);
        Assert.True(((FencerStatusModel)fencer.Status!).CanKill);

        // Act
        fencer.CanKill = false;

        // Assert
        Assert.False(fencer.CanKill);
        Assert.False(((FencerStatusModel)fencer.Status!).CanKill);
    }

    [Fact]
    public void ResetOnMeetingStart_CleansUpAndDisablesCanKill()
    {
        // Arrange
        var fencer = new FencerRole();
        fencer.CreateRoleAllOption();

        var initMethod = typeof(FencerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
        initMethod?.Invoke(fencer, null);

        var status = (FencerStatusModel)fencer.Status!;
        status.IsCounter = true;
        fencer.CanKill = true;

        // Act
        fencer.ResetOnMeetingStart();

        // Assert
        Assert.False(status.IsCounter);
        Assert.False(fencer.CanKill);
    }

    [Fact]
    public void ResetOnMeetingEnd_ExecutesWithoutError()
    {
        // Arrange
        var fencer = new FencerRole();

        // Act & Assert (does nothing)
        fencer.ResetOnMeetingEnd(null);
    }

    [Fact]
    public void Update_WhenStatusIsNull_DoesNothing()
    {
        // Arrange
        var fencer = new FencerRole();
        var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

        // Act
        fencer.Update(mockPlayer.Object);

        // Assert
        Assert.Null(fencer.Status);
    }

    [Fact]
    public void Update_WhenTimerZeroOrNegative_SetsCanKillFalse()
    {
        // Arrange
        var fencer = new FencerRole();
        fencer.CreateRoleAllOption();

        var initMethod = typeof(FencerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
        initMethod?.Invoke(fencer, null);

        fencer.CanKill = true;
        ((FencerStatusModel)fencer.Status!).Timer = 0.0f;

        var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

        // Act
        fencer.Update(mockPlayer.Object);

        // Assert
        Assert.False(fencer.CanKill);
    }

    [Fact]
    public void Update_WhenTimerPositive_DecreasesTimerByDeltaTime()
    {
        // Arrange
        var fencer = new FencerRole();
        fencer.CreateRoleAllOption();

        var initMethod = typeof(FencerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
        initMethod?.Invoke(fencer, null);

        fencer.CanKill = true;
        var status = (FencerStatusModel)fencer.Status!;
        status.Timer = 5.0f;

        var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

        // Act
        fencer.Update(mockPlayer.Object);

        // Assert
        Assert.True(status.Timer < 5.0f);
    }

    [Fact]
    public void UseAbility_SetsCounterOnAndReturnsTrue()
    {
        // Arrange
        var fencer = new FencerRole();
        fencer.CreateRoleAllOption();

        var initMethod = typeof(FencerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
        initMethod?.Invoke(fencer, null);

        var status = (FencerStatusModel)fencer.Status!;

        // Act
        var result = fencer.UseAbility();

        // Assert
        Assert.True(result);
        Assert.True(status.IsCounter);
    }

    [Fact]
    public void IsAbilityUse_CallsCommonUse()
    {
        // Arrange
        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockData.SetupGet(d => d.IsDead).Returns(false);
        mockData.SetupGet(d => d.Disconnected).Returns(false);
        mockData.SetupGet(d => d.Object).Returns(mockLocalPlayer.Object);
        mockLocalPlayer.SetupGet(p => p.Data).Returns(mockData.Object);
        mockLocalPlayer.SetupGet(p => p.CanMove).Returns(true);

        var fencer = new FencerRole();

        // Act
        var result = fencer.IsAbilityUse();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Ability_CounterOn_SetsIsCounterTrue()
    {
        // Arrange
        byte playerId = 5;

        var fencer = new FencerRole();
        fencer.CreateRoleAllOption();

        var initMethod = typeof(FencerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
        initMethod?.Invoke(fencer, null);

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.GameRole[playerId] = fencer;

        var mockReader = new Mock<MessageReader>();
        mockReader.SetupSequence(r => r.ReadByte())
                  .Returns(playerId)
                  .Returns((byte)FencerRole.FencerAbility.CounterOn);

        var reader = mockReader.Object;

        // Act
        FencerRole.Ability(ref reader);

        // Assert
        Assert.True(((FencerStatusModel)fencer.Status!).IsCounter);
    }

    [Fact]
    public void Ability_CounterOff_SetsIsCounterFalse()
    {
        // Arrange
        byte playerId = 5;

        var fencer = new FencerRole();
        fencer.CreateRoleAllOption();

        var initMethod = typeof(FencerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
        initMethod?.Invoke(fencer, null);

        var status = (FencerStatusModel)fencer.Status!;
        status.IsCounter = true;

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.GameRole[playerId] = fencer;

        var mockReader = new Mock<MessageReader>();
        mockReader.SetupSequence(r => r.ReadByte())
                  .Returns(playerId)
                  .Returns((byte)FencerRole.FencerAbility.CounterOff);

        var reader = mockReader.Object;

        // Act
        FencerRole.Ability(ref reader);

        // Assert
        Assert.False(status.IsCounter);
    }

    [Fact]
    public void Ability_ActivateKillButton_CallsEnableKillButton()
    {
        // Arrange
        byte playerId = 5;

        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

        var fencer = new FencerRole();
        fencer.CreateRoleAllOption();

        var initMethod = typeof(FencerRole).GetMethod("RoleSpecificInit", BindingFlags.NonPublic | BindingFlags.Instance);
        initMethod?.Invoke(fencer, null);

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.GameRole[playerId] = fencer;

        var mockReader = new Mock<MessageReader>();
        mockReader.SetupSequence(r => r.ReadByte())
                  .Returns(playerId)
                  .Returns((byte)FencerRole.FencerAbility.ActivateKillButton);

        var reader = mockReader.Object;

        // Act
        FencerRole.Ability(ref reader);

        // Assert
        Assert.True(fencer.CanKill);
    }

    [Fact]
    public void Ability_InvalidPlayerOrType_DoesNotThrow()
    {
        // Arrange
        ExtremeRoleManager.GameRole.Clear();

        var mockReader = new Mock<MessageReader>();
        mockReader.SetupSequence(r => r.ReadByte())
                  .Returns((byte)99)
                  .Returns((byte)FencerRole.FencerAbility.CounterOn);

        var reader = mockReader.Object;

        // Act & Assert
        FencerRole.Ability(ref reader);
    }

    [Fact]
    public void CreateSpecificOption_CreatesExpectedOptions()
    {
        // Arrange
        int groupId = ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Fencer);
        using AutoParentSetOptionCategoryFactory factory = OptionCategoryAssembler.CreateAutoParentSetOptionCategory(
            groupId,
            "FencerTestCategory",
            OptionTab.CrewmateTab,
            Color.white);

        var fencer = new FencerRole();

        // Act
        var createOptionMethod = typeof(FencerRole).GetMethod("CreateSpecificOption", BindingFlags.NonPublic | BindingFlags.Instance);
        createOptionMethod?.Invoke(fencer, new object[] { factory });

        // Assert
        Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.CrewmateTab, groupId, out var category));
        Assert.NotNull(category);
    }

    [Fact]
    public void CreateAbility_ConfiguresButton()
    {
        // Arrange
        SetupHudManagerMock();

        var mockSprite = new Mock<Sprite>(IntPtr.Zero);
        LruCache<string, Sprite>.Add($"{ObjectPath.FencerCounter}115", mockSprite.Object);

        var fencer = new FencerRole();
        fencer.CreateRoleAllOption();

        // Act
        fencer.CreateAbility();

        // Assert
        Assert.NotNull(fencer.Button);
    }

    private static void SetupHudManagerMock()
    {
        if (MockVector3get_oneHelper.Instance == null)
        {
            var mockOne = new Mock<MockVector3get_oneHelper>();
            mockOne.Setup(x => x.Invoke()).Returns(new Vector3(1f, 1f, 1f));
            MockVector3get_oneHelper.Instance = mockOne.Object;
        }

        var mockObjectImplicitInt = new Mock<Il2CppSystem.MockObjectop_ImplicitHelper6>();
        mockObjectImplicitInt.Setup(x => x.Invoke(It.IsAny<int>())).Returns(new Mock<Il2CppSystem.Object>(IntPtr.Zero).Object);
        Il2CppSystem.MockObjectop_ImplicitHelper6.Instance = mockObjectImplicitInt.Object;

        var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
        mockGameObject.Setup(g => g.SetActive(It.IsAny<bool>()));

        var mockGridArrange = new Mock<GridArrange>(IntPtr.Zero);
        mockGridArrange.Setup(g => g.ArrangeChilds());

        var mockParentGameObject = new Mock<GameObject>(IntPtr.Zero);
        mockParentGameObject.Setup(g => g.GetComponent<GridArrange>()).Returns(mockGridArrange.Object);

        var mockParentTransform = new Mock<Transform>(IntPtr.Zero);
        mockParentTransform.SetupGet(t => t.gameObject).Returns(mockParentGameObject.Object);

        var mockTransform = new Mock<Transform>(IntPtr.Zero);
        mockTransform.SetupGet(t => t.parent).Returns(mockParentTransform.Object);
        mockTransform.Setup(t => t.FindChild(It.IsAny<string>())).Returns((Transform)null!);

        var mockMaterial = new Mock<Material>(IntPtr.Zero);
        mockMaterial.Setup(m => m.SetFloat(It.IsAny<string>(), It.IsAny<float>()));

        var mockSpriteRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);
        mockSpriteRenderer.SetupProperty(s => s.sprite);
        mockSpriteRenderer.SetupProperty(s => s.color);
        mockSpriteRenderer.SetupProperty(s => s.enabled);
        mockSpriteRenderer.SetupGet(s => s.material).Returns(mockMaterial.Object);

        var mockLabelText = new Mock<TextMeshPro>(IntPtr.Zero);
        mockLabelText.SetupProperty(t => t.color);
        mockLabelText.SetupProperty(t => t.fontMaterial);
        mockLabelText.SetupProperty(t => t.text);
        mockLabelText.SetupGet(t => t.transform).Returns(mockTransform.Object);

        var mockCoolText = new Mock<TextMeshPro>(IntPtr.Zero);
        mockCoolText.SetupProperty(t => t.color);
        mockCoolText.SetupProperty(t => t.enableWordWrapping);
        mockCoolText.SetupProperty(t => t.text);
        mockCoolText.SetupGet(t => t.gameObject).Returns(mockGameObject.Object);
        mockCoolText.SetupGet(t => t.transform).Returns(mockTransform.Object);

        var mockPersistentCallGroup = new Mock<PersistentCallGroup>(IntPtr.Zero);
        mockPersistentCallGroup.Setup(p => p.Clear());

        var mockOnClick = new Mock<UnityEngine.UI.Button.ButtonClickedEvent>(IntPtr.Zero);
        mockOnClick.Setup(e => e.RemoveAllListeners());
        mockOnClick.Setup(e => e.AddListener(It.IsAny<UnityAction>()));
        mockOnClick.SetupGet(e => e.m_PersistentCalls).Returns(mockPersistentCallGroup.Object);

        var mockPassiveButton = new Mock<PassiveButton>(IntPtr.Zero);
        mockPassiveButton.SetupGet(p => p.OnClick).Returns(mockOnClick.Object);

        var mockKillButton = new Mock<KillButton>(IntPtr.Zero);
        mockKillButton.SetupGet(b => b.transform).Returns(mockTransform.Object);
        mockKillButton.SetupGet(b => b.gameObject).Returns(mockGameObject.Object);
        mockKillButton.SetupGet(b => b.graphic).Returns(mockSpriteRenderer.Object);
        mockKillButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
        mockKillButton.SetupGet(b => b.cooldownTimerText).Returns(mockCoolText.Object);
        mockKillButton.Setup(b => b.GetComponent<PassiveButton>()).Returns(mockPassiveButton.Object);
        mockKillButton.SetupGet(b => b.isActiveAndEnabled).Returns(true);
        mockKillButton.Setup(b => b.OverrideText(It.IsAny<string>()));
        mockKillButton.Setup(b => b.SetCoolDown(It.IsAny<float>(), It.IsAny<float>()));
        mockKillButton.Setup(b => b.SetCooldownFill(It.IsAny<float>()));

        var mockUseButton = new Mock<UseButton>(IntPtr.Zero);
        mockUseButton.SetupGet(b => b.buttonLabelText).Returns(mockLabelText.Object);
        mockUseButton.SetupGet(b => b.transform).Returns(mockTransform.Object);

        var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
        mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => original);
        MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

        var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
        mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => original);
        MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

        var mockUnityActionImplicit = new Mock<MockUnityActionop_ImplicitHelper>();
        mockUnityActionImplicit.Setup(x => x.Invoke(It.IsAny<Action>()))
            .Returns((Action action) => action != null ? new UnityAction(IntPtr.Zero) : null!);
        MockUnityActionop_ImplicitHelper.Instance = mockUnityActionImplicit.Object;

        var mockHud = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();
        mockHud.SetupGet(h => h.KillButton).Returns(mockKillButton.Object);
        mockHud.SetupGet(h => h.UseButton).Returns(mockUseButton.Object);
    }
}
