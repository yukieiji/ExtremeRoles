using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral.Yandere;

#nullable enable

public sealed class SurrogatorRole : SingleRoleBase, IRoleAutoBuildAbility
{
	public enum Option
	{
		UseVent,
		Range,
		PreventNum,
		PreventKillTime,
	}

	public ExtremeAbilityButton? Button { get; set; }

	private NetworkedPlayerInfo? targetBody;
	private NetworkedPlayerInfo DeadBody => Player.GetDeadBodyInfo(this.range);
	private byte activateTarget;
	private float range;

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"死体纏い",
			UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.EvolverEvolved),
			checkAbility: CheckAbility,
			abilityOff: CleanUp,
			forceAbilityOff: ForceCleanUp);
	}

	public bool IsAbilityUse()
	{
		this.targetBody = this.DeadBody;
		return IRoleAbility.IsCommonUse() && this.targetBody != null;
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void ResetOnMeetingStart()
	{
	}

	public bool UseAbility()
	{
		if (this.targetBody == null)
		{
			return false;
		}
		this.activateTarget = this.targetBody.PlayerId;
		return true;
	}

	public void CleanUp()
	{
		Player.RpcCleanDeadBody(this.activateTarget);

		SurrogatorGurdSystem.RpcReduce();

		this.activateTarget = byte.MaxValue;
	}

	public bool CheckAbility()
	{
		var check = this.DeadBody;
		return
			check != null &&
			this.activateTarget == check.PlayerId;
	}

	public void ForceCleanUp()
	{
		this.targetBody = null;
	}

	public static bool TryGurdOnesideLover(PlayerControl killer, byte targetPlayerId)
	{
		if (!(
				ExtremeSystemTypeManager.Instance.TryGet<SurrogatorGurdSystem>(
					ExtremeSystemType.SurrogatorGurdSystem, out var system) &&
				system.CanGuard(targetPlayerId)
			))
		{
			return false;
		}

		killer.SetKillTimer(system.PreventKillTime);

		SurrogatorGurdSystem.RpcReduce();
		return true;
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(Option.UseVent, false);
		IRoleAbility.CreateAbilityCountOption(factory, 1, 10, 3.0f);
		factory.CreateFloatOption(Option.Range, 0.7f, 0.1f, 3.5f, 0.1f);
		factory.CreateIntOption(Option.PreventNum, 1, 0, 10, 1);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.UseVent = loader.GetValue<Option, bool>(Option.UseVent);
		this.range = loader.GetValue<Option, float>(Option.Range);
		var system = SurrogatorGurdSystem.CreateOrGet(
			loader.GetValue<Option, float>(Option.PreventKillTime));
		system.AddGuardNum(
			loader.GetValue<Option, int>(Option.PreventNum));
	}
}
