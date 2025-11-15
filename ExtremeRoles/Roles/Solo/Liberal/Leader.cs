using TMPro;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;


namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class Leader : SingleRoleBase
{
	private readonly TextMeshPro text;
	private readonly LiberalMoneyBankSystem system;

	public Leader() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Leader,
			ColorPalette.AgencyYellowGreen),
		false, false, false, false)
	{
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
