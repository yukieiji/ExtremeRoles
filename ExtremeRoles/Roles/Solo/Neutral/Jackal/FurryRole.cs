using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Jackal;

public sealed class FurryRole : SingleRoleBase, IRoleWinPlayerModifier, IRoleUpdate
{
	public enum Option
	{
		UseVent
	}

	public override IStatusModel? Status => this.status;

	private FurryStatus? status;

	public FurryRole() : base(
		ExtremeRoleId.Furry,
		ExtremeRoleType.Neutral,
		ExtremeRoleId.Furry.ToString(),
		ColorPalette.JackalBlue,
		true, false, true, false)
	{ }

	public void ModifiedWinPlayer(
		NetworkedPlayerInfo rolePlayerInfo,
		GameOverReason reason,
		in WinnerTempData winner)
	{
		switch (reason)
		{
			case (GameOverReason)RoleGameOverReason.JackalKillAllOther:
				winner.AddPool(rolePlayerInfo);
				break;
			default:
				break;
		}
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(Option.UseVent, false);
	}

	protected override void RoleSpecificInit()
	{
		this.UseVent = this.Loader.GetValue<Option, bool>(Option.UseVent);
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (ShipStatus.Instance == null ||
			!ShipStatus.Instance.enabled ||
			!RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return;
		}

		if (this.status is null)
		{
			this.status = new FurryStatus();
		}
	}
}
