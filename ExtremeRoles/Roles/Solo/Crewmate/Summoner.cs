using System;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.CustomOption.Factory;

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

	private PlayerControl? targetData;
	private PlayerControl? summonTarget;
	private CountBehavior? markingBehavior;

    private float range;

    public Summoner() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Summoner,
			ColorPalette.SummonerToukoushoku),
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

		this.markingBehavior = new CountBehavior(
			Tr.GetString("Marking"),
			UnityObjectLoader.LoadFromResources(
				ExtremeRoleId.Summoner,
				ObjectPath.SummonerMarking),
			isUseMarking,
			marking);
		this.markingBehavior.SetCoolTime(coolTime);
		this.markingBehavior.SetAbilityCount(
			loader.GetValue<Option, int>(Option.MarkingCount));

		var summonAbility = new CountBehavior(
			Tr.GetString("Summon"),
			UnityObjectLoader.LoadFromResources(
				ExtremeRoleId.Summoner,
				ObjectPath.SummonerSummon),
			isUseSummon,
			summon);
		summonAbility.SetCoolTime(coolTime);
		summonAbility.SetAbilityCount(
			loader.GetValue<Option, int>(Option.SummonCount));

		this.Button = new ExtremeMultiModalAbilityButton(
			new RoleButtonActivator(),
			KeyCode.F,
			this.markingBehavior,
			summonAbility);

		this.Button.SetLabelToCrewmate();
	}

    public string GetFakeOptionString() => "";

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
		if (this.summonTarget != null &&
			this.summonTarget.Data != null &&
			(
				this.summonTarget.Data.IsDead ||
				(
					exiledPlayer != null &&
					exiledPlayer.PlayerId == this.summonTarget.PlayerId
			)))
		{
			this.summonTarget = null;
		}
	}

    public void ResetOnMeetingStart()
    {
    }

	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (this.summonTarget != null &&
			this.summonTarget.PlayerId == targetPlayerId)
		{
			return Design.ColoredString(this.Core.Color, " ◀");
		}
		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {

		factory.CreateNewFloatOption(
			RoleAbilityCommonOption.AbilityCoolTime,
			IRoleAbility.DefaultCoolTime,
			IRoleAbility.MinCoolTime,
			IRoleAbility.MaxCoolTime,
			IRoleAbility.Step,
			format: OptionUnit.Second);

		factory.CreateNewIntOption(
			Option.MarkingCount,
			3, 1, 10, 1);

		factory.CreateNewFloatOption(
			Option.Range,
			2.5f, 0.0f, 7.5f, 0.1f);

		factory.CreateNewIntOption(
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
		if (target == null ||
			(
				this.summonTarget != null &&
				target.PlayerId == this.summonTarget.PlayerId
			))
		{
			return false;
		}

		this.targetData = target;

		return IRoleAbility.IsCommonUse();
	}

	private bool isUseSummon()
		=> this.summonTarget != null && IRoleAbility.IsCommonUse();

	private bool summon()
	{
		var local = PlayerControl.LocalPlayer;
		if (local == null ||
			this.summonTarget == null ||
			this.summonTarget.Data == null)
		{
			return false;
		}

		if (this.summonTarget.onLadder ||
			this.summonTarget.inVent ||
			this.summonTarget.inMovingPlat)
		{
			return false;
		}

		var pos = local.transform.position;
		byte lastPlayerId = this.summonTarget.PlayerId;
		bool isDead = this.summonTarget.Data.IsDead;

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
		if (this.internalButton is null ||
			this.markingBehavior is null ||
			this.targetData == null) { return false; }

		this.summonTarget = this.targetData;
		if (this.markingBehavior.AbilityCount <= 1)
		{
			this.internalButton.Remove(this.markingBehavior);
			this.markingBehavior = null;
		}

		return true;
	}
}
