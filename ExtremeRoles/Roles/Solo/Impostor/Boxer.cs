using Hazel;
using UnityEngine;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Boxer : SingleRoleBase, IRoleAutoBuildAbility
{
    public enum Option
    {
        StraightChargeTime,
		StraightRange,
		StraightFirstSpeed,
		StraightAcceleration,
		StraightKillSpeed,
		StraightReflectionE,
	}

	private float range;
    private PlayerControl? target;
	private float speed;
	private float killSpeed;
	private BoxerButtobiBehaviour.Parameter param;

    public ExtremeAbilityButton? Button { get; set; }


    public Boxer() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.Boxer),
        true, false, true, true)
    { }

    public void CreateAbility()
    {
		var beha = new ChargingAndReclickCountBehavior(
			Tr.GetString("BoxerAbility"),
			UnityObjectLoader.LoadSpriteFromResources(ObjectPath.TestButton),
			(isCharge, _) =>
			{
				if (isCharge)
				{
					return IsAbilityUse();
				}
				return IRoleAbility.IsCommonUse();
			},
			UseAbility,
			UseChargedAbility,
			reduceOnCharge: false);
		this.Button = new ExtremeAbilityButton(
			beha,
			new RoleButtonActivator(),
			KeyCode.F);
		((IRoleAbility)this).RoleAbilityInit();

		beha.ChargeTime = this.Loader.GetValue<Option, float>(Option.StraightChargeTime);
	}

    public bool IsAbilityUse()
    {
		this.target = Player.GetClosestPlayerInRange(PlayerControl.LocalPlayer, this, this.range);
        return IRoleAbility.IsCommonUse() && this.target != null;
    }

	public bool UseAbility() => true;

	public bool UseChargedAbility(float x)
	{
		var local = PlayerControl.LocalPlayer;
		if (this.target == null || local == null)
		{
			return false;
		}

		var direction = this.target.GetTruePosition() - local.GetTruePosition();
		direction = direction.normalized * x;

		using (var op = RPCOperator.CreateCaller(RPCOperator.Command.BoxerRpcOps))
		{
			op.WriteByte(PlayerControl.LocalPlayer.PlayerId);
			op.WriteByte(this.target.PlayerId);
			op.WriteFloat(direction.x);
			op.WriteFloat(direction.y);
		}

		return true;
	}

	public static void AbilityOps(in MessageReader reader)
	{
		byte rolePlayerId = reader.ReadByte();
		byte targetPlayerId = reader.ReadByte();

		float x = reader.ReadSingle();
		float y = reader.ReadSingle();

		if (targetPlayerId != PlayerControl.LocalPlayer.PlayerId ||
			!ExtremeRoleManager.TryGetSafeCastedRole<Boxer>(rolePlayerId, out var role))
		{
			return;
		}
		var targetPlayer = Player.GetPlayerControlById(targetPlayerId);
		if (targetPlayer == null)
		{
			return;
		}

		var directionOrSpeed = new Vector2(x, y);
		var beha = targetPlayer.gameObject.TryAddComponent<BoxerButtobiBehaviour>();
		beha.Initialize(directionOrSpeed, role.speed, role.killSpeed, role.param);
	}

	protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(factory, 2, 5);
		factory.CreateFloatOption(Option.StraightChargeTime, 3.0f, 0.1f, 30.0f, 0.1f, format: OptionUnit.Second);
		factory.CreateFloatOption(Option.StraightRange, 1.2f, 0.1f, 3.0f, 0.1f);
		factory.CreateIntOption(Option.StraightFirstSpeed, 15, 1, 100, 1);
		factory.CreateFloatOption(Option.StraightAcceleration, -2.5f, -10.0f, 10.0f, 0.25f);
		factory.CreateFloatOption(Option.StraightKillSpeed, 10.0f, 1.0f, 200.0f, 0.5f);
		factory.CreateFloatOption(Option.StraightReflectionE, 0.5f, 0.0f, 2.0f, 0.1f);
	}

    protected override void RoleSpecificInit()
    {
        var cate = this.Loader;

		this.range = cate.GetValue<Option, float>(Option.StraightRange);
		this.speed = cate.GetValue<Option, int>(Option.StraightFirstSpeed);
		this.killSpeed = cate.GetValue<Option, float>(Option.StraightKillSpeed);
		this.param = new BoxerButtobiBehaviour.Parameter(
			cate.GetValue<Option, float>(Option.StraightAcceleration),
			cate.GetValue<Option, float>(Option.StraightReflectionE));
    }

    public void ResetOnMeetingStart()
    {

    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
    }
}
