using AmongUs.GameOptions;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Forestaller :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleAwake<RoleTypes>,
	IRoleResetMeeting
{
	public enum Option
	{
		AwakeTaskGage,
	}

	public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

	public bool IsAwake => GameSystem.IsLobby || this.awakeRole;

	public ExtremeAbilityButton? Button { get; set; }

	private bool awakeRole;
	private float awakeTaskGage;
	private bool awakeHasOtherVision;

	public Forestaller() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Forestaller,
			ColorPalette.ForestallerGreen))
	{ }

	public override string GetColoredRoleName(bool isTruthColor = false)
	{
		if (isTruthColor || IsAwake)
		{
			return base.GetColoredRoleName();
		}
		else
		{
			return Design.ColoredString(
				Palette.White,
				Tr.GetString(RoleTypes.Crewmate.ToString()));
		}
	}

	public override string GetFullDescription()
	{
		if (IsAwake)
		{
			return Tr.GetString($"{this.Core.Id}FullDescription");
		}
		else
		{
			return Tr.GetString($"{RoleTypes.Crewmate}FullDescription");
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
			return Design.ColoredString(
				Palette.White,
				$"{this.GetColoredRoleName()}: {Tr.GetString("crewImportantText")}");
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
			return Design.ColoredString(
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

	public void Update(PlayerControl rolePlayer)
	{
		if (!this.awakeRole)
		{
			if (Player.GetPlayerTaskGage(rolePlayer) >= this.awakeTaskGage)
			{
				this.awakeRole = true;
				this.HasOtherVision = this.awakeHasOtherVision;
				this.Button?.SetButtonShow(true);
			}
			else
			{
				this.Button?.SetButtonShow(false);
			}
		}
	}

	public string GetFakeOptionString() => "";

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"forestall",
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Forestaller),
			abilityOff: CleanUp,
			forceAbilityOff: CleanUp);
		this.Button?.SetLabelToCrewmate();
	}

	public bool UseAbility()
	{
		ForestallerSystem.StartForestall();
		return true;
	}

	public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

	public void CleanUp()
	{
		ForestallerSystem.EndForestall();
	}

	public void ResetOnMeetingStart()
	{
		CleanUp();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null) { }

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateIntOption(
			Option.AwakeTaskGage,
			70, 0, 100, 10,
			format: OptionUnit.Percentage);

		IRoleAbility.CreateAbilityCountOption(
			factory,
			defaultAbilityCount: 3,
			maxAbilityCount: 10,
			defaultActiveTime: 10.0f);
	}

	protected override void RoleSpecificInit()
	{
		var cate = this.Loader;

		this.awakeTaskGage = cate.GetValue<Option, int>(
			Option.AwakeTaskGage) / 100.0f;

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

		ExtremeSystemTypeManager.Instance.CreateOrGet<ForestallerSystem>(
			ForestallerSystem.Type);
	}
}
