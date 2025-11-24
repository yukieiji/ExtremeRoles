using TMPro;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Ability;


namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class LeaderAbilityHandler : IAbility, IInvincible
{
	public bool IsBlockKillFrom(byte? fromPlayer)
	{
		throw new System.NotImplementedException();
	}

	public bool IsValidTarget(byte target)
	{
		throw new System.NotImplementedException();
	}
}

public sealed class Leader : SingleRoleBase
{
	private readonly TextMeshPro text;
	private readonly LiberalMoneyBankSystem system;

	public Leader(
		LeaderAbilityHandler abilityHandler,
		LiberalMoneyBankSystem system) : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Leader,
			ColorPalette.AgencyYellowGreen),
		false, false, false, false)
	{
		this.system = system;
		this.AbilityClass = abilityHandler;
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
