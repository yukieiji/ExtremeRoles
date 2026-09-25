using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Roles.API.Interface;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class SpecialDove : SingleRoleBase, IRoleUpdate
{
	private DoveCommonAbilityHandler? handler;

	public SpecialDove() : base(
		RoleArgs.BuildLiberalDove(ExtremeRoleId.SpecialDove))
	{
	}

	public void Update(PlayerControl rolePlayer)
	{
		this.handler ??= new DoveCommonAbilityHandler(
			ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptionLoader>());
		this.handler.Update(rolePlayer);
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		this.handler?.ClearTask(rolePlayer);
	}

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		this.handler?.ClearTask(rolePlayer);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		CreateKillerOption(factory);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.HasOtherKillCool = loader.GetValue<KillerCommonOption, bool>(
			KillerCommonOption.HasOtherKillCool);
		if (this.HasOtherKillCool)
		{
			this.KillCoolTime = loader.GetValue<KillerCommonOption, float>(
				KillerCommonOption.KillCoolDown);
		}

		this.HasOtherKillRange = loader.GetValue<KillerCommonOption, bool>(
			KillerCommonOption.HasOtherKillRange);

		if (this.HasOtherKillRange)
		{
			this.KillRange = loader.GetValue<KillerCommonOption, int>(
				KillerCommonOption.KillRange);
		}
	}
}
