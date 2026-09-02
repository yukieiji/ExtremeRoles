using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using Xunit;

namespace ExtremeRoles.UnitTest.GameMode.Option.ShipGlobal;

[Collection(nameof(MockSetupHelper.SetupUnityCommonMocks))]
public class ShipGlobalOptionTests
{
    public ShipGlobalOptionTests()
    {
        MockSetupHelper.SetupUnityCommonMocks();
        MockSetupHelper.SetupExtremeSystemTypeManagerMock();
        MockSetupHelper.SetupAmongUsClientMock();
        MockSetupHelper.SetupLobbyMock();
        var plugin = MockSetupHelper.SetupMockExtremeRolePlugin();
        MockSetupHelper.SetupLogger();
        MockSetupHelper.SetupDebugMode();
        MockSetupHelper.SetupMockConfig(plugin);

        EnsureShipGlobalOptionsCreated();
    }

    private static void EnsureShipGlobalOptionsCreated()
    {
        if (ClientOption.Instance == null || !OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)OptionCreator.CommonOption.RandomOption, out _))
        {
            OptionCreator.Create();
        }
    }

    [Fact]
    public void IShipGlobalOption_Create_RegistersAllCategories()
    {
        EnsureShipGlobalOptionsCreated();

        foreach (ShipGlobalOptionCategory category in Enum.GetValues<ShipGlobalOptionCategory>())
        {
            bool exists = OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)category, out var cate);
            Assert.True(exists, $"Category {category} ({(int)category}) should be registered in OptionManager.");
            Assert.NotNull(cate);
        }
    }

    [Fact]
    public void IShipGlobalOption_WallCheckTask_ContainsExpectedTasks()
    {
        IShipGlobalOption option = new ClassicGameModeShipGlobalOption();
        var wallCheckTasks = option.WallCheckTask;

        Assert.Equal(4, wallCheckTasks.Count);
        Assert.Contains(TaskTypes.EmptyGarbage, wallCheckTasks);
        Assert.Contains(TaskTypes.FixShower, wallCheckTasks);
        Assert.Contains(TaskTypes.DevelopPhotos, wallCheckTasks);
        Assert.Contains(TaskTypes.DivertPower, wallCheckTasks);
    }

    [Fact]
    public void IShipGlobalOption_ChangeTask_ReturnsEnabledFixTasks()
    {
        IShipGlobalOption option = new ClassicGameModeShipGlobalOption();
        var taskCate = OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)ShipGlobalOptionCategory.TaskOption, out var cate) ? cate : null;
        Assert.NotNull(taskCate);

        // Turn all off
        for (int i = (int)TaskOption.GarbageTask; i <= (int)TaskOption.DivertPowerTask; ++i)
        {
            taskCate.Get(i).Selection = 0;
        }

        Assert.Empty(option.ChangeTask);

        // Turn on GarbageTask and ShowerTask
        taskCate.Get((int)TaskOption.GarbageTask).Selection = 1;
        taskCate.Get((int)TaskOption.ShowerTask).Selection = 1;

        var changeTasks = option.ChangeTask;
        Assert.Equal(2, changeTasks.Count);
        Assert.Contains(TaskTypes.EmptyGarbage, changeTasks);
        Assert.Contains(TaskTypes.FixShower, changeTasks);
        Assert.DoesNotContain(TaskTypes.DevelopPhotos, changeTasks);
        Assert.DoesNotContain(TaskTypes.DivertPower, changeTasks);

        // Reset to default
        taskCate.Get((int)TaskOption.GarbageTask).Selection = 0;
        taskCate.Get((int)TaskOption.ShowerTask).Selection = 0;
    }

    [Fact]
    public void ClassicGameModeShipGlobalOption_Properties_ReturnExpectedDefaults()
    {
        var classic = new ClassicGameModeShipGlobalOption();

        Assert.True(classic.IsEnableImpostorVent);
        Assert.True(classic.CanUseHorseMode);
        Assert.False(classic.IsBreakEmergencyButton);
    }

    [Fact]
    public void ClassicGameModeShipGlobalOption_Load_ReadsDefaultValuesFromOptionManager()
    {
        var classic = new ClassicGameModeShipGlobalOption();
        classic.Load();

        // GameStart
        Assert.True(classic.GameStart.IsKillCoolDownIsTen);
        Assert.True(classic.GameStart.RemoveSomeoneButton);
        Assert.Equal(1, classic.GameStart.ReduceNum);
        Assert.Equal(15, classic.GameStart.FirstButtonCoolDown);

        // Meeting
        Assert.Equal(10, classic.Meeting.MaxMeetingCount);
        Assert.False(classic.Meeting.UseRaiseHand);
        Assert.False(classic.Meeting.IsChangeVoteAreaButtonSortArg);
        Assert.False(classic.Meeting.IsFixedVoteAreaPlayerLevel);
        Assert.False(classic.Meeting.IsBlockSkipInMeeting);
        Assert.False(classic.Meeting.DisableSelfVote);
        Assert.False(classic.Meeting.OverruleSuccessIsNeutral);

        // Exile
        Assert.Equal(ConfirmExileMode.Impostor, classic.Exile.Mode);
        Assert.False(classic.Exile.IsConfirmRole);

        // Vent
        Assert.False(classic.Vent.Disable);
        Assert.False(classic.Vent.EngineerUseImpostorVent);
        Assert.False(classic.Vent.CanKillVentInPlayer);
        Assert.Equal(VentAnimationMode.VanillaAnimation, classic.Vent.AnimationMode);

        // Spawn
        Assert.True(classic.Spawn.EnableSpecialSetting);
        Assert.False(classic.Spawn.Skeld);
        Assert.False(classic.Spawn.MiraHq);
        Assert.False(classic.Spawn.Polus);
        Assert.True(classic.Spawn.AirShip);
        Assert.False(classic.Spawn.Fungle);
        Assert.False(classic.Spawn.IsAutoSelectRandom);

        // Map Modules
        Assert.False(classic.Admin.Disable);
        Assert.False(classic.Admin.EnableLimit);
        Assert.Equal(30.0f, classic.Admin.LimitTime);
        Assert.Equal(AirShipAdminMode.ModeBoth, classic.Admin.AirShipEnable);

        Assert.False(classic.Vital.Disable);
        Assert.False(classic.Vital.EnableLimit);
        Assert.Equal(30.0f, classic.Vital.LimitTime);
        Assert.Equal(PolusVitalPos.DefaultKey, classic.Vital.PolusPos);

        Assert.False(classic.Security.Disable);
        Assert.False(classic.Security.EnableLimit);
        Assert.Equal(30.0f, classic.Security.LimitTime);

        // Task & Neutral Win
        Assert.False(classic.DisableTaskWinWhenNoneTaskCrew);
        Assert.False(classic.DisableTaskWin);
        Assert.True(classic.IsSameNeutralSameWin);
        Assert.False(classic.DisableNeutralSpecialForceEnd);

        // Ghost Role
        Assert.Equal(4.0f, classic.GhostRole.HauntMinigameMaxSpeed);
        Assert.True(classic.GhostRole.IsAssignNeutralToVanillaCrewGhostRole);
        Assert.False(classic.GhostRole.IsBlockGAAbilityReport);

        // Task & Map Options
        Assert.False(classic.ChangeForceWallCheck);
        Assert.False(classic.IsAllowParallelMedbayScan);
        Assert.False(classic.IsRandomMap);
    }

    [Fact]
    public void ClassicGameModeShipGlobalOption_Load_ReadsUpdatedValues()
    {
        var classic = new ClassicGameModeShipGlobalOption();

        Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)ShipGlobalOptionCategory.TaskOption, out var taskCate));
        Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)ShipGlobalOptionCategory.RandomMapOption, out var randomMapCate));
        Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)ShipGlobalOptionCategory.TaskWinOption, out var taskWinCate));
        Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)ShipGlobalOptionCategory.NeutralWinOption, out var neutralWinCate));

        taskCate.Get((int)TaskOption.ParallelMedBayScans).Selection = 1;
        taskCate.Get((int)TaskOption.IsFixWallHaskTask).Selection = 1;
        randomMapCate.Get((int)RandomMap.Enable).Selection = 1;
        taskWinCate.Get((int)TaskWinOption.DisableWhenNoneTaskCrew).Selection = 1;
        taskWinCate.Get((int)TaskWinOption.DisableAll).Selection = 1;
        neutralWinCate.Get((int)NeutralWinOption.IsSame).Selection = 0;
        neutralWinCate.Get((int)NeutralWinOption.DisableSpecialEnd).Selection = 1;

        try
        {
            classic.Load();

            Assert.True(classic.IsAllowParallelMedbayScan);
            Assert.True(classic.ChangeForceWallCheck);
            Assert.True(classic.IsRandomMap);
            Assert.True(classic.DisableTaskWinWhenNoneTaskCrew);
            Assert.True(classic.DisableTaskWin);
            Assert.False(classic.IsSameNeutralSameWin);
            Assert.True(classic.DisableNeutralSpecialForceEnd);
        }
        finally
        {
            // Reset
            taskCate.Get((int)TaskOption.ParallelMedBayScans).Selection = 0;
            taskCate.Get((int)TaskOption.IsFixWallHaskTask).Selection = 0;
            randomMapCate.Get((int)RandomMap.Enable).Selection = 0;
            taskWinCate.Get((int)TaskWinOption.DisableWhenNoneTaskCrew).Selection = 0;
            taskWinCate.Get((int)TaskWinOption.DisableAll).Selection = 0;
            neutralWinCate.Get((int)NeutralWinOption.IsSame).Selection = 1;
            neutralWinCate.Get((int)NeutralWinOption.DisableSpecialEnd).Selection = 0;
        }
    }

    [Fact]
    public void ClassicGameModeShipGlobalOption_Emergency_LazyInitialization()
    {
        var classic = new ClassicGameModeShipGlobalOption();

        var emergency1 = classic.Emergency;
        Assert.NotNull(emergency1);

        var emergency2 = classic.Emergency;
        Assert.Same(emergency1, emergency2);
    }

    [Fact]
    public void ClassicGameModeShipGlobalOption_TryGetInvalidOption_ReturnsTrueAndAllEnable()
    {
        var classic = new ClassicGameModeShipGlobalOption();

        bool result = classic.TryGetInvalidOption((int)ShipGlobalOptionCategory.VentOption, out var useOptionId);

        Assert.True(result);
        Assert.Equal(OptionSplitter.AllEnable, useOptionId);
    }

    [Fact]
    public void HideNSeekModeShipGlobalOption_Properties_ReturnExpectedDefaults()
    {
        var hns = new HideNSeekModeShipGlobalOption();

        Assert.True(hns.CanUseHorseMode);
        Assert.False(hns.IsEnableImpostorVent);
        Assert.Equal(ConfirmExileMode.Impostor, hns.ExilMode);
        Assert.False(hns.IsConfirmRole);
        Assert.False(hns.DisableTaskWinWhenNoneTaskCrew);
        Assert.False(hns.DisableTaskWin);
        Assert.True(hns.IsBreakEmergencyButton);
    }

    [Fact]
    public void HideNSeekModeShipGlobalOption_Load_ReadsValuesFromOptionManager()
    {
        var hns = new HideNSeekModeShipGlobalOption();

        Assert.True(OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)ShipGlobalOptionCategory.VentOption, out var ventCate));

        ventCate.Get((int)VentOption.Disable).Selection = 1;
        ventCate.Get((int)VentOption.AnimationModeInVison).Selection = (int)VentAnimationMode.DonotWallHack;

        try
        {
            hns.Load();

            Assert.True(hns.Vent.Disable);
            Assert.False(hns.Vent.EngineerUseImpostorVent);
            Assert.False(hns.Vent.CanKillVentInPlayer);
            Assert.Equal(VentAnimationMode.DonotWallHack, hns.Vent.AnimationMode);
        }
        finally
        {
            ventCate.Get((int)VentOption.Disable).Selection = 0;
            ventCate.Get((int)VentOption.AnimationModeInVison).Selection = (int)VentAnimationMode.VanillaAnimation;
        }
    }

    [Fact]
    public void HideNSeekModeShipGlobalOption_Emergency_LazyInitialization()
    {
        var hns = new HideNSeekModeShipGlobalOption();

        var emergency1 = hns.Emergency;
        Assert.NotNull(emergency1);

        var emergency2 = hns.Emergency;
        Assert.Same(emergency1, emergency2);
    }

    [Fact]
    public void HideNSeekModeShipGlobalOption_TryGetInvalidOption_FiltersCategories()
    {
        var hns = new HideNSeekModeShipGlobalOption();

        // VentOption should return true with only Disable and AnimationModeInVison
        bool ventResult = hns.TryGetInvalidOption((int)ShipGlobalOptionCategory.VentOption, out var ventOptions);
        Assert.True(ventResult);
        Assert.NotNull(ventOptions);
        Assert.Contains((int)VentOption.Disable, ventOptions);
        Assert.Contains((int)VentOption.AnimationModeInVison, ventOptions);
        Assert.DoesNotContain((int)VentOption.EngineerUseImpostor, ventOptions);
        Assert.DoesNotContain((int)VentOption.CanKillInPlayer, ventOptions);

        // RandomSpawnOption should return true with AllEnable
        bool spawnResult = hns.TryGetInvalidOption((int)ShipGlobalOptionCategory.RandomSpawnOption, out var spawnOptions);
        Assert.True(spawnResult);
        Assert.Equal(OptionSplitter.AllEnable, spawnOptions);

        // Unregistered category in useOption dictionary should return false
        bool meetingResult = hns.TryGetInvalidOption((int)ShipGlobalOptionCategory.MeetingOption, out var meetingOptions);
        Assert.False(meetingResult);
        Assert.Null(meetingOptions);
    }

    [Fact]
    public void SubOptions_DefaultAndConstructorInits_WorkAsExpected()
    {
        // GameStartOption
        var defaultGameStart = new GameStartOption();
        Assert.True(defaultGameStart.IsKillCoolDownIsTen);
        Assert.True(defaultGameStart.RemoveSomeoneButton);
        Assert.Equal(1, defaultGameStart.ReduceNum);
        Assert.Equal(15, defaultGameStart.FirstButtonCoolDown);

        // MeetingHudOption
        var defaultMeeting = new MeetingHudOption();
        Assert.Equal(0, defaultMeeting.MaxMeetingCount);
        Assert.False(defaultMeeting.UseRaiseHand);

        // VentConsoleOption direct constructor
        var ventDirect = new VentConsoleOption(true, true, true, VentAnimationMode.DonotOutVison);
        Assert.True(ventDirect.Disable);
        Assert.True(ventDirect.EngineerUseImpostorVent);
        Assert.True(ventDirect.CanKillVentInPlayer);
        Assert.Equal(VentAnimationMode.DonotOutVison, ventDirect.AnimationMode);

        // MapModuleDisableFlag record struct
        var flag = new MapModuleDisableFlag(true, false, true, AirShipAdminMode.ModeCockpitOnly);
        Assert.True(flag.Admin);
        Assert.False(flag.Security);
        Assert.True(flag.Vital);
        Assert.Equal(AirShipAdminMode.ModeCockpitOnly, flag.AirShipAdminMode);
    }
}
