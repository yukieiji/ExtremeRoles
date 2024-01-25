using AmongUs.GameOptions;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Bait : SingleRoleBase, IRoleAwake<RoleTypes>
{
	public enum Option
	{
		AwakeTaskGage,
		DelayUntilForceReport
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

	private bool awakeRole;

	private SpriteRenderer rend;

    public Bait() : base(
        ExtremeRoleId.Bait,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Bait.ToString(),
        ColorPalette.BakaryWheatColor,
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

		flushOnKillerPlayer(killerPlayer);

		if (this.delayUntilForceReport == 0.0f)
		{
			killerPlayer.ReportDeadBody(rolePlayer.Data);
		}
		else
		{
			// ディレイの強制通報
		}
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
	}

    protected override void RoleSpecificInit()
    {
		var allOpt = OptionManager.Instance;

		this.awakeTaskGage = allOpt.GetValue<int>(
			GetRoleOptionId(Option.AwakeTaskGage)) / 100.0f;
		this.delayUntilForceReport = allOpt.GetValue<float>(
			GetRoleOptionId(Option.DelayUntilForceReport));

		this.awakeRole = this.awakeTaskGage <= 0.0f;

		ExtremeSystemTypeManager.Instance.TryAdd(
			ExtremeSystemType.BakeryReport);
	}

	private void flushOnKillerPlayer(PlayerControl killer)
	{
		if (killer.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
		{
			return;
		}

		var hudManager = FastDestroyableSingleton<HudManager>.Instance;

		if (this.rend == null)
		{
			this.rend = Object.Instantiate(
				hudManager.FullScreen, hudManager.transform);
			this.rend.transform.localPosition = new Vector3(0f, 0f, 20f);
			this.rend.gameObject.SetActive(true);
		}

		this.rend.enabled = true;

		hudManager.StartCoroutine(
			Effects.Lerp(1.0f, new System.Action<float>((p) =>
			{
				if (this.rend == null) { return; }

				float alpha = p < 0.5 ?
					Mathf.Clamp01(p * 2 * 0.75f) :
					Mathf.Clamp01((1 - p) * 2 * 0.75f);

				this.rend.color = new Color(
					this.NameColor.r, this.NameColor.g,
					this.NameColor.b, alpha);

				if (p == 1f)
				{
					this.rend.enabled = false;
				}
			}))
		);
	}
}
