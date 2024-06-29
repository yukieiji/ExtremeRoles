using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;


using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Evolver : SingleRoleBase, IRoleAutoBuildAbility
{
    public enum EvolverOption
    {
        IsEvolvedAnimation,
        IsEatingEndCleanBody,
        EatingRange,
        KillCoolReduceRate,
        KillCoolResuceRateMulti,
    }


    public NetworkedPlayerInfo targetBody;
    public byte eatingBodyId;

    private float eatingRange = 1.0f;
    private float reduceRate = 1.0f;
    private float reruceMulti = 1.0f;
    private bool isEvolvdAnimation;
    private bool isEatingEndCleanBody;

    private string defaultButtonText;
    private string eatingText;

    private float defaultKillCoolTime;

    public ExtremeAbilityButton Button
    {
        get => this.evolveButton;
        set
        {
            this.evolveButton = value;
        }
    }
    private ExtremeAbilityButton evolveButton;

    public Evolver() : base(
        ExtremeRoleId.Evolver,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.Evolver.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    {
        this.isEatingEndCleanBody = false;
    }

    public void CreateAbility()
    {
        this.defaultButtonText = Translation.GetString("evolve");

        this.CreateAbilityCountButton(
            "evolve",
			Resources.Loader.CreateSpriteFromResources(
				Path.EvolverEvolved),
            checkAbility: CheckAbility,
            abilityOff: CleanUp,
            forceAbilityOff: ForceCleanUp);
    }

    public bool IsAbilityUse()
    {
        this.targetBody = Player.GetDeadBodyInfo(
            this.eatingRange);
        return IRoleAbility.IsCommonUse() && this.targetBody != null;
    }

    public void ForceCleanUp()
    {
        this.targetBody = null;
    }

    public void CleanUp()
    {

        PlayerControl rolePlayer = PlayerControl.LocalPlayer;

        if (this.isEvolvdAnimation)
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.UncheckedShapeShift))
            {
                caller.WriteByte(rolePlayer.PlayerId);
                caller.WriteByte(rolePlayer.PlayerId);
                caller.WriteByte(byte.MaxValue);
            }
            RPCOperator.UncheckedShapeShift(
                rolePlayer.PlayerId,
                rolePlayer.PlayerId,
                byte.MaxValue);
        }

        this.KillCoolTime = this.KillCoolTime * ((100f - this.reduceRate) / 100f);
        this.reduceRate = this.reduceRate * this.reruceMulti;

        this.CanKill = true;
        this.KillCoolTime = Mathf.Clamp(
            this.KillCoolTime, 0.1f, this.defaultKillCoolTime);

        this.Button.Behavior.SetButtonText(this.defaultButtonText);

        if (!this.isEatingEndCleanBody) { return; }

        Player.RpcCleanDeadBody(this.eatingBodyId);
    }

    public bool CheckAbility()
    {
        this.targetBody = Player.GetDeadBodyInfo(
            this.eatingRange);

        bool result;

        if (this.targetBody == null)
        {
            result = false;
        }
        else
        {
            result = this.eatingBodyId == this.targetBody.PlayerId;
        }

        this.Button.Behavior.SetButtonText(
            result ? this.eatingText : this.defaultButtonText);

        return result;
    }

    public bool UseAbility()
    {
        this.eatingBodyId = this.targetBody.PlayerId;
        return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateBoolOption(
            EvolverOption.IsEvolvedAnimation,
            true);

        factory.CreateBoolOption(
            EvolverOption.IsEatingEndCleanBody,
            true);

        factory.CreateFloatOption(
            EvolverOption.EatingRange,
            2.5f, 0.5f, 5.0f, 0.5f);

        factory.CreateIntOption(
            EvolverOption.KillCoolReduceRate,
            10, 1, 50, 1,
            format: OptionUnit.Percentage);

        factory.CreateFloatOption(
            EvolverOption.KillCoolResuceRateMulti,
            1.0f, 1.0f, 5.0f, 0.1f,
            format: OptionUnit.Multiplier);

        IRoleAbility.CreateAbilityCountOption(
            factory, 5, 10, 5.0f);
    }

    protected override void RoleSpecificInit()
    {
        if(!this.HasOtherKillCool)
        {
            this.HasOtherKillCool = true;
            this.KillCoolTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
                FloatOptionNames.KillCooldown);
        }

        this.defaultKillCoolTime = this.KillCoolTime;

        var cate = this.Loader;

        this.isEvolvdAnimation = cate.GetValue<EvolverOption, bool>(
            EvolverOption.IsEvolvedAnimation);
        this.isEatingEndCleanBody = cate.GetValue<EvolverOption, bool>(
            EvolverOption.IsEatingEndCleanBody);
        this.eatingRange = cate.GetValue<EvolverOption, float>(
            EvolverOption.EatingRange);
        this.reduceRate = cate.GetValue<EvolverOption, int>(
            EvolverOption.KillCoolReduceRate);
        this.reruceMulti = cate.GetValue<EvolverOption, float>(
            EvolverOption.KillCoolResuceRateMulti);

        this.eatingText = Translation.GetString("eating");

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
