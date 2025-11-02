using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Patches.Button;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Smasher : SingleRoleBase, IRoleAutoBuildAbility
{
    public enum SmasherOption
    {
        SmashPenaltyKillCool,
    }

    public ExtremeAbilityButton Button
    {
        get => this.smashButton;
        set
        {
            this.smashButton = value;
        }
    }

    private ExtremeAbilityButton smashButton;
    private byte targetPlayerId;
    private float prevKillCool;
    private float penaltyKillCool;

    public Smasher() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.Smasher),
        true, false, true, true)
    { }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "smash", HudManager.Instance.KillButton.graphic.sprite);
    }

    public bool IsAbilityUse()
    {
        this.targetPlayerId = byte.MaxValue;
        var player = Player.GetClosestPlayerInKillRange();
        if (player != null)
        {
            this.targetPlayerId = player.PlayerId;
        }
        return IRoleAbility.IsCommonUse() && this.targetPlayerId != byte.MaxValue;
    }

    public bool UseAbility()
    {
        PlayerControl killer = PlayerControl.LocalPlayer;
        if (killer == null ||
			killer.Data.IsDead ||
			!killer.CanMove)
		{
			return false;
		}

		var target = Player.GetPlayerControlById(this.targetPlayerId);
		var killResult = KillButtonDoClickPatch.CheckPreKillCondition(
			this, PlayerControl.LocalPlayer,
			target);

		switch (killResult)
		{
			case KillButtonDoClickPatch.KillResult.Success:
				break;
			case KillButtonDoClickPatch.KillResult.BlockedToBodyguard:
				featKillPenalty(killer);
				return true;
			default:
				return false;
		}

        this.prevKillCool = PlayerControl.LocalPlayer.killTimer;

        Player.RpcUncheckMurderPlayer(
            killer.PlayerId,
            target.PlayerId,
            byte.MaxValue);

        featKillPenalty(killer);
        return true;
    }

    private void featKillPenalty(PlayerControl killer)
    {
        if (this.penaltyKillCool > 0.0f)
        {

            this.HasOtherKillCool = true;
            API.Extension.State.RoleState.AddKillCoolOffset(
                this.penaltyKillCool);
        }

        killer.killTimer = this.prevKillCool;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(
            factory, 1, 14);

        factory.CreateNewFloatOption(
            SmasherOption.SmashPenaltyKillCool,
            4.0f, 0.0f, 30f, 0.5f,
            format: OptionUnit.Second);

    }

    protected override void RoleSpecificInit()
    {
        this.penaltyKillCool = this.Loader.GetValue<SmasherOption, float>(
            SmasherOption.SmashPenaltyKillCool);
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }
}
