using System;
using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Roles.Solo.Neutral.Queen;
using ExtremeRoles.Roles.API.Extension.State;

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

	private SpriteRenderer? flash;

	public ExorcistRole() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Exorcist,
			ColorPalette.AgencyYellowGreen),
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
				if (hudManager == null ||
					PlayerControl.LocalPlayer == null)
				{
					return;
				}
				var localRole = ExtremeRoleManager.GetLocalPlayerRole();
				if (localRole.IsCrewmate() || !localRole.CanKill())
				{
					return;
				}
				if (exorcist.flash == null)
				{
					exorcist.flash = UnityEngine.Object.Instantiate(
						 hudManager.FullScreen,
						 hudManager.transform);
					exorcist.flash.transform.localPosition = new Vector3(0f, 0f, 20f);
					exorcist.flash.gameObject.SetActive(true);
				}

				Color32 color = new Color(0f, 0.8f, 0f);

				exorcist.flash.enabled = true;

				hudManager.StartCoroutine(
					Effects.Lerp(1.0f, new Action<float>((p) =>
					{
						if (exorcist.flash == null)
						{
							return;
						}
						if (p < 0.5)
						{
							exorcist.flash.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));

						}
						else
						{
							exorcist.flash.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
						}
						if (p == 1f)
						{
							exorcist.flash.enabled = false;
						}
					}))
				);
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
		factory.CreateFloatOption(
			Option.Range,
			1.7f, 0.1f, 3.5f, 0.1f);
		factory.CreateBoolOption(Option.WithName, false);
		factory.CreateSelectionOption<Option, BlockMode>(Option.CrewBlockSystemType);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.withName = loader.GetValue<Option, bool>(Option.WithName);
		this.status = new ExorcistStatus(
			this,
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
			s.AddCondtion((int)ButtonLockSystem.ConditionId.Exorcist, exorcistBlockCondition);
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
			s.RpcLock(ButtonLockSystem.Ops.Lock, (int)ButtonLockSystem.ConditionId.Exorcist);
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
		=> this.target == this.status?.CurTarget;

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"悪魔祓い",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.TestButton),
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

	private void unlock()
	{
		foreach (var s in this.lockSystem)
		{
			s.RpcLock(ButtonLockSystem.Ops.Unlock, (int)ButtonLockSystem.ConditionId.Exorcist);
		}
	}

	private static bool exorcistBlockCondition()
	{
		if (PlayerControl.LocalPlayer == null ||
			!RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return true;
		}
		var role = ExtremeRoleManager.GetLocalPlayerRole();
		return
			(role.IsCrewmate() && role.Core.Id is not ExtremeRoleId.Exorcist) ||
			// もしくはサーヴァント + エクソ
			!(
				ExtremeRoleManager.TryGetSafeCastedLocalRole<ServantRole>(out var servant) && 
				servant.AnotherRole != null &&
				servant.AnotherRole.Core.Id is ExtremeRoleId.Exorcist
			);
	}
}
