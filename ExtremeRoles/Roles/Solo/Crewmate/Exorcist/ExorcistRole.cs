using Hazel;
using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;


using static ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus;
using ExtremeRoles.Module.CustomOption.Factory;

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

	private List<ButtonLockSystem> lockSystem = [];

	public enum RpcOpsMode : byte
	{
		Alert,
		AwakeFakeImp
	}

	public enum BlockMode
	{
		BlockModeAbilityButton,
		BlockModeReportButton,
		BlockModeKillButton,
		BlockModeAbilityAndReportButton,
		BlockModeAbilityAndKillButton,
		BlockModeKillAndReportButton,
		BlockModeAll,
		BlockModeNone
	}

	public enum Option
	{
		Range,
		WithName,
		CrewBlockSystemType,
		AwakeTaskGage,
	}

	private readonly FullScreenFlasher flasher = new FullScreenFlasher(Palette.CrewmateBlue);

	public ExorcistRole() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Exorcist,
			Palette.ImpostorRed),
		false, true, false, false)
	{

	}

	public static void RpcOps(in MessageReader reader)
	{
		var ops = (RpcOpsMode)reader.ReadByte();
		byte player = reader.ReadByte();
		var exorcist = ExtremeRoleManager.GetSafeCastedRole<ExorcistRole>(player);
		if (exorcist is null)
		{
			return;
		}

		switch (ops)
		{
			case RpcOpsMode.Alert:
				var hudManager = HudManager.Instance;
				if (hudManager == null || PlayerControl.LocalPlayer == null)
				{
					return;
				}
				var localRole = ExtremeRoleManager.GetLocalPlayerRole();
				if (localRole.IsCrewmate() || !localRole.CanKill())
				{
					return;
				}
				exorcist.flasher.Flash();
				break;
			case RpcOpsMode.AwakeFakeImp:
				exorcist?.status?.UpdateToFakeImpostor();
				break;
			default:
				break;
		}
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
		factory.CreateNewFloatOption(
			Option.Range,
			1.7f, 0.1f, 3.5f, 0.1f);
		factory.CreateNewBoolOption(Option.WithName, false);
		factory.CreateSelectionOption<Option, BlockMode>(Option.CrewBlockSystemType);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.withName = loader.GetValue<Option, bool>(Option.WithName);
		this.status = new ExorcistStatus(
			loader.GetValue<Option, int>(Option.AwakeTaskGage) / 100.0f,
			loader.GetValue<Option, float>(Option.Range));

		this.lockSystem = (BlockMode)loader.GetValue<Option, int>(Option.CrewBlockSystemType) switch
		{
			BlockMode.BlockModeAbilityButton => [ButtonLockSystem.CreateOrGetAbilityButtonLockSystem()],
			BlockMode.BlockModeReportButton => [ ButtonLockSystem.CreateOrGetReportButtonLock(), ],
			BlockMode.BlockModeKillButton => [ButtonLockSystem.CreateOrGetKillButtonLockSystem(),],
			BlockMode.BlockModeAbilityAndReportButton => [
				ButtonLockSystem.CreateOrGetAbilityButtonLockSystem(),
				ButtonLockSystem.CreateOrGetReportButtonLock()
			],
			BlockMode.BlockModeAbilityAndKillButton => [
				ButtonLockSystem.CreateOrGetAbilityButtonLockSystem(),
				ButtonLockSystem.CreateOrGetKillButtonLockSystem(),
			],
			BlockMode.BlockModeKillAndReportButton => [
				ButtonLockSystem.CreateOrGetReportButtonLock(),
				ButtonLockSystem.CreateOrGetKillButtonLockSystem(),
			],
			BlockMode.BlockModeAll => [
				ButtonLockSystem.CreateOrGetAbilityButtonLockSystem(),
				ButtonLockSystem.CreateOrGetReportButtonLock(),
				ButtonLockSystem.CreateOrGetKillButtonLockSystem(),
			],
			_ => []
		};
		foreach (var s in this.lockSystem)
		{
			s.AddCondition((int)ButtonLockSystem.ConditionId.Exorcist, exorcistBlockCondition);
		}
	}

	public bool UseAbility()
	{
		if (PlayerControl.LocalPlayer == null)
		{
			return false;
		}

		this.target = this.tmpTarget;
		foreach (var s in this.lockSystem)
		{
			s.RpcLock(ButtonLockSystem.Ops.RpcLock, (int)ButtonLockSystem.ConditionId.Exorcist);
		}
		using (var op = RPCOperator.CreateCaller(
			RPCOperator.Command.ExorcistOps))
		{
			op.WriteByte((byte)RpcOpsMode.Alert);
			op.WriteByte(PlayerControl.LocalPlayer.PlayerId);
		}
		return true;
	}

	public bool IsAbilityUse()
	{
		this.tmpTarget = this.status?.CurTarget;
		return this.tmpTarget != null;
	}

	public bool IsAbilityActive()
	{
		return this.target == this.status?.CurTarget;
	}

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			Tr.GetString("ExorcistAbility"),
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Exorcist),
			checkAbility: IsAbilityActive,
			forceAbilityOff: unlock,
			abilityOff: this.CleanUp,
			isReduceOnActive: true);
		this.Button?.SetLabelToCrewmate();
	}

	public void CleanUp()
	{
		unlock();

		if (this.target == null ||
			!ExtremeRolesPlugin.ShipState.DeadPlayerInfo.TryGetValue(
				this.target.PlayerId, out var info) ||
			info.Killer == null ||
			info.Killer.Data == null ||
			MeetingHud.Instance != null)
		{
			return;
		}

		var killer = info.Killer.Data;

		string reason = Tr.GetString(info.Reason.ToString());
		MeetingReporter.Instance.AddMeetingChatReport(
			this.withName ?
			Tr.GetString("ExorcistReportWithName", killer.DefaultOutfit.PlayerName, this.target.DefaultOutfit.PlayerName, reason) :
			Tr.GetString(
				"ExorcistReport", this.target.DefaultOutfit.PlayerName, reason,
				killer.IsDead ? Tr.GetString(PlayerStatus.Dead.ToString()) : Tr.GetString(PlayerStatus.Alive.ToString())));

		var localPlayer = PlayerControl.LocalPlayer;
		if (!(
				localPlayer == null ||
				localPlayer.Data == null ||
				localPlayer.Data.IsDead ||
				localPlayer.Data.Disconnected
			))
		{
			localPlayer.CmdReportDeadBody(this.target);
		}
		this.target = null;
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void ResetOnMeetingStart()
	{
		this.flasher.Hide();
	}

	private void unlock()
	{
		foreach (var s in this.lockSystem)
		{
			s.RpcLock(ButtonLockSystem.Ops.Unlock, (int)ButtonLockSystem.ConditionId.Exorcist);
		}
	}

	private static bool exorcistBlockCondition()
	{
		if (PlayerControl.LocalPlayer == null || !GameProgressSystem.IsTaskPhase)
		{
			return true;
		}

		if (ExtremeRoleManager.TryGetSafeCastedLocalRole<ExorcistRole>(out _))
		{
			return false;
		}
		return ExtremeRoleManager.GetLocalPlayerRole().IsCrewmate();
	}
}
