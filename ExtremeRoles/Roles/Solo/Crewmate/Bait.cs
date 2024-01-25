﻿using AmongUs.GameOptions;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Performance;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Extension.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Bait : SingleRoleBase, IRoleAwake<RoleTypes>
{
	public enum Option
	{
		AwakeTaskGage,
		DelayUntilForceReport,
		EnableBaitBenefit,
		KillCoolReduceMulti,
		ReduceTimer
	}

	public bool IsAwake
	{
		get
		{
			return GameSystem.IsLobby || this.awakeRole;
		}
	}

	public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

	private float awakeTaskGage;
	private float delayUntilForceReport;

	private bool enableBaitBenefit;
	private float killCoolReduceMulti;
	private float timer;

	private bool awakeRole;

    public Bait() : base(
        ExtremeRoleId.Bait,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Bait.ToString(),
        ColorPalette.BaitCyan,
        false, true, false, false)
    { }

	public void Update(PlayerControl rolePlayer)
	{
		if (this.awakeRole) { return; }

		float taskGage = Player.GetPlayerTaskGage(rolePlayer);

		if (taskGage >= this.awakeTaskGage &&
			!this.awakeRole)
		{
			this.awakeRole = true;
		}
	}

	public string GetFakeOptionString() => "";

	public override string GetColoredRoleName(bool isTruthColor = false)
	{
		if (isTruthColor || IsAwake)
		{
			return base.GetColoredRoleName();
		}
		else
		{
			return Design.ColoedString(
				Palette.White, Translation.GetString(RoleTypes.Crewmate.ToString()));
		}
	}
	public override string GetFullDescription()
	{
		if (IsAwake)
		{
			return Translation.GetString(
				$"{this.Id}FullDescription");
		}
		else
		{
			return Translation.GetString(
				$"{RoleTypes.Crewmate}FullDescription");
		}
	}

	public override string GetImportantText(bool isContainFakeTask = true)
	{
		if (IsAwake)
		{
			return base.GetImportantText(isContainFakeTask);

		}
		else
		{
			return Design.ColoedString(
				Palette.White,
				$"{this.GetColoredRoleName()}: {Translation.GetString("crewImportantText")}");
		}
	}

	public override string GetIntroDescription()
	{
		if (IsAwake)
		{
			return base.GetIntroDescription();
		}
		else
		{
			return Design.ColoedString(
				Palette.CrewmateBlue,
				CachedPlayerControl.LocalPlayer.Data.Role.Blurb);
		}
	}

	public override Color GetNameColor(bool isTruthColor = false)
	{
		if (isTruthColor || IsAwake)
		{
			return base.GetNameColor(isTruthColor);
		}
		else
		{
			return Palette.White;
		}
	}

	public override void RolePlayerKilledAction(
		PlayerControl rolePlayer,
		PlayerControl killerPlayer)
	{
		if (!IsAwake && MeetingHud.Instance != null)
		{
			return;
		}

		PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
		if (localPlayer.PlayerId == killerPlayer.PlayerId)
		{
			var baitReporter = FastDestroyableSingleton<HudManager>.Instance.gameObject.AddComponent<BaitDalayReporter>();
			baitReporter.StartReportTimer(
				this.NameColor, rolePlayer.Data,
				this.delayUntilForceReport);
		}

		var role = ExtremeRoleManager.GetLocalPlayerRole();
		if (!this.enableBaitBenefit || !role.CanKill()) { return; }

		var reducer = localPlayer.gameObject.TryAddComponent<BaitKillCoolReducer>();
		reducer.Timer = this.timer;
		reducer.ReduceMulti = this.killCoolReduceMulti;
	}

	protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
		CreateIntOption(
			Option.AwakeTaskGage,
			70, 0, 100, 10,
			parentOps,
			format: OptionUnit.Percentage);
		CreateFloatOption(
			Option.DelayUntilForceReport,
			5.0f, 0.0f, 30.0f, 0.5f,
			parentOps, format: OptionUnit.Second);
		CreateBoolOption(
			Option.EnableBaitBenefit,
			true, parentOps);
		CreateFloatOption(
			Option.KillCoolReduceMulti,
			2.0f, 1.1f, 5.0f, 0.1f, parentOps,
			format: OptionUnit.Multiplier);
		CreateFloatOption(
			Option.ReduceTimer,
			5.0f, 1.0f, 30.0f, 0.5f, parentOps,
			format: OptionUnit.Second);
	}

    protected override void RoleSpecificInit()
    {
		var allOpt = OptionManager.Instance;

		this.awakeTaskGage = allOpt.GetValue<int>(
			GetRoleOptionId(Option.AwakeTaskGage)) / 100.0f;
		this.delayUntilForceReport = allOpt.GetValue<float>(
			GetRoleOptionId(Option.DelayUntilForceReport));
		this.enableBaitBenefit = allOpt.GetValue<bool>(
			GetRoleOptionId(Option.EnableBaitBenefit));
		this.killCoolReduceMulti = allOpt.GetValue<float>(
			GetRoleOptionId(Option.KillCoolReduceMulti)) - 1.0f;
		this.timer = allOpt.GetValue<float>(
			GetRoleOptionId(Option.ReduceTimer));

		this.awakeRole = this.awakeTaskGage <= 0.0f;
	}
}