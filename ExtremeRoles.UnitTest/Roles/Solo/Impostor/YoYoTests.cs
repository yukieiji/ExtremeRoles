using System;
using AmongUs.GameOptions;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Moq;
using UnityEngine;
using Xunit;

namespace ExtremeRoles.UnitTest.Roles.Solo.Impostor;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public sealed class YoYoTests
{
    private readonly Mock<AmongUsClient> mockClient;
    private readonly Mock<PlayerControl> mockLocalPlayer;

    public YoYoTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupMockConfig(plugin);

        SetupLobbyBehaviourMock();
        SetupTranslationControllerMock();

        mockClient = MockSetupHelper.SetupAmongUsClientMock();
        var mockWriter = new Mock<MessageWriter>(IntPtr.Zero);
        mockClient.Setup(c => c.StartRpcImmediately(It.IsAny<uint>(), It.IsAny<byte>(), It.IsAny<SendOption>(), It.IsAny<int>()))
            .Returns(mockWriter.Object);

        mockLocalPlayer = MockSetupHelper.SetupPlayerControlMocks();
        mockLocalPlayer.SetupGet(p => p.PlayerId).Returns((byte)1);
        mockLocalPlayer.Setup(p => p.GetTruePosition()).Returns(new Vector2(10f, 20f));

        MockSetupHelper.SetupGameDataMock();
        MockSetupHelper.SetupGameOptionsManagerMock();
        MockSetupHelper.SetupOptionManager();

        if (ExtremeRolesPlugin.ShipState == null)
        {
            var shipStateProp = typeof(ExtremeRolesPlugin).GetProperty(
                nameof(ExtremeRolesPlugin.ShipState),
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            shipStateProp?.SetValue(null, new ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus());
        }
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

    private static void InitializeRole(YoYo role, byte playerId = 1)
    {
        role.CreateRoleAllOption();
        role.Initialize();

        ExtremeRoleManager.GameRole[playerId] = role;
    }

    [Fact]
    public void Initialize_RegistersRoleAndImpostorTeam()
    {
        var role = new YoYo();
        InitializeRole(role);

        Assert.Equal(ExtremeRoleId.YoYo, role.Core.Id);
        Assert.True(role.IsImpostor());
        Assert.Null(role.SavedPosition);
    }

    [Fact]
    public void SetMarkAndClearMark_UpdatesSavedPosition()
    {
        var role = new YoYo();
        InitializeRole(role);

        Vector2 markPos = new Vector2(5f, -15f);
        role.SetMark(markPos, isCreateMarker: false);

        Assert.NotNull(role.SavedPosition);
        Assert.Equal(markPos, role.SavedPosition!.Value);

        role.ClearMark();
        Assert.Null(role.SavedPosition);
    }

    [Fact]
    public void UseAbility_SavesCurrentPosition_AndWhenSavedPositionExists_TeleportsAndClearsMark()
    {
        var role = new YoYo();
        InitializeRole(role);

        bool saveResult = role.UseAbilityInternal(isCreateMarker: false);
        Assert.True(saveResult);
        Assert.NotNull(role.SavedPosition);
        Assert.Equal(new Vector2(10f, 20f), role.SavedPosition!.Value);

        bool teleportResult = role.UseAbilityInternal(isCreateMarker: false);
        Assert.True(teleportResult);
        Assert.Null(role.SavedPosition);
    }

    [Fact]
    public void ResetOnMeetingStart_WhenOptionIsEnabled_ClearsSavedPosition()
    {
        var role = new YoYo();
        InitializeRole(role);

        role.SetMark(new Vector2(12f, 34f), isCreateMarker: false);
        Assert.NotNull(role.SavedPosition);

        role.ResetOnMeetingStart();
        Assert.Null(role.SavedPosition);
    }

    [Fact]
    public void Behavior_TryUseAbility_WhenIsReduceAbilityCountFalse_DoesNotReduceCount()
    {
        bool abilityExecuted = false;
        var behavior = new YoYo.YoYoAbilityBehavior(
            "Test", null!,
            () => true,
            () => { abilityExecuted = true; return true; });

        behavior.SetAbilityCount(3);
        behavior.IsReduceAbilityCount = false;

        bool result = behavior.TryUseAbility(0f, AbilityState.Ready, out var newState);

        Assert.True(result);
        Assert.True(abilityExecuted);
        Assert.Equal(3, behavior.AbilityCount);
        Assert.Equal(AbilityState.CoolDown, newState);
    }

    [Fact]
    public void Behavior_TryUseAbility_WhenIsReduceAbilityCountTrue_ReducesCount()
    {
        bool abilityExecuted = false;
        var behavior = new YoYo.YoYoAbilityBehavior(
            "Test", null!,
            () => true,
            () => { abilityExecuted = true; return true; });

        behavior.SetAbilityCount(3);
        behavior.IsReduceAbilityCount = true;

        bool result = behavior.TryUseAbility(0f, AbilityState.Ready, out var newState);

        Assert.True(result);
        Assert.True(abilityExecuted);
        Assert.Equal(2, behavior.AbilityCount);
        Assert.False(behavior.IsReduceAbilityCount);
        Assert.Equal(AbilityState.CoolDown, newState);
    }

    [Fact]
    public void Behavior_Update_ReturnsExpectedState()
    {
        var behavior = new YoYo.YoYoAbilityBehavior(
            "Test", null!,
            () => true,
            () => true);

        behavior.SetAbilityCount(1);
        // Updating after SetAbilityCount resolves one-time isUpdate flag to CoolDown
        Assert.Equal(AbilityState.CoolDown, behavior.Update(AbilityState.Ready));
        // Subsequent update returns curState when count > 0
        Assert.Equal(AbilityState.Ready, behavior.Update(AbilityState.Ready));

        behavior.SetAbilityCount(0);
        behavior.Update(AbilityState.Ready); // consume isUpdate flag
        Assert.Equal(AbilityState.None, behavior.Update(AbilityState.Ready));
    }
}
