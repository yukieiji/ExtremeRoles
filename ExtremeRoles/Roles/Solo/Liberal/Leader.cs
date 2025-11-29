using TMPro;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.GameMode.RoleSelector;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class LeaderAbilityHandler : IAbility, IInvincible
{
	public bool IsBlockKillFrom(byte? fromPlayer)
	{
		return true;
	}

	public bool IsValidTarget(byte target)
	{
		return true;
	}
}

public sealed class Leader : SingleRoleBase
{
	private readonly LiberalMoneyBankSystem system;

	public Leader(
		LiberalDefaultOptipnLoader option,
		LeaderAbilityHandler abilityHandler,
		LiberalMoneyBankSystem system) : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Leader,
			ColorPalette.AgencyYellowGreen),
		option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.CanKillLeader),
		option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.CanHasTaskLeader),
		false, false)
	{
		this.system = system;
		this.AbilityClass = abilityHandler;

		LiberalSettingOverrider.OverrideDefault(this, option);

		this.HasOtherVision = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.LeaderHasOtherVisonSize);
		if (this.HasOtherVision)
		{
			this.Vision = option.GetValue<LiberalGlobalSetting, float>(LiberalGlobalSetting.LeaderVison);
		}
		if (!this.CanKill)
		{
			return;
		}
		this.HasOtherKillRange = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.LeaderHasOtherKillRange);
		if (this.HasOtherKillRange)
		{
			this.KillRange = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.LeaderKillRange);
		}

		this.HasOtherKillRange = option.GetValue<LiberalGlobalSetting, bool>(LiberalGlobalSetting.LeaderHasOtherKillCool);
		if (this.HasOtherKillRange)
		{
			this.KillCoolTime = option.GetValue<LiberalGlobalSetting, int>(LiberalGlobalSetting.LeaderKillCool);
		}
	}

	public override string GetRoleTag()
		=> $" ({this.system.Money}/{this.system.WinMoney})";
	public override string GetRolePlayerNameTag(SingleRoleBase targetRole, byte targetPlayerId)
		=> this.GetRoleTag();

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{

	}

	protected override void RoleSpecificInit()
	{

	}
}
