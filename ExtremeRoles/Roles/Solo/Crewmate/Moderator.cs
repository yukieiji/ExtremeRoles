using AmongUs.GameOptions;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Moderator :
	SingleRoleBase,
	IRoleAbility,
	IRoleAwake<RoleTypes>
{
	public enum ModeratorOption
	{
		AwakeTaskGage,
		MeetingTimerOffset
	}

	public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

	public bool IsAwake
	{
		get
		{
			return GameSystem.IsLobby || this.awakeRole;
		}
	}

	public ExtremeAbilityButton? Button { get; set; }

	private TextPopUpper? textPopUp;

	private float offset = 0.0f;

	private bool awakeRole;
	private float awakeTaskGage;
	private bool awakeHasOtherVision;

	public Moderator() : base(
		ExtremeRoleId.Moderator,
		ExtremeRoleType.Crewmate,
		ExtremeRoleId.Moderator.ToString(),
		ColorPalette.OpenerSpringGreen,
		false, true, false, false)
	{ }

	public override string GetColoredRoleName(bool isTruthColor = false)
	{
		if (isTruthColor || IsAwake)
		{
			return base.GetColoredRoleName();
		}
		else
		{
			return Design.ColoedString(
				Palette.White,
				Translation.GetString(RoleTypes.Crewmate.ToString()));
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

	public void Update(PlayerControl rolePlayer)
	{
		if (!this.awakeRole)
		{
			if (Player.GetPlayerTaskGage(rolePlayer) >= this.awakeTaskGage)
			{
				this.awakeRole = true;
				this.HasOtherVision = this.awakeHasOtherVision;
			}
		}
	}

	public string GetFakeOptionString() => "";

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"moderate",
			Loader.CreateSpriteFromResources(
				Path.OpenerOpenDoor));
		this.Button?.SetLabelToCrewmate();
	}

	public bool UseAbility()
	{
		ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(ExtremeSystemType.MeetingTimeOffset,
			(x) =>
			{
				x.Write((byte)MeetingTimeChangeSystem.Ops.ChangeMeetingHudTempOffset);
				x.Write(this.offset);
			});

		this.textPopUp?.AddText(
			string.Format(
				Translation.GetString("changeMeetingTime"),
				this.offset));

		return true;
	}

	public bool IsAbilityUse() => this.IsCommonUse();

	public void ResetOnMeetingStart()
	{
		return;
	}

	public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
	{
		return;
	}

	protected override void CreateSpecificOption(
		IOptionInfo parentOps)
	{
		CreateIntOption(
			ModeratorOption.AwakeTaskGage,
			60, 0, 100, 10,
			parentOps,
			format: OptionUnit.Percentage);
		this.CreateAbilityCountOption(
			parentOps, 1, 5);
		CreateFloatOption(ModeratorOption.MeetingTimerOffset, 30.0f, 5.0f, 360f, 1.0f, parentOps);
	}

	protected override void RoleSpecificInit()
	{
		this.textPopUp = new TextPopUpper(
			3, 10.0f,
			new Vector3(-3.75f, -2.5f, -250.0f),
			TMPro.TextAlignmentOptions.BottomLeft);

		this.awakeTaskGage = OptionManager.Instance.GetValue<int>(
		   GetRoleOptionId(ModeratorOption.AwakeTaskGage)) / 100.0f;

		this.awakeHasOtherVision = this.HasOtherVision;

		if (this.awakeTaskGage <= 0.0f)
		{
			this.awakeRole = true;
			this.HasOtherVision = this.awakeHasOtherVision;
		}
		else
		{
			this.awakeRole = false;
			this.HasOtherVision = false;
		}

		this.offset = OptionManager.Instance.GetValue<float>(
			this.GetRoleOptionId(ModeratorOption.MeetingTimerOffset));
		this.RoleAbilityInit();
	}
}
