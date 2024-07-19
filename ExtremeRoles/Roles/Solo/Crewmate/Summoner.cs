using System;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Resources;

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

	private NetworkedPlayerInfo? targetData;
	private NetworkedPlayerInfo? summonTarget;

    private float range;

    public Summoner() : base(
        ExtremeRoleId.Summoner,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Summoner.ToString(),
        ColorPalette.SummonerLiseron,
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
		var loader = this.Loader;

		float coolTime = loader.GetValue<RoleAbilityCommonOption, float>(
			RoleAbilityCommonOption.AbilityCoolTime);

		var img = UnityObjectLoader.LoadSpriteFromResources(ObjectPath.TestButton);

		var markingAbility = new CountBehavior(
			"marking", img,
			isUseMarking,
			marking);
		markingAbility.SetCoolTime(coolTime);
		markingAbility.SetAbilityCount(
			loader.GetValue<Option, int>(Option.MarkingCount));

		var summonAbility = new CountBehavior(
			"Summon", img,
			isUseSummon,
			summon);
		summonAbility.SetCoolTime(coolTime);
		summonAbility.SetAbilityCount(
			loader.GetValue<Option, int>(Option.SummonCount));

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
            PlayerControl.LocalPlayer, this,
            this.range);
        if (target == null) { return false; }

		this.targetData = target.Data;

        return IRoleAbility.IsCommonUse();
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
		if (this.summonTarget != null &&
			this.summonTarget.IsDead)
		{
			this.summonTarget = null;
		}
	}

    public void ResetOnMeetingStart()
    {
    }

	public override Color GetTargetRoleSeeColor(SingleRoleBase? targetRole, byte targetPlayerId)
	{
		if (this.summonTarget != null &&
			this.summonTarget.PlayerId == targetPlayerId)
		{
			return ColorPalette.DelusionerPink;
		}
		return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
	}

	protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {

		factory.CreateFloatOption(
			RoleAbilityCommonOption.AbilityCoolTime,
			IRoleAbility.DefaultCoolTime,
			IRoleAbility.MinCoolTime,
			IRoleAbility.MaxCoolTime,
			IRoleAbility.Step,
			format: OptionUnit.Second);

		factory.CreateIntOption(
			Option.MarkingCount,
			3, 1, 10, 1);

		factory.CreateFloatOption(
			Option.Range,
			2.5f, 0.0f, 7.5f, 0.1f);

		factory.CreateIntOption(
			Option.SummonCount,
			3, 1, 10, 1);
	}

    protected override void RoleSpecificInit()
    {
        this.range = this.Loader.GetValue<Option, float>(Option.Range);
    }

	private bool isUseMarking()
	{
		this.targetData = null;

		PlayerControl target = Player.GetClosestPlayerInRange(
			PlayerControl.LocalPlayer, this,
			this.range);
		if (target == null) { return false; }

		this.targetData = target.Data;

		return IRoleAbility.IsCommonUse();
	}

	private bool isUseSummon()
		=> this.summonTarget != null && IRoleAbility.IsCommonUse();

	private bool summon()
	{
		var local = PlayerControl.LocalPlayer;
		if (this.summonTarget == null ||
			local == null)
		{
			return false;
		}

		var pos = local.transform.position;
		byte lastPlayerId = this.summonTarget.PlayerId;
		bool isDead = this.summonTarget.IsDead;

		using (var writer = RPCOperator.CreateCaller(
			RPCOperator.Command.SummonerOps))
		{
			writer.WriteByte(local.PlayerId);
			writer.WriteFloat(pos.x);
			writer.WriteFloat(pos.y);
			writer.WriteByte(lastPlayerId);
			writer.WriteBoolean(isDead);
		}
		RpcOps(
			local.PlayerId,
			lastPlayerId,
			pos.x, pos.y,
			isDead);

		return true;
	}

	private bool marking()
	{
		if (this.targetData == null) { return false; }

		this.summonTarget = this.targetData;

		return true;
	}
}
