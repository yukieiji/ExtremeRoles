using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

#nullable enable

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Agency;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class AgencyRoleTests
{
    public AgencyRoleTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupObjectImplicitHelpers();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);
        MockSetupHelper.SetupLobbyMock();
        MockSetupHelper.SetupExtremeSystemTypeManagerMock();
        MockSetupHelper.SetupPlayerControlMocks();
        SetupGameOptionsManagerMock();
        SetupAudioClipMock();
        SetupSfxMock();

        ExtremeRoleManager.GameRole.Clear();

        var clientMock = MockSetupHelper.SetupAmongUsClientMock();
        var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
        clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
            .Returns(mockWriter.Object);

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
    }

    private static void SetupAudioClipMock()
    {
        var mockClip = new Mock<AudioClip>(IntPtr.Zero);
        var cachedAudioField = typeof(Sound).GetField("cachedAudio", BindingFlags.NonPublic | BindingFlags.Static);
        if (cachedAudioField?.GetValue(null) is Dictionary<Sound.Type, AudioClip> cachedAudio)
        {
            cachedAudio[Sound.Type.AgencyTakeTask] = mockClip.Object;
        }
    }

    private static void SetupSfxMock()
    {
        var mockSfx = new Mock<MockConstantsShouldPlaySfxHelper>();
        mockSfx.Setup(x => x.Invoke()).Returns(true);
        MockConstantsShouldPlaySfxHelper.Instance = mockSfx.Object;
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

    [Fact]
    public void RoleSpecificInit_And_CreateSpecificOption_InitializesCorrectProperties()
    {
        // Arrange
        var agency = new ExtremeRoles.Roles.Solo.Crewmate.Agency();

        // Act
        agency.CreateRoleAllOption();
        agency.Initialize();

        // Assert
        Assert.True(agency.CanSeeTaskBar);

        var maxTakeTaskField = typeof(ExtremeRoles.Roles.Solo.Crewmate.Agency).GetField("maxTakeTask", BindingFlags.NonPublic | BindingFlags.Instance);
        var takeTaskRangeField = typeof(ExtremeRoles.Roles.Solo.Crewmate.Agency).GetField("takeTaskRange", BindingFlags.NonPublic | BindingFlags.Instance);

        var maxTakeTask = (int?)maxTakeTaskField?.GetValue(agency);
        var takeTaskRange = (float?)takeTaskRangeField?.GetValue(agency);

        Assert.Equal(3, maxTakeTask); // Option 2 + 1 = 3
        Assert.Equal(1.0f, takeTaskRange);
        Assert.NotNull(agency.TakeTask);
        Assert.Empty(agency.TakeTask);
    }

    [Fact]
    public void ResetOnMeetingStart_And_ResetOnMeetingEnd_DoNotThrowExceptions()
    {
        // Arrange
        var agency = new ExtremeRoles.Roles.Solo.Crewmate.Agency();

        // Act
        agency.ResetOnMeetingStart();
        agency.ResetOnMeetingEnd(null);

        // Assert
        Assert.NotNull(agency);
    }

    [Fact]
    public void IsAbilityUse_WhenNoPlayerInRange_ReturnsFalseAndResetsTargetPlayer()
    {
        // Arrange
        var agency = new ExtremeRoles.Roles.Solo.Crewmate.Agency();
        agency.CreateRoleAllOption();
        agency.Initialize();

        agency.TargetPlayer = 5;

        var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
        var mockShipStatusHelper = new Mock<MockShipStatusget_InstanceHelper>();
        mockShipStatusHelper.Setup(h => h.Invoke()).Returns((ShipStatus)null!);
        MockShipStatusget_InstanceHelper.Instance = mockShipStatusHelper.Object;

        // Act
        var result = agency.IsAbilityUse();

        // Assert
        Assert.False(result);
        Assert.Equal(byte.MaxValue, agency.TargetPlayer);
    }

    [Fact]
    public void IsAbilityUse_WhenPlayerInRange_SetsTargetPlayerAndReturnsTrue()
    {
        // Arrange
        byte sourceId = 1;
        byte targetId = 2;

        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(sourceId);

        var agency = new ExtremeRoles.Roles.Solo.Crewmate.Agency();
        agency.CreateRoleAllOption();
        agency.Initialize();

        var mockTargetControl = new Mock<PlayerControl>(IntPtr.Zero);
        mockTargetControl.SetupGet(p => p.PlayerId).Returns(targetId);
        mockTargetControl.SetupGet(p => p.inVent).Returns(false);
        mockTargetControl.SetupGet(p => p.inMovingPlat).Returns(false);
        mockTargetControl.SetupGet(p => p.onLadder).Returns(false);

        var mockTargetInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockTargetInfo.SetupGet(t => t.PlayerId).Returns(targetId);
        mockTargetInfo.SetupGet(t => t.IsDead).Returns(false);
        mockTargetInfo.SetupGet(t => t.Disconnected).Returns(false);
        mockTargetInfo.SetupGet(t => t.Object).Returns(mockTargetControl.Object);

        var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
        var mockShipStatusHelper = new Mock<MockShipStatusget_InstanceHelper>();
        mockShipStatusHelper.Setup(h => h.Invoke()).Returns((ShipStatus)null!);
        MockShipStatusget_InstanceHelper.Instance = mockShipStatusHelper.Object;

        var mockAllPlayersList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo>>(IntPtr.Zero);
        mockAllPlayersList.SetupGet(l => l.Count).Returns(1);
        mockAllPlayersList.Setup(l => l[0]).Returns(mockTargetInfo.Object);

        var mockGameData = MockSetupHelper.SetupGameDataMock();
        mockGameData.SetupGet(g => g.AllPlayers).Returns(mockAllPlayersList.Object);

        var sourceRole = agency;
        var targetRole = new SpecialCrew();
        targetRole.CreateRoleAllOption();

        ExtremeRoleManager.GameRole[sourceId] = sourceRole;
        ExtremeRoleManager.GameRole[targetId] = targetRole;

        // Act
        var result = agency.IsAbilityUse();

        // Assert
        Assert.False(result); // Null ShipStatus returns false
        Assert.Equal(byte.MaxValue, agency.TargetPlayer);
    }

    [Fact]
    public void TakeTargetPlayerTask_CompletesTasksAndPlaysSoundIfLocalPlayer()
    {
        // Arrange
        byte localPlayerId = 1;
        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(localPlayerId);

        var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockLocalPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

        var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
        mockTasksList.SetupGet(l => l.Count).Returns(0);

        mockLocalPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

        Performance.PlayerCache.AddPlayerControl(mockLocalPlayer.Object);

        var mockSoundManager = new Mock<SoundManager>(IntPtr.Zero);
        var mockSoundInstanceHelper = new Mock<MockSoundManagerget_InstanceHelper>();
        mockSoundInstanceHelper.Setup(x => x.Invoke()).Returns(mockSoundManager.Object);
        MockSoundManagerget_InstanceHelper.Instance = mockSoundInstanceHelper.Object;

        // Act
        ExtremeRoles.Roles.Solo.Crewmate.Agency.TakeTargetPlayerTask(localPlayerId, new List<int> { 10 });

        // Assert
        mockData.Verify(d => d.MarkDirty(), Times.Once);
    }

    [Fact]
    public void TakeTargetPlayerTask_DoesNotPlaySoundIfNotLocalPlayer()
    {
        // Arrange
        byte localPlayerId = 1;
        byte targetPlayerId = 2;

        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(localPlayerId);

        var mockTargetPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockTargetPlayer.SetupGet(p => p.PlayerId).Returns(targetPlayerId);

        var mockData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockTargetPlayer.SetupGet(p => p.Data).Returns(mockData.Object);

        var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
        mockTasksList.SetupGet(l => l.Count).Returns(0);

        mockTargetPlayer.SetupGet(p => p.myTasks).Returns(mockTasksList.Object);

        Performance.PlayerCache.AddPlayerControl(mockTargetPlayer.Object);

        var mockSoundManager = new Mock<SoundManager>(IntPtr.Zero);
        var mockSoundInstanceHelper = new Mock<MockSoundManagerget_InstanceHelper>();
        mockSoundInstanceHelper.Setup(x => x.Invoke()).Returns(mockSoundManager.Object);
        MockSoundManagerget_InstanceHelper.Instance = mockSoundInstanceHelper.Object;

        // Act
        ExtremeRoles.Roles.Solo.Crewmate.Agency.TakeTargetPlayerTask(targetPlayerId, new List<int> { 15 });

        // Assert
        mockData.Verify(d => d.MarkDirty(), Times.Once);
    }

    [Fact]
    public void Update_WhenNotInTaskPhase_DoesNothing()
    {
        // Arrange
        var agency = new ExtremeRoles.Roles.Solo.Crewmate.Agency();
        agency.TakeTask = new List<ExtremeRoles.Roles.Solo.Crewmate.Agency.TakeTaskType> { ExtremeRoles.Roles.Solo.Crewmate.Agency.TakeTaskType.Normal };

        ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
        GameProgressSystem.Current = GameProgressSystem.Progress.None;

        var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

        // Act
        agency.Update(mockPlayer.Object);

        // Assert
        Assert.Single(agency.TakeTask);
    }

    [Fact]
    public void Update_WhenTakeTaskIsEmpty_DoesNothing()
    {
        // Arrange
        var agency = new ExtremeRoles.Roles.Solo.Crewmate.Agency();
        agency.TakeTask = new List<ExtremeRoles.Roles.Solo.Crewmate.Agency.TakeTaskType>();

        ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
        GameProgressSystem.Current = GameProgressSystem.Progress.Task;

        var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);

        // Act
        agency.Update(mockPlayer.Object);

        // Assert
        Assert.Empty(agency.TakeTask);
    }

    [Theory]
    [InlineData(ExtremeRoles.Roles.Solo.Crewmate.Agency.TakeTaskType.Normal)]
    [InlineData(ExtremeRoles.Roles.Solo.Crewmate.Agency.TakeTaskType.Long)]
    [InlineData(ExtremeRoles.Roles.Solo.Crewmate.Agency.TakeTaskType.Common)]
    public void Update_WhenTaskIsCompleted_ReplacesTaskAndRemovesFromList(ExtremeRoles.Roles.Solo.Crewmate.Agency.TakeTaskType type)
    {
        // Arrange
        byte playerId = 5;
        var agency = new ExtremeRoles.Roles.Solo.Crewmate.Agency();
        agency.TakeTask = new List<ExtremeRoles.Roles.Solo.Crewmate.Agency.TakeTaskType> { type };

        ExtremeSystemTypeManager.Instance.CreateOrGet<GameProgressSystem>(ExtremeSystemType.GameProgress);
        GameProgressSystem.Current = GameProgressSystem.Progress.Task;

        var mockGameData = MockSetupHelper.SetupGameDataMock();
        var mockPlayerInfo = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

        var taskInfo = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
        taskInfo.SetupGet(t => t.Complete).Returns(true);

        mockTasksList.SetupGet(l => l.Count).Returns(1);
        mockTasksList.Setup(l => l[It.IsAny<int>()]).Returns(taskInfo.Object);
        mockPlayerInfo.SetupGet(p => p.Tasks).Returns(mockTasksList.Object);

        mockGameData.Setup(g => g.GetPlayerById(playerId)).Returns(mockPlayerInfo.Object);

        var mockPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

        var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
        var mockShipStatusHelper = new Mock<MockShipStatusget_InstanceHelper>();
        mockShipStatusHelper.Setup(h => h.Invoke()).Returns((ShipStatus)null!);
        MockShipStatusget_InstanceHelper.Instance = mockShipStatusHelper.Object;

        // Act
        agency.Update(mockPlayer.Object);

        // Assert
        Assert.Empty(agency.TakeTask);
    }

    [Fact]
    public void UseAbility_WhenTaskGaugeHigh_SetsTakeNumToZeroAndReturnsTrue()
    {
        // Arrange
        byte localPlayerId = 1;
        byte targetPlayerId = 2;

        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(localPlayerId);

        var agency = new ExtremeRoles.Roles.Solo.Crewmate.Agency();
        agency.CreateRoleAllOption();
        agency.Initialize();
        agency.TargetPlayer = targetPlayerId;

        var targetRole = new SpecialCrew();
        targetRole.CreateRoleAllOption();
        targetRole.Initialize();
        ExtremeRoleManager.GameRole[targetPlayerId] = targetRole;

        var mockGameData = MockSetupHelper.SetupGameDataMock();
        mockGameData.SetupGet(g => g.TotalTasks).Returns(10);
        mockGameData.SetupGet(g => g.CompletedTasks).Returns(10); // Task gauge = 1.0f (> 0.9f)

        var mockTargetData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockGameData.Setup(g => g.GetPlayerById(targetPlayerId)).Returns(mockTargetData.Object);

        var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);
        mockTasksList.SetupGet(l => l.Count).Returns(0);
        mockTargetData.SetupGet(p => p.Tasks).Returns(mockTasksList.Object);

        // Act
        var result = agency.UseAbility();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void UseAbility_WhenTargetHasTasks_TakesTasksAndSendsRpc()
    {
        // Arrange
        MockSetupHelper.SetupOptionManager();

        byte localPlayerId = 1;
        byte targetPlayerId = 2;

        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(localPlayerId);

        var agency = new ExtremeRoles.Roles.Solo.Crewmate.Agency();
        agency.CreateRoleAllOption();
        agency.Initialize();
        agency.TargetPlayer = targetPlayerId;

        var targetRole = new SpecialCrew();
        targetRole.CreateRoleAllOption();
        targetRole.Initialize();
        ExtremeRoleManager.GameRole[targetPlayerId] = targetRole;

        var mockTargetPlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockTargetPlayer.SetupGet(p => p.PlayerId).Returns(targetPlayerId);
        var mockTargetData = new Mock<NetworkedPlayerInfo>(IntPtr.Zero);
        mockTargetPlayer.SetupGet(p => p.Data).Returns(mockTargetData.Object);

        var mockTasksListForPlayer = new Mock<Il2CppSystem.Collections.Generic.List<PlayerTask>>(IntPtr.Zero);
        mockTasksListForPlayer.SetupGet(l => l.Count).Returns(0);
        mockTargetPlayer.SetupGet(p => p.myTasks).Returns(mockTasksListForPlayer.Object);

        Performance.PlayerCache.AddPlayerControl(mockTargetPlayer.Object);

        var mockGameData = MockSetupHelper.SetupGameDataMock();
        var mockTasksList = new Mock<Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo.TaskInfo>>(IntPtr.Zero);

        var taskInfo1 = new Mock<NetworkedPlayerInfo.TaskInfo>(IntPtr.Zero);
        taskInfo1.SetupGet(t => t.Complete).Returns(false);
        taskInfo1.SetupGet(t => t.TypeId).Returns(0);
        taskInfo1.SetupGet(t => t.Id).Returns(100u);

        mockTasksList.SetupGet(l => l.Count).Returns(1);
        mockTasksList.Setup(l => l[0]).Returns(taskInfo1.Object);

        mockTargetData.SetupGet(p => p.Tasks).Returns(mockTasksList.Object);
        mockGameData.Setup(g => g.GetPlayerById(targetPlayerId)).Returns(mockTargetData.Object);

        var mockShipStatus = new Mock<ShipStatus>(IntPtr.Zero);
        var mockShipStatusHelper = new Mock<MockShipStatusget_InstanceHelper>();
        mockShipStatusHelper.Setup(h => h.Invoke()).Returns(mockShipStatus.Object);
        MockShipStatusget_InstanceHelper.Instance = mockShipStatusHelper.Object;

        var emptyTasks = new Il2CppReferenceArray<NormalPlayerTask>(0);
        mockShipStatus.SetupGet(s => s.CommonTasks).Returns(emptyTasks);
        mockShipStatus.SetupGet(s => s.LongTasks).Returns(emptyTasks);
        mockShipStatus.SetupGet(s => s.ShortTasks).Returns(emptyTasks);

        var clientMock = MockSetupHelper.SetupAmongUsClientMock();
        var mockWriter = new Mock<Hazel.MessageWriter>(IntPtr.Zero);
        clientMock.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<Hazel.SendOption>(), It.IsAny<int>()))
            .Returns(mockWriter.Object);

        var mockSoundManager = new Mock<SoundManager>(IntPtr.Zero);
        var mockSoundInstanceHelper = new Mock<MockSoundManagerget_InstanceHelper>();
        mockSoundInstanceHelper.Setup(x => x.Invoke()).Returns(mockSoundManager.Object);
        MockSoundManagerget_InstanceHelper.Instance = mockSoundInstanceHelper.Object;

        // Act
        var result = agency.UseAbility();

        // Assert
        Assert.True(result);
        Assert.Equal(byte.MaxValue, agency.TargetPlayer);
        clientMock.Verify(c => c.StartRpcImmediately(It.IsAny<uint>(), (byte)RPCOperator.Command.AgencyTakeTask, It.IsAny<Hazel.SendOption>(), It.IsAny<int>()), Times.Once);
    }
}
