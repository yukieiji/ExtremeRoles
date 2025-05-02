﻿using UnityEngine;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;

using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Madmate :
    SingleRoleBase,
    IRoleAutoBuildAbility,
    IRoleUpdate,
    IRoleSpecialSetUp,
    IRoleWinPlayerModifier,
	IRoleFakeIntro
{
    public enum MadmateOption
    {
        IsDontCountAliveCrew,
        CanFixSabotage,
        CanUseVent,
        CanMoveVentToVent,
        HasTask,
        SeeImpostorTaskGage,
        CanSeeFromImpostor,
        CanSeeFromImpostorTaskGage,
    }

    private bool canMoveVentToVent = false;
    private bool canSeeFromImpostor = false;
    private bool isDontCountAliveCrew = false;

    private bool isSeeImpostorNow = false;
    private bool isUpdateMadmate = false;
    private float seeImpostorTaskGage;
    private float seeFromImpostorTaskGage;

    public ExtremeAbilityButton Button
    {
        get => this.madmateAbilityButton;
        set
        {
            this.madmateAbilityButton = value;
        }
    }

    public bool IsDontCountAliveCrew => this.isDontCountAliveCrew;

	public ExtremeRoleType FakeTeam =>
		this.isUpdateMadmate ? ExtremeRoleType.Impostor : ExtremeRoleType.Crewmate;

	private ExtremeAbilityButton madmateAbilityButton;

    public Madmate() : base(
        ExtremeRoleId.Madmate,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Madmate.ToString(),
        Palette.ImpostorRed,
        false, false, false, false)
    { }

    public static void ToFakeImpostor(byte playerId)
    {

        Madmate madmate = ExtremeRoleManager.GetSafeCastedRole<Madmate>(playerId);
        if (madmate == null) { return; }

        madmate.FakeImposter = true;
    }

    public void CreateAbility()
    {
        this.CreateNormalAbilityButton(
            "selfKill", Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.SucideSprite));
    }

    public bool UseAbility()
    {

        byte playerId = PlayerControl.LocalPlayer.PlayerId;

        Helper.Player.RpcUncheckMurderPlayer(
            playerId, playerId, byte.MaxValue);
        return true;
    }

    public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void IntroBeginSetUp()
    {
        return;
    }

    public void IntroEndSetUp()
    {
        if (!this.UseVent || this.canMoveVentToVent) { return; }

        // 全てのベントリンクを解除
        foreach (Vent vent in ShipStatus.Instance.AllVents)
        {
            vent.Right = null;
            vent.Center = null;
            vent.Left = null;
        }
    }

    public void ModifiedWinPlayer(
        NetworkedPlayerInfo rolePlayerInfo,
        GameOverReason reason,
		in WinnerTempData winner)
    {
        switch (reason)
        {
            case GameOverReason.ImpostorsByVote:
            case GameOverReason.ImpostorsByKill:
            case GameOverReason.ImpostorsBySabotage:
            case GameOverReason.ImpostorDisconnect:
            case GameOverReason.HideAndSeek_ImpostorsByKills:
            case (GameOverReason)RoleGameOverReason.AssassinationMarin:
			case (GameOverReason)RoleGameOverReason.TeroristoTeroWithShip:
				winner.AddWithPlus(rolePlayerInfo);
				break;
            default:
                break;
        }
    }

    public void Update(PlayerControl rolePlayer)
    {
        if (!this.HasTask) { return; }

        float taskGage = Helper.Player.GetPlayerTaskGage(rolePlayer);
        if (taskGage >= this.seeImpostorTaskGage && !isSeeImpostorNow)
        {
            this.isSeeImpostorNow = true;
        }
        if (this.canSeeFromImpostor &&
            taskGage >= this.seeFromImpostorTaskGage &&
            !this.isUpdateMadmate)
        {
            this.isUpdateMadmate = true;

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.MadmateToFakeImpostor))
            {
                caller.WriteByte(rolePlayer.PlayerId);
            }
            ToFakeImpostor(rolePlayer.PlayerId);
        }
    }

    public override Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole, byte targetPlayerId)
    {
        if (this.isSeeImpostorNow &&
            (targetRole.IsImpostor() || targetRole.FakeImposter))
        {
            return Palette.ImpostorRed;
        }

        return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateBoolOption(
            MadmateOption.IsDontCountAliveCrew,
            false);
        factory.CreateBoolOption(
            MadmateOption.CanFixSabotage,
            false);
        var ventUseOpt = factory.CreateBoolOption(
            MadmateOption.CanUseVent,
            false);
        factory.CreateBoolOption(
            MadmateOption.CanMoveVentToVent,
            false, ventUseOpt);
        var taskOpt = factory.CreateBoolOption(
            MadmateOption.HasTask,
            false);
        factory.CreateIntOption(
            MadmateOption.SeeImpostorTaskGage,
            70, 0, 100, 10,
            taskOpt,
            format: OptionUnit.Percentage);
        var impFromSeeOpt = factory.CreateBoolOption(
            MadmateOption.CanSeeFromImpostor,
            false, taskOpt);
        factory.CreateIntOption(
            MadmateOption.CanSeeFromImpostorTaskGage,
            70, 0, 100, 10,
            impFromSeeOpt,
            format: OptionUnit.Percentage);

        IRoleAbility.CreateCommonAbilityOption(factory);
    }

    protected override void RoleSpecificInit()
    {
        var cate = this.Loader;
        this.isSeeImpostorNow = false;
        this.isUpdateMadmate = false;
        this.FakeImposter = false;

        this.isDontCountAliveCrew = cate.GetValue<MadmateOption, bool>(
            MadmateOption.IsDontCountAliveCrew);

        this.CanRepairSabotage = cate.GetValue<MadmateOption, bool>(
            MadmateOption.CanFixSabotage);
        this.UseVent = cate.GetValue<MadmateOption, bool>(
            MadmateOption.CanUseVent);
        this.canMoveVentToVent = cate.GetValue<MadmateOption, bool>(
            MadmateOption.CanMoveVentToVent);
        this.HasTask = cate.GetValue<MadmateOption, bool>(
            MadmateOption.HasTask);
        this.seeImpostorTaskGage = cate.GetValue<MadmateOption, int>(
            MadmateOption.SeeImpostorTaskGage) / 100.0f;
        this.canSeeFromImpostor = cate.GetValue<MadmateOption, bool>(
            MadmateOption.CanSeeFromImpostor);
        this.seeFromImpostorTaskGage = cate.GetValue<MadmateOption, int>(
            MadmateOption.CanSeeFromImpostorTaskGage) / 100.0f;

        this.isSeeImpostorNow =
            this.HasTask &&
            this.seeImpostorTaskGage <= 0.0f;
        this.isUpdateMadmate =
            this.HasTask &&
            this.canSeeFromImpostor &&
            this.seeFromImpostorTaskGage <= 0.0f;

        this.FakeImposter = this.isUpdateMadmate;
    }
}
