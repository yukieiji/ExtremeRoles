using AmongUs.GameOptions;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Performance;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Extension.Il2Cpp;




using ExtremeRoles.Module.CustomOption.Factory;

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
	private bool awakeHasOtherVision;

	public Bait() : base(
        ExtremeRoleId.Bait,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Bait.ToString(),
        ColorPalette.BaitCyan,
        false, true, false, false)
    { }

	public static void Awake(byte playerId)
	{
		var bait = ExtremeRoleManager.GetSafeCastedRole<Bait>(playerId);
		if (bait == null) { return; }
		bait.awakeRole = true;
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (this.awakeRole) { return; }

		float taskGage = Player.GetPlayerTaskGage(rolePlayer);

		if (taskGage >= this.awakeTaskGage &&
			!this.awakeRole)
		{
			this.awakeRole = true;
			this.HasOtherVision = this.awakeHasOtherVision;

			using (var caller = RPCOperator.CreateCaller(
				RPCOperator.Command.BaitAwakeRole))
			{
				caller.WriteByte(rolePlayer.PlayerId);
			}
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
				PlayerControl.LocalPlayer.Data.Role.Blurb);
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
		if (!IsAwake || MeetingHud.Instance != null)
		{
			return;
		}

		PlayerControl localPlayer = PlayerControl.LocalPlayer;
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
        AutoParentSetOptionCategoryFactory factory)
    {
		factory.CreateIntOption(
			Option.AwakeTaskGage,
			70, 0, 100, 10,
			format: OptionUnit.Percentage);
		factory.CreateFloatOption(
			Option.DelayUntilForceReport,
			5.0f, 0.0f, 30.0f, 0.5f,
			format: OptionUnit.Second);
		factory.CreateBoolOption(
			Option.EnableBaitBenefit,
			true);
		factory.CreateFloatOption(
			Option.KillCoolReduceMulti,
			2.0f, 1.1f, 5.0f, 0.1f,
			format: OptionUnit.Multiplier);
		factory.CreateFloatOption(
			Option.ReduceTimer,
			5.0f, 1.0f, 30.0f, 0.5f,
			format: OptionUnit.Second);
	}

    protected override void RoleSpecificInit()
    {
		var loader = this.Loader;

		this.awakeTaskGage = loader.GetValue<Option, int>(
			Option.AwakeTaskGage) / 100.0f;
		this.delayUntilForceReport = loader.GetValue<Option, float>(
			Option.DelayUntilForceReport);
		this.enableBaitBenefit = loader.GetValue<Option, bool>(
			Option.EnableBaitBenefit);
		this.killCoolReduceMulti = loader.GetValue<Option, float>(
			Option.KillCoolReduceMulti) - 1.0f;
		this.timer = loader.GetValue<Option, float>(
			Option.ReduceTimer);

		this.awakeHasOtherVision = this.HasOtherVision;

		if (this.awakeTaskGage <= 0.0)
		{
			this.awakeRole = true;
			this.HasOtherVision = this.awakeHasOtherVision;
		}
		else
		{
			this.awakeRole = false;
			this.HasOtherVision = false;
		}
	}
}
