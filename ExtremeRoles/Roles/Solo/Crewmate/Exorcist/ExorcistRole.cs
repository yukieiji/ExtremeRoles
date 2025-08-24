using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;

using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using Hazel;

namespace ExtremeRoles.Roles.Solo.Crewmate.Exorcist;

#nullable enable

public sealed class ExorcistRole :
	SingleRoleBase,
	IDeadBodyReportOverride,
	IRoleUpdate,
	IRoleAutoBuildAbility
{
	public override IStatusModel? Status => status;

	public ExtremeAbilityButton? Button { get; set; }

	public bool CanReport => false;

	private ExorcistStatus? status;
	private NetworkedPlayerInfo? target;
	private NetworkedPlayerInfo? tmpTarget;
	private bool withName;

	public enum Option
	{
		Range,
		WithName,
		AwakeTaskGage,
	}

	public ExorcistRole() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Exorcist,
			ColorPalette.AgencyYellowGreen),
		false, true, false, false)
	{

	}

	public static void RpcOps(in MessageReader reader)
	{
		byte player = reader.ReadByte();
		var exorcist = ExtremeRoleManager.GetSafeCastedRole<ExorcistRole>(player);
		exorcist?.status?.UpdateToFakeImpostor();
	}

	public void Update(PlayerControl rolePlayer)
	{
		this.status?.FrameUpdate(rolePlayer);
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		factory.Create0To100Percentage10StepOption(Option.AwakeTaskGage);
	
		IRoleAbility.CreateAbilityCountOption(factory, 1, 5, 3.0f);
		factory.CreateFloatOption(
			Option.Range,
			1.7f, 0.1f, 3.5f, 0.1f);
		factory.CreateBoolOption(Option.WithName, false);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.withName = loader.GetValue<Option, bool>(Option.WithName);
		this.status = new ExorcistStatus(
			this,
			loader.GetValue<Option, int>(Option.AwakeTaskGage) / 100.0f,
			loader.GetValue<Option, float>(Option.Range));
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
		this.CreateActivatingAbilityCountButton(
			"悪魔祓い",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.TestButton),
			abilityOff: this.CleanUp,
			isReduceOnActive: true);
		this.Button?.SetLabelToCrewmate();
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
			$"{this.target.DefaultOutfit.PlayerName} killed with {info.Reason} killer is Dead {killer.IsDead}");

		// 能力使用後に強制的に会議発動
		var localPlayer = PlayerControl.LocalPlayer;
		MeetingRoomManager.Instance.AssignSelf(localPlayer, this.target);
		HudManager.Instance.OpenMeetingRoom(localPlayer);
		localPlayer.RpcStartMeeting(this.target);
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void ResetOnMeetingStart()
	{
	}
}
