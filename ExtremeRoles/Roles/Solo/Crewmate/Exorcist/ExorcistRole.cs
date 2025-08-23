using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;

using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Crewmate.Exorcist;

#nullable enable

public sealed class ExorcistRole :
	SingleRoleBase,
	IRoleUpdate,
	IRoleAutoBuildAbility
{
	public override IStatusModel? Status => status;

	public ExtremeAbilityButton? Button { get; set; }

	private ExorcistStatus? status;
	private NetworkedPlayerInfo? target;
	private NetworkedPlayerInfo? tmpTarget;
	private bool withName;

	public enum Option
	{
		Range,
		Mode,
		AwakeTaskGage,
	}

	public ExorcistRole() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Exorcist,
			ColorPalette.AgencyYellowGreen),
		false, true, false, false)
	{

	}

	public void Update(PlayerControl rolePlayer)
	{
		this.status?.FrameUpdate(rolePlayer);
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 1, 5, 3.0f);
		factory.CreateFloatOption(
			Option.Range,
			1.7f, 0.1f, 3.5f, 0.1f);
		factory.Create0To100Percentage10StepOption(Option.AwakeTaskGage);
	}

	protected override void RoleSpecificInit()
	{
		this.status = new ExorcistStatus(
			this,
			100.0f / this.Loader.GetValue<Option, int>(Option.AwakeTaskGage),
			this.Loader.GetValue<Option, float>(Option.Range));
	}

	public bool UseAbility()
	{
		this.target = this.tmpTarget;
		return true;
	}

	public bool IsAbilityUse()
	{
		this.tmpTarget = this.status?.CurTarget;
		return this.tmpTarget != null;
	}

	public bool IsAbilityActive()
		=> this.target == this.status?.CurTarget;

	public void CreateAbility()
	{
	}

	public void CleanUp()
	{
		if (this.target == null ||
			!ExtremeRolesPlugin.ShipState.DeadPlayerInfo.TryGetValue(
				this.target.PlayerId, out var info) ||
			info is null ||
			info.Killer == null ||
			info.Killer.Data == null)
		{
			return;
		}

		var killer = info.Killer.Data;

		MeetingReporter.Instance.AddMeetingStartReport(
			this.withName ?
			$"{killer.DefaultOutfit.PlayerName} kill {this.target.DefaultOutfit.PlayerName} with {info.Reason}" :
			$"{this.target.DefaultOutfit.PlayerName} killed with {info.Reason} killer is Dead{killer.IsDead}");
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{ }

	public void ResetOnMeetingStart()
	{
	}
}
