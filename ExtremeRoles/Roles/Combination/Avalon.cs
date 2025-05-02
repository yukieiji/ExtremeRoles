﻿using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Performance;


using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

#nullable enable

namespace ExtremeRoles.Roles.Combination;

public sealed class Avalon : ConstCombinationRoleManagerBase
{
    public const string Name = "AvalonsRoles";
    public Avalon() : base(
		CombinationRoleType.Avalon,
        Name, DefaultColor, 2,
        GameSystem.MaxImposterNum)
    {
        this.Roles.Add(new Assassin());
        this.Roles.Add(new Marlin());
    }
}

public sealed class Assassin : MultiAssignRoleBase, IKilledFrom
{
    public enum AssassinOption
    {
        HasTask,
        CanKilled,
        CanKilledFromCrew,
        CanKilledFromNeutral,
        IsDeadForceMeeting,
        CanSeeRoleBeforeFirstMeeting,
        CanSeeVote,
    }

    public bool IsFirstMeeting = false;
    public bool CanSeeRoleBeforeFirstMeeting = false;
    public bool CanSeeVote = false;

    public bool CanKilled { get; private set; } = false;
    public bool CanKilledFromCrew { get; private set; } = false;
    public bool CanKilledFromNeutral { get; private set; } = false;
    private bool isDeadForceMeeting = true;

    public Assassin(
        ) : base(
            ExtremeRoleId.Assassin,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Assassin.ToString(),
            Palette.ImpostorRed,
            true, false, true, true,
            tab: OptionTab.CombinationTab)
    {}

    public bool TryKilledFrom(
        PlayerControl rolePlayer, PlayerControl fromPlayer)
    {
        if (!this.CanKilled) { return false; }

        var fromPlayerRole = ExtremeRoleManager.GameRole[fromPlayer.PlayerId];

        if (fromPlayerRole.IsNeutral())
        {
            return this.CanKilledFromNeutral;
        }
        else if (fromPlayerRole.IsCrewmate())
        {
            return this.CanKilledFromCrew;
        }

        return false;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateBoolOption(
            AssassinOption.HasTask,
            false);
        var killedOps = factory.CreateBoolOption(
            AssassinOption.CanKilled,
            false);
        factory.CreateBoolOption(
            AssassinOption.CanKilledFromCrew,
            false, killedOps);
        factory.CreateBoolOption(
            AssassinOption.CanKilledFromNeutral,
            false, killedOps);
        var meetingOpt = factory.CreateBoolOption(
            AssassinOption.IsDeadForceMeeting,
            true, killedOps);
        factory.CreateBoolOption(
            AssassinOption.CanSeeRoleBeforeFirstMeeting,
            false, meetingOpt);

        factory.CreateBoolOption(
             AssassinOption.CanSeeVote,
            true);
    }

    public override void ExiledAction(
        PlayerControl rolePlayer)
    {

        if (isServant()) { return; }

		this.IsFirstMeeting = false;
		assassinMeetingTriggerOn(rolePlayer);
	}

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer,
        PlayerControl killerPlayer)
    {

        if (isServant()) { return; }

		byte rolePlayerId = rolePlayer.PlayerId;

        if (!this.isDeadForceMeeting || MeetingHud.Instance != null)
        {
            addDead(rolePlayerId);
            return;
        }

        this.IsFirstMeeting = false;

		assassinMeetingTriggerOn(rolePlayer, killerPlayer);
	}

    public override bool IsBlockShowPlayingRoleInfo()
    {
        return !this.IsFirstMeeting && !this.CanSeeRoleBeforeFirstMeeting;
    }

    public override bool IsBlockShowMeetingRoleInfo()
    {
        if (OnemanMeetingSystemManager.TryGetActiveSystem(out var system) &&
			system.IsActiveMeeting<AssassinAssassinateTargetMeeting>())
        {
            return true;
        }
        else if (this.CanSeeRoleBeforeFirstMeeting)
        {
            return this.IsFirstMeeting;
        }

        return false;

    }
    protected override void RoleSpecificInit()
    {
        var loader = this.Loader;

        this.HasTask = loader.GetValue<AssassinOption, bool>(
            AssassinOption.HasTask);
        this.CanKilled = loader.GetValue<AssassinOption, bool>(
            AssassinOption.CanKilled);
        this.CanKilledFromCrew = loader.GetValue<AssassinOption, bool>(
            AssassinOption.CanKilledFromCrew);
        this.CanKilledFromNeutral = loader.GetValue<AssassinOption, bool>(
            AssassinOption.CanKilledFromNeutral);
        this.CanSeeVote = loader.GetValue<AssassinOption, bool>(
            AssassinOption.CanSeeVote);

        this.isDeadForceMeeting = loader.GetValue<AssassinOption, bool>(
            AssassinOption.IsDeadForceMeeting);
        this.CanSeeRoleBeforeFirstMeeting = loader.GetValue<AssassinOption, bool>(
            AssassinOption.CanSeeRoleBeforeFirstMeeting);
        this.IsFirstMeeting = true;
		_ = OnemanMeetingSystemManager.CreateOrGet();
    }

    private void assassinMeetingTriggerOn(PlayerControl caller, PlayerControl? reporter = null)
    {
		if (!OnemanMeetingSystemManager.TryGetSystem(out var system))
		{
			return;
		}
		system.Start(caller, OnemanMeetingSystemManager.Type.Assassin, reporter);
	}

    public static void addDead(byte playerId)
    {
		if (!OnemanMeetingSystemManager.TryGetSystem(out var system))
		{
			return;
		}
		system.AddQueue(playerId, OnemanMeetingSystemManager.Type.Assassin);
	}

    private bool isServant() => this.AnotherRole?.Id == ExtremeRoleId.Servant;
}


public sealed class Marlin : MultiAssignRoleBase, IRoleSpecialSetUp, IRoleResetMeeting
{
    public enum MarlinOption
    {
        HasTask,
        CanSeeAssassin,
        CanSeeVote,
        CanSeeNeutral,
        CanUseVent,
    }

    public bool IsAssassinate = false;
    public bool CanSeeVote = false;
    public bool CanSeeNeutral = false;
    private bool canSeeAssassin = false;
    private GridArrange? grid;

    private Dictionary<byte, PoolablePlayer> PlayerIcon = [];
    public Marlin(
        ) : base(
            ExtremeRoleId.Marlin,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Marlin.ToString(),
            ColorPalette.MarineBlue,
            false, false, false, false,
            tab: OptionTab.CombinationTab)
    {}

    public void IntroBeginSetUp()
    {
        return;
    }

    public void IntroEndSetUp()
    {
        GameObject bottomLeft = new GameObject("BottomLeft");
        bottomLeft.transform.SetParent(
            HudManager.Instance.UseButton.transform.parent.parent);
        AspectPosition aspectPosition = bottomLeft.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        aspectPosition.anchorPoint = new Vector2(0.5f, 0.5f);
        aspectPosition.DistanceFromEdge = new Vector3(0.375f, 0.35f);
        aspectPosition.AdjustPosition();

        this.grid = bottomLeft.AddComponent<GridArrange>();
        this.grid.CellSize = new Vector2(0.625f, 0.75f);
        this.grid.MaxColumns = 10;
        this.grid.Alignment = GridArrange.StartAlign.Right;
        this.grid.cells = new();

        this.PlayerIcon = Player.CreatePlayerIcon(
            bottomLeft.transform, Vector3.one * 0.275f);
        this.updateShowIcon();
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        this.updateShowIcon();
    }

    public void ResetOnMeetingStart()
    {
        foreach (var (_, poolPlayer) in this.PlayerIcon)
        {
            poolPlayer.gameObject.SetActive(false);
        }
    }

    public override Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole,
        byte targetPlayerId)
    {
        if (targetRole.Id == ExtremeRoleId.Assassin && !this.canSeeAssassin)
        {
            return Palette.White;
        }
        else if (targetRole.IsImpostor())
        {
            return Palette.ImpostorRed;
        }
        else if (targetRole.IsNeutral() && this.CanSeeNeutral)
        {
            return ColorPalette.NeutralColor;
        }

        return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
    }


    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateBoolOption(
            MarlinOption.HasTask,
            false);

        factory.CreateBoolOption(
            MarlinOption.CanSeeAssassin,
            true);

        factory.CreateBoolOption(
            MarlinOption.CanSeeVote,
            true);
        factory.CreateBoolOption(
            MarlinOption.CanSeeNeutral,
            false);
        factory.CreateBoolOption(
            MarlinOption.CanUseVent,
            false);
    }

    protected override void RoleSpecificInit()
    {
        this.IsAssassinate = false;

        var loader = this.Loader;

        this.HasTask = loader.GetValue<MarlinOption, bool>(
            MarlinOption.HasTask);
        this.canSeeAssassin = loader.GetValue<MarlinOption, bool>(
            MarlinOption.CanSeeAssassin);
        this.CanSeeVote = loader.GetValue<MarlinOption, bool>(
            MarlinOption.CanSeeVote);
        this.CanSeeNeutral = loader.GetValue<MarlinOption, bool>(
            MarlinOption.CanSeeNeutral);
        this.UseVent = loader.GetValue<MarlinOption, bool>(
            MarlinOption.CanUseVent);
        this.PlayerIcon = new Dictionary<byte, PoolablePlayer>();
    }

    private void updateShowIcon()
    {
        foreach (var(playerId, poolPlayer) in this.PlayerIcon)
        {
            if (playerId == PlayerControl.LocalPlayer.PlayerId) { continue; }

            SingleRoleBase role = ExtremeRoleManager.GameRole[playerId];
            if (role.IsCrewmate() ||
                (role.IsNeutral() && !this.CanSeeNeutral) ||
                (role.Id == ExtremeRoleId.Assassin && !this.canSeeAssassin))
            {
                poolPlayer.gameObject.SetActive(false);
            }
            else
            {
                poolPlayer.transform.localScale = Vector3.one * 0.275f;
                poolPlayer.gameObject.SetActive(true);
            }
        }
		if (this.grid == null)
		{
			return;
		}
        this.grid.ArrangeChilds();
    }
}
