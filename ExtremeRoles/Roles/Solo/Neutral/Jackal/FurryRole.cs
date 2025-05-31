using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.RoleAssign;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


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
	private bool isUpdate;

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

	public static void BecomeToJackal(byte targetJackal, byte targetFurry)
	{
		var curJackal = ExtremeRoleManager.GetSafeCastedRole<JackalRole>(targetJackal);
		if (curJackal == null) { return; }
		var newJackal = (JackalRole)curJackal.Clone();

		newJackal.Initialize();
		if (targetFurry == PlayerControl.LocalPlayer.PlayerId)
		{
			newJackal.CreateAbility();
		}

		if (newJackal.Button?.Behavior is ICountBehavior countBehavior)
		{
			countBehavior.SetAbilityCount(0);
		}

		newJackal.CurRecursion = 0;
		newJackal.SidekickPlayerId = [];
		newJackal.SetControlId(curJackal.GameControlId);

		ExtremeRoleManager.SetNewRole(targetFurry, newJackal);
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
			!RoleAssignState.Instance.IsRoleSetUpEnd ||
			this.isUpdate)
		{
			return;
		}

		if (this.status is null)
		{
			this.status = new FurryStatus();
		}
		this.status.Update();

		if (this.status.TargetJackal.HasValue)
		{

			ExtremeRoleManager.RpcReplaceRole(
				this.status.TargetJackal.Value, rolePlayer.PlayerId,
				ExtremeRoleManager.ReplaceOperation.RebornJackal);
			this.isUpdate = true;
		}
	}
}
