using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;

#nullable enable

namespace ExtremeRoles.Roles.Combination.Avalon;

public sealed class AvalonRole : ConstCombinationRoleManagerBase
{
    public const string Name = "AvalonsRoles";
    public AvalonRole() : base(
		CombinationRoleType.Avalon,
        Name, DefaultColor, 2,
        GameSystem.MaxImposterNum)
    {
        Roles.Add(new Assassin());
        Roles.Add(new Marlin());
    }
}

public sealed class Assassin : MultiAssignRoleBase
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

    private AssassinStatusModel? status;
    private bool isDeadForceMeeting = true;
    public override IStatusModel? Status => status;

    public Assassin(
        ) : base(
			RoleCore.BuildImpostor(ExtremeRoleId.Assassin),
            true, false, true, true,
            tab: OptionTab.CombinationTab)
    {
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
		var killOptActive = new ParentActive(killedOps);

        factory.CreateBoolOption(
            AssassinOption.CanKilledFromCrew,
            false, killOptActive);
        factory.CreateBoolOption(
            AssassinOption.CanKilledFromNeutral,
            false, killOptActive);
        var meetingOpt = factory.CreateBoolOption(
            AssassinOption.IsDeadForceMeeting,
            true, killOptActive);
        factory.CreateBoolOption(
            AssassinOption.CanSeeRoleBeforeFirstMeeting,
            false, new ParentActive(meetingOpt));

        factory.CreateBoolOption(
             AssassinOption.CanSeeVote,
            true);
    }

    public override void ExiledAction(
        PlayerControl rolePlayer)
    {

        if (isServant())
		{
			return;
		}

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
		this.status = new AssassinStatusModel(
            loader.GetValue<AssassinOption, bool>(AssassinOption.CanKilled),
            loader.GetValue<AssassinOption, bool>(AssassinOption.CanKilledFromCrew),
            loader.GetValue<AssassinOption, bool>(AssassinOption.CanKilledFromNeutral)
        );
		this.AbilityClass = new AssassinAbilityHandler(status);

		this.HasTask = loader.GetValue<AssassinOption, bool>(
            AssassinOption.HasTask);
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

    private bool isServant() => this.AnotherRole?.Core.Id == ExtremeRoleId.Servant;
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
			RoleCore.BuildCrewmate(
				ExtremeRoleId.Marlin,
				ColorPalette.MarineBlue),
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
        updateShowIcon();
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        updateShowIcon();
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
        if (targetRole.Core.Id == ExtremeRoleId.Assassin && !this.canSeeAssassin)
        {
            return Palette.White;
        }
        else if (targetRole.IsImpostor())
        {
            return Palette.ImpostorRed;
        }
        else if (targetRole.IsNeutral() && CanSeeNeutral)
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
            if (playerId == PlayerControl.LocalPlayer.PlayerId)
            {
                continue;
            }

            if (!ExtremeRoleManager.TryGetRole(playerId, out var role))
            {
                continue;
            }

            if (role.IsCrewmate() ||
                (role.IsNeutral() && !this.CanSeeNeutral) ||
                (role.Core.Id == ExtremeRoleId.Assassin && !this.canSeeAssassin))
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
