using System;
using System.Collections.Generic;
using System.Reflection;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.Solo.Crewmate.Fencer;
using ExtremeRoles.UnitTest;
using Hazel;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Crewmate.Fencer;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class FencerAbilityHandlerTests
{
    public FencerAbilityHandlerTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        var mockClient = MockSetupHelper.SetupAmongUsClientMock();
        var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
        mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
                  .Returns(mockWriter.Object);

        SetupSoundManagerAndCachedAudioMock();
        SetupShouldPlaySfxMock();
        SetupMapBehaviourMock(null);
        SetupMinigameMock(null);
    }

    private static void SetupShouldPlaySfxMock()
    {
        var mockSfxHelper = new Mock<MockConstantsShouldPlaySfxHelper>();
        mockSfxHelper.Setup(x => x.Invoke()).Returns(false);
        MockConstantsShouldPlaySfxHelper.Instance = mockSfxHelper.Object;
    }

    private static void SetupSoundManagerAndCachedAudioMock()
    {
        var mockSoundManager = new Mock<SoundManager>(IntPtr.Zero);
        mockSoundManager.Setup(s => s.PlaySound(It.IsAny<AudioClip>(), It.IsAny<bool>(), It.IsAny<float>(), null))
                         .Returns((AudioSource)null!);

        var mockSoundInstanceHelper = new Mock<MockSoundManagerget_InstanceHelper>();
        mockSoundInstanceHelper.Setup(x => x.Invoke()).Returns(mockSoundManager.Object);
        MockSoundManagerget_InstanceHelper.Instance = mockSoundInstanceHelper.Object;

        var cachedAudioField = typeof(Sound).GetField("cachedAudio", BindingFlags.NonPublic | BindingFlags.Static);
        if (cachedAudioField?.GetValue(null) is Dictionary<Sound.Type, AudioClip> cachedAudio)
        {
            var mockClip = new Mock<AudioClip>(IntPtr.Zero);
            cachedAudio[Sound.Type.GuardianAngleGuard] = mockClip.Object;
        }
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
    public void TryKilledFrom_WhenIsCounterFalse_ReturnsTrue()
    {
        // Arrange
        var status = new FencerStatusModel(5.0f)
        {
            IsCounter = false
        };
        var handler = new FencerAbilityHandler(status);

        var mockRolePlayer = new Mock<PlayerControl>(IntPtr.Zero);
        mockRolePlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

        var mockFromPlayer = new Mock<PlayerControl>(IntPtr.Zero);

        // Act
        var result = handler.TryKilledFrom(mockRolePlayer.Object, mockFromPlayer.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryKilledFrom_WhenIsCounterTrue_EnablesKillButtonAndReturnsFalse()
    {
        // Arrange
        var status = new FencerStatusModel(5.0f)
        {
            IsCounter = true
        };
        var handler = new FencerAbilityHandler(status);

        byte localPlayerId = 1;
        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(localPlayerId);

        var mockFromPlayer = new Mock<PlayerControl>(IntPtr.Zero);

        // Act
        var result = handler.TryKilledFrom(mockLocalPlayer.Object, mockFromPlayer.Object);

        // Assert
        Assert.False(result);
        Assert.True(status.CanKill);
        Assert.Equal(5.0f, status.Timer);
    }

    [Fact]
    public void EnableKillButton_WhenNotLocalPlayer_DoesNothing()
    {
        // Arrange
        var status = new FencerStatusModel(5.0f)
        {
            CanKill = false,
            Timer = 0.0f
        };
        var handler = new FencerAbilityHandler(status);

        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);

        // Act
        handler.EnableKillButton(2); // Local player is 1, target is 2

        // Assert
        Assert.False(status.CanKill);
        Assert.Equal(0.0f, status.Timer);
    }

    [Fact]
    public void EnableKillButton_WhenLocalPlayerMatches_UpdatesStatusAndPlayerState()
    {
        // Arrange
        var status = new FencerStatusModel(10.0f)
        {
            CanKill = false,
            Timer = 0.0f
        };
        var handler = new FencerAbilityHandler(status);

        byte playerId = 3;
        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

        // Act
        handler.EnableKillButton(playerId);

        // Assert
        Assert.True(status.CanKill);
        Assert.Equal(10.0f, status.Timer);
    }

    [Fact]
    public void EnableKillButton_WhenMapAndMinigameExist_ClosesMapAndForcesCloseMinigame()
    {
        // Arrange
        var status = new FencerStatusModel(10.0f);
        var handler = new FencerAbilityHandler(status);

        byte playerId = 3;
        var mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns(playerId);

        var mockMap = new Mock<MapBehaviour>(IntPtr.Zero);
        mockMap.Setup(m => m.Close());
        SetupMapBehaviourMock(mockMap.Object);

        var mockMinigame = new Mock<Minigame>(IntPtr.Zero);
        mockMinigame.Setup(m => m.ForceClose());
        SetupMinigameMock(mockMinigame.Object);

        // Act
        handler.EnableKillButton(playerId);

        // Assert
        mockMap.Verify(m => m.Close(), Times.Once);
        mockMinigame.Verify(m => m.ForceClose(), Times.Once);
        Assert.True(status.CanKill);
    }
}
