using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using InnerNet;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Event;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Crewmate.Exorcist;
using ExtremeRoles.Roles.Solo.Impostor;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Exorcist;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ExorcistRoleTests
{
    public ExorcistRoleTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupObjectImplicitHelpers();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        MockSetupHelper.SetupLobbyMock();
        MockSetupHelper.SetupExtremeSystemTypeManagerMock();
        MockSetupHelper.SetupPlayerControlMocks();
        SetupGameOptionsManagerMock();

        var shipStateField = typeof(ExtremeRolesPlugin).GetField("<ShipState>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
        shipStateField?.SetValue(null, new ExtremeShipStatus());

        var eventManagerField = typeof(ExtremeRoles.Module.Event.EventManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
        eventManagerField?.SetValue(null, new ExtremeRoles.Module.Event.EventManager());

        MeetingReporter.Reset();
        _ = MeetingReporter.Instance;
        ExtremeRoleManager.GameRole.Clear();

        var clientMock = MockSetupHelper.SetupAmongUsClientMock();
        var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
        clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
            .Returns(mockWriter.Object);

        var mockTranslation = MockSetupHelper.SetupDestroyableSingletonMock<TranslationController>();
        mockTranslation.Setup(t => t.GetString(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Il2CppSystem.Object[]>()))
            .Returns((string id, string defaultStr, Il2CppSystem.Object[] parts) => defaultStr ?? id);

        var mockActionImplicit = new Mock<Il2CppSystem.MockActionop_ImplicitHelper<float>>();
        mockActionImplicit.Setup(x => x.Invoke(It.IsAny<Action<float>>()))
            .Returns((Action<float> act) => act != null ? new Il2CppSystem.Action<float>(IntPtr.Zero) : null!);
        Il2CppSystem.MockActionop_ImplicitHelper<float>.Instance = mockActionImplicit.Object;

        var mockMeetingHudHelper = new Mock<MockMeetingHudget_InstanceHelper>();
        mockMeetingHudHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
        MockMeetingHudget_InstanceHelper.Instance = mockMeetingHudHelper.Object;

        var mockDestroyableMeetingHelper = new Mock<MockDestroyableSingletonget_InstanceHelper<MeetingHud>>();
        mockDestroyableMeetingHelper.Setup(x => x.Invoke()).Returns((MeetingHud)null!);
        MockDestroyableSingletonget_InstanceHelper<MeetingHud>.Instance = mockDestroyableMeetingHelper.Object;

        var mockExileControllerHelper = new Mock<MockExileControllerget_InstanceHelper>();
        mockExileControllerHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
        MockExileControllerget_InstanceHelper.Instance = mockExileControllerHelper.Object;

        var mockDestroyableExileHelper = new Mock<MockDestroyableSingletonget_InstanceHelper<ExileController>>();
        mockDestroyableExileHelper.Setup(x => x.Invoke()).Returns((ExileController)null!);
        MockDestroyableSingletonget_InstanceHelper<ExileController>.Instance = mockDestroyableExileHelper.Object;

        if (InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance == null)
        {
            var mockWriteNetObj = new Mock<InnerNet.MockMessageExtensionsWriteNetObjectHelper>();
            mockWriteNetObj.Setup(w => w.Invoke(It.IsAny<MessageWriter>(), It.IsAny<InnerNetObject>()));
            InnerNet.MockMessageExtensionsWriteNetObjectHelper.Instance = mockWriteNetObj.Object;
        }

        if (Hazel.MockMessageWriterGetHelper.Instance == null)
        {
            var mockGet = new Mock<Hazel.MockMessageWriterGetHelper>();
            mockGet.Setup(g => g.Invoke(It.IsAny<SendOption>())).Returns(mockWriter.Object);
            Hazel.MockMessageWriterGetHelper.Instance = mockGet.Object;
        }

        if (Hazel.MockMessageReaderGetHelper.Instance == null)
        {
            var mockReaderHelper = new Mock<Hazel.MockMessageReaderGetHelper>();
            var mockReader = new Mock<MessageReader>();
            mockReaderHelper.Setup(g => g.Invoke(It.IsAny<Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<byte>>()))
                .Returns(mockReader.Object);
            Hazel.MockMessageReaderGetHelper.Instance = mockReaderHelper.Object;
        }

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

    private static void SetupGameOptionsManagerMock()
    {
        if (MockGameOptionsManagerget_InstanceHelper.Instance == null)
        {
            var mockGameOptions = new Mock<IGameOptions>(IntPtr.Zero);
            var mockGameOptionsManager = new Mock<GameOptionsManager>(IntPtr.Zero);
            mockGameOptionsManager.SetupGet(g => g.CurrentGameOptions).Returns(mockGameOptions.Object);

            var mockOptionsMgrHelper = new Mock<MockGameOptionsManagerget_InstanceHelper>();
            mockOptionsMgrHelper.Setup(h => h.Invoke()).Returns(mockGameOptionsManager.Object);
            MockGameOptionsManagerget_InstanceHelper.Instance = mockOptionsMgrHelper.Object;
        }
    }

    [Theory]
    [InlineData(ExorcistRole.BlockMode.BlockModeAbilityButton, 1)]
    [InlineData(ExorcistRole.BlockMode.BlockModeReportButton, 1)]
    [InlineData(ExorcistRole.BlockMode.BlockModeKillButton, 1)]
    [InlineData(ExorcistRole.BlockMode.BlockModeAbilityAndReportButton, 2)]
    [InlineData(ExorcistRole.BlockMode.BlockModeAbilityAndKillButton, 2)]
    [InlineData(ExorcistRole.BlockMode.BlockModeKillAndReportButton, 2)]
    [InlineData(ExorcistRole.BlockMode.BlockModeAll, 3)]
    [InlineData(ExorcistRole.BlockMode.BlockModeNone, 0)]
    public void Initialize_WithDifferentBlockModes_InitializesCorrectLockSystemCount(ExorcistRole.BlockMode blockMode, int expectedLockSystemCount)
    {
        // Arrange
        var role = new ExorcistRole();
        role.CreateRoleAllOption();

        if (role.Loader.TryGet(ExorcistRole.Option.CrewBlockSystemType, out var option) && option != null)
        {
            option.Selection = (int)blockMode;
        }

        // Act
        role.Initialize();

        // Assert
        Assert.NotNull(role.Status);

        var lockSystemField = typeof(ExorcistRole).GetField("lockSystem", BindingFlags.NonPublic | BindingFlags.Instance);
        var lockSystems = lockSystemField?.GetValue(role) as List<ButtonLockSystem>;

        Assert.NotNull(lockSystems);
        Assert.Equal(expectedLockSystemCount, lockSystems.Count);
    }

    [Fact]
    public void Update_ExecutesStatusFrameUpdateAndAwakesToFakeImpostor()
    {
        // Arrange
        var role = new ExorcistRole();
        role.CreateRoleAllOption();

        if (role.Loader.TryGet(ExorcistRole.Option.AwakeTaskGage, out var awakeOption) && awakeOption != null)
        {
            awakeOption.Selection = 5; // 50%
        }

        role.Initialize();

        var statusBefore = role.Status as ExorcistStatus;
        Assert.NotNull(statusBefore);
        Assert.False(statusBefore.IsFakeImpostor);

        var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

        var task1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
        task1.SetupGet(t => t.Complete).Returns(true);

        mockTasksList.SetupGet(l => l.Count).Returns(1);
        mockTasksList.Setup(l => l[0]).Returns(task1.Object);

        mockPlayerInfo.SetupGet(p => p.Tasks).Returns(mockTasksList.Object);
        mockPlayer.SetupGet(p => p.Data).Returns(mockPlayerInfo.Object);

        // Act
        role.Update(mockPlayer.Object);

        // Assert
        Assert.True(statusBefore.IsFakeImpostor);
    }

    [Fact]
    public void IsAbilityUse_WhenCurTargetIsNull_SetsTmpTargetToNullAndReturnsFalse()
    {
        // Arrange
        MockSetupHelper.SetupPlayerControlMocks();
        var role = new ExorcistRole();
        role.CreateRoleAllOption();
        role.Initialize();

        // Act
        var isUse = role.IsAbilityUse();

        // Assert
        Assert.False(isUse);

        var tmpTargetField = typeof(ExorcistRole).GetField("tmpTarget", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.Null(tmpTargetField?.GetValue(role));
    }

    [Fact]
    public void IsAbilityActive_WhenTargetMatchesCurTarget_ReturnsTrue_OtherwiseReturnsFalse()
    {
        // Arrange
        MockSetupHelper.SetupPlayerControlMocks();
        var role = new ExorcistRole();
        role.CreateRoleAllOption();
        role.Initialize();

        // Act & Assert 1: Both target and status.CurTarget null
        Assert.True(role.IsAbilityActive());

        // Act & Assert 2: Target set to non-null, status.CurTarget is null
        var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        var targetField = typeof(ExorcistRole).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
        targetField?.SetValue(role, mockTarget.Object);

        Assert.False(role.IsAbilityActive());
    }

    [Fact]
    public void UseAbility_WhenLocalPlayerIsNull_ReturnsFalse()
    {
        // Arrange
        var mockLocalHelper = new Mock<MockPlayerControlget_LocalPlayerHelper>();
        mockLocalHelper.Setup(h => h.Invoke()).Returns((PlayerControl)null!);
        MockPlayerControlget_LocalPlayerHelper.Instance = mockLocalHelper.Object;

        var role = new ExorcistRole();

        // Act
        var result = role.UseAbility();

        // Assert
        Assert.False(result);

        // Clean up
        MockPlayerControlget_LocalPlayerHelper.Instance = null;
    }

    [Fact]
    public void UseAbility_WhenLocalPlayerIsNotNull_SetsTargetToTmpTarget_SendsRpc_ReturnsTrue()
    {
        // Arrange
        var mockPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockPlayer.SetupGet(p => p.PlayerId).Returns((byte)3);
        mockPlayer.SetupGet(p => p.NetId).Returns(200u);

        var clientMock = MockSetupHelper.SetupAmongUsClientMock();
        var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
        clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
            .Returns(mockWriter.Object);

        var role = new ExorcistRole();
        role.CreateRoleAllOption();
        role.Initialize();

        var mockTarget = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        var tmpTargetField = typeof(ExorcistRole).GetField("tmpTarget", BindingFlags.NonPublic | BindingFlags.Instance);
        tmpTargetField?.SetValue(role, mockTarget.Object);

        // Act
        var result = role.UseAbility();

        // Assert
        Assert.True(result);

        var targetField = typeof(ExorcistRole).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.Same(mockTarget.Object, targetField?.GetValue(role));

        clientMock.Verify(c => c.StartRpcImmediately(200u, (byte)RPCOperator.Command.ExorcistOps, It.IsAny<SendOption>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void CleanUp_WhenTargetIsNull_DoesNotAddChatReport()
    {
        // Arrange
        MeetingReporter.Reset();
        var role = new ExorcistRole();
        role.CreateRoleAllOption();
        role.Initialize();

        // Act
        role.CleanUp();

        // Assert
        Assert.False(MeetingReporter.Instance.HasChatReport);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CleanUp_WhenTargetIsNotNullAndInDeadPlayerInfo_AddsChatReportAndCallsCmdReportDeadBody(bool withName)
    {
        // Arrange
        MeetingReporter.Reset();

        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

        var mockLocalInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockLocalInfo.SetupGet(i => i.Object).Returns(mockLocalPlayer.Object);
        mockLocalInfo.SetupGet(i => i.IsDead).Returns(false);
        mockLocalInfo.SetupGet(i => i.Disconnected).Returns(false);
        mockLocalPlayer.SetupGet(p => p.Data).Returns(mockLocalInfo.Object);

        var role = new ExorcistRole();
        role.CreateRoleAllOption();

        if (role.Loader.TryGet(ExorcistRole.Option.WithName, out var withNameOption) && withNameOption != null)
        {
            withNameOption.Selection = withName ? 1 : 0;
        }

        role.Initialize();

        byte victimId = 10;

        var mockTargetOutfit = new Mock<NetworkedPlayerInfo.PlayerOutfit>(IntPtr.Zero);
        mockTargetOutfit.SetupGet(o => o.PlayerName).Returns("VictimPlayer");

        var mockTargetInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockTargetInfo.SetupGet(t => t.PlayerId).Returns(victimId);
        mockTargetInfo.SetupGet(t => t.DefaultOutfit).Returns(mockTargetOutfit.Object);

        var mockTargetPlayerControl = new Mock<PlayerControl>(IntPtr.Zero);
        mockTargetPlayerControl.SetupGet(p => p.PlayerId).Returns(victimId);
        mockTargetPlayerControl.SetupGet(p => p.Data).Returns(mockTargetInfo.Object);

        var mockKillerOutfit = new Mock<NetworkedPlayerInfo.PlayerOutfit>(IntPtr.Zero);
        mockKillerOutfit.SetupGet(o => o.PlayerName).Returns("KillerPlayer");

        var mockKillerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockKillerInfo.SetupGet(k => k.DefaultOutfit).Returns(mockKillerOutfit.Object);
        mockKillerInfo.SetupGet(k => k.IsDead).Returns(false);

        var mockKillerPlayerControl = new Mock<PlayerControl>(IntPtr.Zero);
        mockKillerPlayerControl.SetupGet(k => k.PlayerId).Returns((byte)11);
        mockKillerPlayerControl.SetupGet(k => k.Data).Returns(mockKillerInfo.Object);

        var targetField = typeof(ExorcistRole).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
        targetField?.SetValue(role, mockTargetInfo.Object);

        // Add DeadPlayerInfo in ShipState
        ExtremeRolesPlugin.ShipState.AddDeadInfo(
            mockTargetPlayerControl.Object,
            DeathReason.Kill,
            mockKillerPlayerControl.Object);

        // Act
        role.CleanUp();

        // Assert
        Assert.True(MeetingReporter.Instance.HasChatReport);
        mockLocalPlayer.Verify(p => p.CmdReportDeadBody(mockTargetInfo.Object), Times.Once);
        Assert.Null(targetField?.GetValue(role));
    }

    [Fact]
    public void ResetOnMeetingStart_DisablesFlasherRenderer()
    {
        // Arrange
        var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

        var mockTransform = new Mock<Transform>(IntPtr.Zero);
        var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
        var mockRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);

        mockRenderer.SetupGet(r => r.transform).Returns(mockTransform.Object);
        mockRenderer.SetupGet(r => r.gameObject).Returns(mockGameObject.Object);
        mockRenderer.SetupProperty(r => r.enabled);

        hudMock.SetupGet(h => h.FullScreen).Returns(mockRenderer.Object);
        hudMock.SetupGet(h => h.transform).Returns(mockTransform.Object);

        var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
        mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

        var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
        mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

        var mockLerpHelper = new Mock<MockEffectsLerpHelper>();
        mockLerpHelper.Setup(x => x.Invoke(It.IsAny<float>(), It.IsAny<Il2CppSystem.Action<float>>()))
            .Returns((Il2CppSystem.Collections.IEnumerator)null!);
        MockEffectsLerpHelper.Instance = mockLerpHelper.Object;

        var role = new ExorcistRole();

        var flasherField = typeof(ExorcistRole).GetField("flasher", BindingFlags.NonPublic | BindingFlags.Instance);
        var flasher = flasherField?.GetValue(role) as FullScreenFlasher;
        Assert.NotNull(flasher);

        flasher.Flash();
        Assert.True(mockRenderer.Object.enabled);

        // Act
        role.ResetOnMeetingStart();

        // Assert
        Assert.False(mockRenderer.Object.enabled);
    }

    [Fact]
    public void RpcOps_WhenExorcistRoleNotFound_DoesNotChangeAnyState()
    {
        // Arrange
        ExtremeRoleManager.GameRole.Clear();

        var reader = new Mock<MessageReader>();
        reader.SetupSequence(r => r.ReadByte())
            .Returns((byte)ExorcistRole.RpcOpsMode.Alert)
            .Returns((byte)99);

        // Act
        ExorcistRole.RpcOps(reader.Object);

        // Assert: No role found for ID 99
        Assert.False(ExtremeRoleManager.TryGetRole(99, out _));
    }

    [Fact]
    public void RpcOps_AwakeFakeImp_UpdatesStatusToFakeImpostor()
    {
        // Arrange
        byte exorcistId = 1;
        byte localPlayerId = 2;

        var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
        localPlayerMock.SetupGet(p => p.PlayerId).Returns(localPlayerId);

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.SetPlyerIdToSingleRoleId((int)ExtremeRoleId.Exorcist, exorcistId, 0);

        var role = ExtremeRoleManager.GetSafeCastedRole<ExorcistRole>(exorcistId);
        Assert.NotNull(role);

        var statusBefore = role.Status as ExorcistStatus;
        Assert.NotNull(statusBefore);

        var reader = new Mock<MessageReader>();
        reader.SetupSequence(r => r.ReadByte())
            .Returns((byte)ExorcistRole.RpcOpsMode.AwakeFakeImp)
            .Returns(exorcistId);

        // Act
        ExorcistRole.RpcOps(reader.Object);

        // Assert
        Assert.True(statusBefore.IsFakeImpostor);
        Assert.Equal(ExtremeRoleType.Impostor, statusBefore.FakeTeam);
    }

    [Fact]
    public void RpcOps_Alert_WhenHudManagerNull_ReturnsEarlyWithoutFlashing()
    {
        // Arrange
        byte exorcistId = 1;
        byte localPlayerId = 2;

        var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
        localPlayerMock.SetupGet(p => p.PlayerId).Returns(localPlayerId);

        var role = new ExorcistRole();
        role.CreateRoleAllOption();
        role.Initialize();

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.SetPlyerIdToSingleRoleId((int)ExtremeRoleId.Exorcist, exorcistId, 0);

        var mockSingleton = new Mock<MockDestroyableSingletonget_InstanceHelper<HudManager>>();
        mockSingleton.Setup(x => x.Invoke()).Returns((HudManager)null!);
        MockDestroyableSingletonget_InstanceHelper<HudManager>.Instance = mockSingleton.Object;

        var reader = new Mock<MessageReader>();
        reader.SetupSequence(r => r.ReadByte())
            .Returns((byte)ExorcistRole.RpcOpsMode.Alert)
            .Returns(exorcistId);

        // Act
        ExorcistRole.RpcOps(reader.Object);

        // Assert: Verified HudManager.Instance accessed and returned null
        mockSingleton.Verify(x => x.Invoke(), Times.AtLeastOnce());
    }

    [Fact]
    public void RpcOps_Alert_WhenLocalPlayerIsCrewmate_DoesNotStartCoroutine()
    {
        // Arrange
        byte exorcistId = 1;
        byte localPlayerId = 2;

        var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
        localPlayerMock.SetupGet(p => p.PlayerId).Returns(localPlayerId);

        var exorcist = new ExorcistRole();
        exorcist.CreateRoleAllOption();
        exorcist.Initialize();

        var crewmate = new SpecialCrew();
        crewmate.CreateRoleAllOption();
        crewmate.Initialize();

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.SetPlyerIdToSingleRoleId((int)ExtremeRoleId.Exorcist, exorcistId, 0);
        ExtremeRoleManager.SetPlyerIdToSingleRoleId((int)ExtremeRoleId.SpecialCrew, localPlayerId, 0);

        var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

        var reader = new Mock<MessageReader>();
        reader.SetupSequence(r => r.ReadByte())
            .Returns((byte)ExorcistRole.RpcOpsMode.Alert)
            .Returns(exorcistId);

        // Act
        ExorcistRole.RpcOps(reader.Object);

        // Assert
        hudMock.Verify(h => h.StartCoroutine(It.IsAny<Il2CppSystem.Collections.IEnumerator>()), Times.Never());
    }

    [Fact]
    public void RpcOps_Alert_WhenLocalPlayerIsImpostor_Flashes()
    {
        // Arrange
        byte exorcistId = 1;
        byte localPlayerId = 2;

        var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
        localPlayerMock.SetupGet(p => p.PlayerId).Returns(localPlayerId);

        var exorcist = new ExorcistRole();
        exorcist.CreateRoleAllOption();
        exorcist.Initialize();

        var impostor = new SpecialImpostor();
        impostor.CreateRoleAllOption();
        impostor.Initialize();

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.SetPlyerIdToSingleRoleId((int)ExtremeRoleId.Exorcist, exorcistId, 0);
        ExtremeRoleManager.SetPlyerIdToSingleRoleId((int)ExtremeRoleId.SpecialImpostor, localPlayerId, 0);

        var hudMock = MockSetupHelper.SetupDestroyableSingletonMock<HudManager>();

        var mockTransform = new Mock<Transform>(IntPtr.Zero);
        var mockGameObject = new Mock<GameObject>(IntPtr.Zero);
        var mockRenderer = new Mock<SpriteRenderer>(IntPtr.Zero);

        mockRenderer.SetupGet(r => r.transform).Returns(mockTransform.Object);
        mockRenderer.SetupGet(r => r.gameObject).Returns(mockGameObject.Object);
        mockRenderer.SetupProperty(r => r.enabled);

        hudMock.SetupGet(h => h.FullScreen).Returns(mockRenderer.Object);
        hudMock.SetupGet(h => h.transform).Returns(mockTransform.Object);

        var mockInstantiate5 = new Mock<MockObjectInstantiateHelper5>();
        mockInstantiate5.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper5.Instance = mockInstantiate5.Object;

        var mockInstantiate10 = new Mock<MockObjectInstantiateHelper10>();
        mockInstantiate10.Setup(x => x.Invoke(It.IsAny<UnityEngine.Object>(), It.IsAny<Transform>()))
            .Returns((UnityEngine.Object original, Transform parent) => mockRenderer.Object);
        MockObjectInstantiateHelper10.Instance = mockInstantiate10.Object;

        var mockLerpHelper = new Mock<MockEffectsLerpHelper>();
        mockLerpHelper.Setup(x => x.Invoke(It.IsAny<float>(), It.IsAny<Il2CppSystem.Action<float>>()))
            .Returns((Il2CppSystem.Collections.IEnumerator)null!);
        MockEffectsLerpHelper.Instance = mockLerpHelper.Object;

        var reader = new Mock<MessageReader>();
        reader.SetupSequence(r => r.ReadByte())
            .Returns((byte)ExorcistRole.RpcOpsMode.Alert)
            .Returns(exorcistId);

        // Act
        ExorcistRole.RpcOps(reader.Object);

        // Assert
        Assert.False(ExtremeRoleManager.GetLocalPlayerRole().IsCrewmate());
        hudMock.Verify(h => h.StartCoroutine(It.IsAny<Il2CppSystem.Collections.IEnumerator>()), Times.Once());
    }

    [Fact]
    public void ExorcistBlockCondition_DuringTaskPhase_BehavesBasedOnLocalRole()
    {
        // Arrange
        byte exorcistId = 10;
        byte crewmateId = 11;
        byte initialLocalPlayerId = 99;

        var localPlayerMock = MockSetupHelper.SetupPlayerControlMocks();
        localPlayerMock.SetupGet(p => p.PlayerId).Returns(initialLocalPlayerId);

        var exorcist = new ExorcistRole();
        exorcist.CreateRoleAllOption();

        if (exorcist.Loader.TryGet(ExorcistRole.Option.CrewBlockSystemType, out var blockOpt) && blockOpt != null)
        {
            blockOpt.Selection = (int)ExorcistRole.BlockMode.BlockModeAbilityButton;
        }

        exorcist.Initialize();

        var blockSys = ButtonLockSystem.CreateOrGetAbilityButtonLockSystem();

        // Test Case 1: GameProgressSystem.IsTaskPhase = false -> block condition returns true
        ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
        GameProgressSystem.Current = GameProgressSystem.Progress.None;

        blockSys.Lock((int)ButtonLockSystem.ConditionId.Exorcist);
        Assert.True(ButtonLockSystem.IsAbilityButtonLock());

        // Test Case 2: GameProgressSystem.IsTaskPhase = true & Local player is Exorcist -> block condition returns false
        GameProgressSystem.Current = GameProgressSystem.Progress.RoleSetUpEnd;
        GameProgressSystem.Current = GameProgressSystem.Progress.Task;

        ExtremeRoleManager.GameRole.Clear();
        ExtremeRoleManager.SetPlyerIdToSingleRoleId((int)ExtremeRoleId.Exorcist, exorcistId, 0);

        localPlayerMock.SetupGet(p => p.PlayerId).Returns(exorcistId);

        blockSys.UnLock((int)ButtonLockSystem.ConditionId.Exorcist);
        blockSys.Lock((int)ButtonLockSystem.ConditionId.Exorcist);
        Assert.False(ButtonLockSystem.IsAbilityButtonLock());

        // Test Case 3: GameProgressSystem.IsTaskPhase = true & Local player is Crewmate -> block condition returns true
        var crewmateRole = new SpecialCrew();
        crewmateRole.CreateRoleAllOption();

        ExtremeRoleManager.SetPlyerIdToSingleRoleId((int)ExtremeRoleId.SpecialCrew, crewmateId, 0);

        localPlayerMock.SetupGet(p => p.PlayerId).Returns(crewmateId);

        blockSys.UnLock((int)ButtonLockSystem.ConditionId.Exorcist);
        blockSys.Lock((int)ButtonLockSystem.ConditionId.Exorcist);
        Assert.True(ButtonLockSystem.IsAbilityButtonLock());
    }
}
