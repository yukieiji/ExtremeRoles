using System;

using UnityEngine;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Performance;




#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Summoner :
    SingleRoleBase,
    IRoleAbility
{

	public ExtremeAbilityButton? Button
	{
		get => this.internalButton;
		set
		{
			if (value is not ExtremeMultiModalAbilityButton button)
			{
				throw new ArgumentException("This role using multimodal ability");
			}
			this.internalButton = button;
		}
	}
	private ExtremeMultiModalAbilityButton? internalButton;

	public enum Option
	{
		MarkingCount,
		SummonCount,
        Range,
    }

	private GameData.PlayerInfo? targetData;
	private GameData.PlayerInfo? summonTarget;

    private float range;

    public Summoner() : base(
        ExtremeRoleId.Summoner,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Summoner.ToString(),
        ColorPalette.DelusionerPink,
        false, true, false, false)
    { }

	public static void RpcOps(byte rolePlayerId, byte targetPlayerId, float x, float y, bool isDead)
	{
		var rolePlayer = Player.GetPlayerControlById(rolePlayerId);
		if (rolePlayer == null) { return; }

		var pos = new Vector2(x, y);
		rolePlayer.NetTransform.SnapTo(pos);

		if (isDead)
		{
			DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
			for (int i = 0; i < array.Length; ++i)
			{
				var target = array[i];
				if (GameData.Instance.GetPlayerById(target.ParentId).PlayerId != targetPlayerId)
				{
					continue;
				}
				target.transform.position = new Vector3(x, y, y / 1000.0f);
				break;
			}
		}
		else
		{
			var targetPlayer = Player.GetPlayerControlById(targetPlayerId);
			if (targetPlayer == null) { return; }
			targetPlayer.NetTransform.SnapTo(pos);
		}
	}

    public void CreateAbility()
    {
		var opt = OptionManager.Instance;
		float coolTime = opt.GetValue<float>(
			this.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime));

		var markingAbility = new CountBehavior(
			"marking", null,
			isUseMarking,
			marking);
		markingAbility.SetCoolTime(coolTime);
		markingAbility.SetAbilityCount(
			opt.GetValue<int>(this.GetRoleOptionId(Option.MarkingCount)));

		var summonAbility = new CountBehavior(
			"Summon", null,
			isUseSummon,
			summon);
		summonAbility.SetCoolTime(coolTime);
		summonAbility.SetAbilityCount(
			opt.GetValue<int>(this.GetRoleOptionId(Option.SummonCount)));

		this.Button = new ExtremeMultiModalAbilityButton(
			new RoleButtonActivator(),
			KeyCode.F,
			markingAbility,
			summonAbility);

		this.Button.SetLabelToCrewmate();
	}

    public string GetFakeOptionString() => "";

    public bool IsAbilityUse()
    {
        this.targetData = null;

        PlayerControl target = Player.GetClosestPlayerInRange(
            CachedPlayerControl.LocalPlayer, this,
            this.range);
        if (target == null) { return false; }

		this.targetData = target.Data;

        return IRoleAbility.IsCommonUse();
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
		if (this.summonTarget != null &&
			this.summonTarget.IsDead)
		{
			this.summonTarget = null;
		}
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {

		CreateFloatOption(
			RoleAbilityCommonOption.AbilityCoolTime,
			IRoleAbilityMixin.DefaultCoolTime,
			IRoleAbilityMixin.MinCoolTime,
			IRoleAbilityMixin.MaxCoolTime,
			IRoleAbilityMixin.Step,
			parentOps,
			format: OptionUnit.Second);

		CreateIntOption(
			Option.MarkingCount,
			3, 1, 10, 1, parentOps);

		CreateFloatOption(
			Option.Range,
			2.5f, 0.0f, 7.5f, 0.1f,
			parentOps);

		CreateIntOption(
			Option.SummonCount,
			3, 1, 10, 1, parentOps);
	}

    protected override void RoleSpecificInit()
    {
        var allOpt = OptionManager.Instance;
        this.range = allOpt.GetValue<float>(
            GetRoleOptionId(Option.Range));
    }

	private bool isUseMarking()
	{
		this.targetData = null;

		PlayerControl target = Player.GetClosestPlayerInRange(
			CachedPlayerControl.LocalPlayer, this,
			this.range);
		if (target == null) { return false; }

		this.targetData = target.Data;

		return IRoleAbility.IsCommonUse();
	}

	private bool isUseSummon()
		=> this.summonTarget != null && IRoleAbility.IsCommonUse();

	private bool summon()
	{
		if (this.summonTarget == null ||
			CachedPlayerControl.LocalPlayer == null)
		{
			return false;
		}

		var local = CachedPlayerControl.LocalPlayer;

		using (var writer = RPCOperator.CreateCaller(
			RPCOperator.Command.SummonerOps))
		{
			var pos = local.PlayerControl.transform.position;
			writer.WriteByte(local.PlayerId);
			writer.WriteFloat(pos.x);
			writer.WriteFloat(pos.y);
			writer.WriteByte(this.summonTarget.PlayerId);
			writer.WriteBoolean(this.summonTarget.IsDead);
		}

		return true;
	}

	private bool marking()
	{
		if (this.targetData == null) { return false; }

		this.summonTarget = this.targetData;

		return true;
	}
}
