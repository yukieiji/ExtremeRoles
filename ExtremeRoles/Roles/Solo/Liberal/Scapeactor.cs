using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class Scapeactor : SingleRoleBase, IRoleUpdate
{
	public enum ScapeactorOption
	{
		TaskCompletedMoney,
	}

	private DoveCommonAbilityHandler? handler;
	private float winMoney;

	public Scapeactor() : base(
		RoleArgs.BuildLiberalDove(ExtremeRoleId.Scapeactor))
	{
	}

	public override string GetRoleTag()
		=> "Sc";

	public void Update(PlayerControl rolePlayer)
	{
		this.handler?.Update(rolePlayer);
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		this.handler?.ClearTask(rolePlayer);

		LiberalMoneyBankSystem.RpcUpdateSystem(
			rolePlayer.PlayerId,
			LiberalMoneyHistory.Reason.AddOnExile,
			this.winMoney);
	}

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		this.handler?.ClearTask(rolePlayer);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateFloatOption(
			ScapeactorOption.TaskCompletedMoney,
			5f, 0.1f, 1000f, 0.1f);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		var liberalOption = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptionLoader>();
		LiberalSettingOverrider.OverrideDefault(this, liberalOption);

		float taskDelta = loader.GetValue<ScapeactorOption, float>(ScapeactorOption.TaskCompletedMoney);
		this.winMoney = liberalOption.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.WinMoney);
		this.handler = new DoveCommonAbilityHandler(taskDelta, 0.0f);
	}
}
