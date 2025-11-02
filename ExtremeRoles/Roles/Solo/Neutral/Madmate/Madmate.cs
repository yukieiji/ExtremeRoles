using UnityEngine;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Module.CustomOption.Factory;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Madmate;

public sealed class MadmateRole :
    SingleRoleBase,
    IRoleAutoBuildAbility,
    IRoleUpdate,
    IRoleWinPlayerModifier
{
    public enum MadmateOption
    {
        IsDontCountAliveCrew,
        CanFixSabotage,
        CanUseVent,
        HasTask,
        SeeImpostorTaskGage,
        CanSeeFromImpostor,
        CanSeeFromImpostorTaskGage,
    }

    private bool canSeeFromImpostor = false;
    private bool isDontCountAliveCrew = false;

    private bool isSeeImpostorNow = false;
    private float seeImpostorTaskGage;
    private float seeFromImpostorTaskGage;

    public ExtremeAbilityButton? Button { get; set; }

    public bool IsDontCountAliveCrew => isDontCountAliveCrew;


	public override IStatusModel? Status => status;
	private MadmateStatus? status;

    public MadmateRole() : base(
		RoleCore.BuildNeutral(
			ExtremeRoleId.Madmate,
			Palette.ImpostorRed),
        false, false, false, false)
    { }

    public static void ToFakeImpostor(byte playerId)
    {

        MadmateRole? madmate = ExtremeRoleManager.GetSafeCastedRole<MadmateRole>(playerId);
        if (madmate is null ||
			madmate.status is null)
		{
			return;
		}

		madmate.status.IsFakeImpostor = true;
    }

    public void CreateAbility()
    {
        this.CreateNormalAbilityButton(
            "selfKill", UnityObjectLoader.LoadSpriteFromResources(
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

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
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
        if (!this.HasTask)
		{
			return;
		}

        float taskGage = Helper.Player.GetPlayerTaskGage(rolePlayer);
        if (taskGage >= seeImpostorTaskGage && !isSeeImpostorNow)
        {
			isSeeImpostorNow = true;
        }
        if (canSeeFromImpostor &&
            taskGage >= seeFromImpostorTaskGage &&
			this.status is not null &&
			!this.status.IsFakeImpostor)
        {
			this.status.IsFakeImpostor = true;

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
            (targetRole.IsImpostor() || (targetRole.Status is IFakeImpostorStatus status && status.IsFakeImpostor)))
        {
            return Palette.ImpostorRed;
        }

        return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateNewBoolOption(
            MadmateOption.IsDontCountAliveCrew,
            false);
        factory.CreateNewBoolOption(
            MadmateOption.CanFixSabotage,
            false);
        var ventUseOpt = factory.CreateNewBoolOption(
            MadmateOption.CanUseVent,
            false);
        var taskOpt = factory.CreateNewBoolOption(
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
        var cate = Loader;
		this.isSeeImpostorNow = false;
		this.status = new MadmateStatus();

		this.isDontCountAliveCrew = cate.GetValue<MadmateOption, bool>(
            MadmateOption.IsDontCountAliveCrew);

		this.CanRepairSabotage = cate.GetValue<MadmateOption, bool>(
            MadmateOption.CanFixSabotage);
		this.UseVent = cate.GetValue<MadmateOption, bool>(
            MadmateOption.CanUseVent);
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

		this.status.IsFakeImpostor = 
			this.HasTask &&
			this.canSeeFromImpostor &&
			this.seeFromImpostorTaskGage <= 0.0f;
	}
}
