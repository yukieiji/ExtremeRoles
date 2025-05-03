using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API;

using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class TimeBreaker : SingleRoleBase, IRoleAutoBuildAbility
{
	public enum Opt
	{
		ActiveTime,
		EffectImp,
		EffectOnMarlin,
		IsActiveScreen
	}

	public ExtremeAbilityButton Button { get; set; }

	public TimeBreaker() : base(
		ExtremeRoleId.TimeBreaker,
		ExtremeRoleType.Impostor,
		ExtremeRoleId.TimeBreaker.ToString(),
		Palette.ImpostorRed,
		true, false, true, true)
	{ }

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"timeBreakerTimeBreak",
			UnityObjectLoader.LoadFromResources(
				ExtremeRoleId.TimeBreaker));
	}

	public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse();

	public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
	{ }

	public void ResetOnMeetingStart()
	{ }

	public bool UseAbility()
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.TimeBreakerTimeBreakSystem,
			(_) => { });

		return true;
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(
			factory, 2, 100);
		factory.CreateFloatOption(
			Opt.ActiveTime, 10.0f, 1.0f, 120.0f, 0.5f,
			format: OptionUnit.Second);
		var impOpt =　factory.CreateBoolOption(
			Opt.EffectImp, true);
		factory.CreateBoolOption(
			Opt.EffectOnMarlin, false,
			impOpt);
		factory.CreateBoolOption(
			Opt.IsActiveScreen, true);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		_ = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.TimeBreakerTimeBreakSystem,
			() => new TimeBreakerTimeBreakSystem(
				loader.GetValue<Opt, float>(Opt.ActiveTime),
				loader.GetValue<Opt, bool>(Opt.EffectImp),
				loader.GetValue<Opt, bool>(Opt.EffectOnMarlin),
				loader.GetValue<Opt, bool>(Opt.IsActiveScreen)));
	}
}
