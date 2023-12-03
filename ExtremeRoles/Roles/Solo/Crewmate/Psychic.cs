using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

public sealed class Psychic :
    SingleRoleBase,
    IRoleAbility,
    IRoleAwake<RoleTypes>,
    IRoleReportHook
{

    public enum PsychicOption
    {
        AwakeTaskGage,
		AwakeDeadPlayerNum,
        IsUpgradeAbility,
		UpgradeTaskGage,
		UpgradeDeadPlayerNum,
	}
    public bool IsAwake
    {
        get
        {
            return GameSystem.IsLobby || this.awakeRole;
        }
    }

    public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;
    private bool awakeRole;
    private float awakeTaskGage;
	private int awakeDeadPlayerNum;

    private bool awakeHasOtherVision;

	private bool enableUpgrade;
	private float upgradeTaskGage;
	private int upgradeDeadPlayerNum;

	private SpriteRenderer? flash;
    public const float FlashTime = 1.0f;

#pragma warning disable CS8618
	public ExtremeAbilityButton Button { get; set; }

	public Psychic() : base(
        ExtremeRoleId.Photographer,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Photographer.ToString(),
        ColorPalette.PhotographerVerdeSiena,
        false, true, false, false)
    { }
#pragma warning restore CS8618

	public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "takePhoto",
            Loader.CreateSpriteFromResources(
                Path.PhotographerPhotoCamera));
        this.Button.SetLabelToCrewmate();
    }

    public bool UseAbility()
    {
        var hudManager = FastDestroyableSingleton<HudManager>.Instance;

        return true;
    }

    public bool IsAbilityUse()
        => this.IsAwake && this.IsCommonUse();

    public string GetFakeOptionString() => "";

    public void HookReportButton(
        PlayerControl rolePlayer, GameData.PlayerInfo reporter)
    {
        sendPhotoInfo();
    }

    public void HookBodyReport(
        PlayerControl rolePlayer,
        GameData.PlayerInfo reporter,
        GameData.PlayerInfo reportBody)
    {
        sendPhotoInfo();
    }

    public void ResetOnMeetingStart()
    {
        if (this.flash != null)
        {
            this.flash.enabled = false;
        }
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
    {
    }

    public void Update(PlayerControl rolePlayer)
    {
        float taskGage = Player.GetPlayerTaskGage(rolePlayer);
		int deadPlayerNum = 0;

		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (player == null || player.IsDead || player.Disconnected)
			{
				++deadPlayerNum;
			}
		}

        if (!this.awakeRole)
        {
            if (taskGage >= this.awakeTaskGage &&
				deadPlayerNum >= this.awakeDeadPlayerNum &&
				!this.awakeRole)
            {
                this.awakeRole = true;
                this.HasOtherVision = this.awakeHasOtherVision;
                this.Button.SetButtonShow(true);
            }
            else
            {
                this.Button.SetButtonShow(false);
            }
        }
    }

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

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        CreateIntOption(
            PsychicOption.AwakeTaskGage,
            30, 0, 100, 10,
            parentOps,
            format: OptionUnit.Percentage);
		CreateIntOption(
		   PsychicOption.AwakeDeadPlayerNum,
		   2, 0, 7, 1, parentOps);

        this.CreateAbilityCountOption(
            parentOps, 1, 5, 3.0f);

		var isUpgradeOpt = CreateBoolOption(
			PsychicOption.IsUpgradeAbility,
			false, parentOps);
		CreateIntOption(
			PsychicOption.AwakeTaskGage,
			70, 0, 100, 10,
			isUpgradeOpt,
			format: OptionUnit.Percentage);
		CreateIntOption(
		   PsychicOption.AwakeDeadPlayerNum,
		   5, 0, 10, 1, isUpgradeOpt);
	}

    protected override void RoleSpecificInit()
    {
        var allOpt = OptionManager.Instance;

        this.awakeTaskGage = allOpt.GetValue<int>(
            GetRoleOptionId(PsychicOption.AwakeTaskGage)) / 100.0f;
		this.awakeDeadPlayerNum = allOpt.GetValue<int>(
			GetRoleOptionId(PsychicOption.AwakeDeadPlayerNum));

		this.upgradeTaskGage = allOpt.GetValue<int>(
			GetRoleOptionId(PsychicOption.UpgradeTaskGage)) / 100.0f;
		this.upgradeDeadPlayerNum = allOpt.GetValue<int>(
			GetRoleOptionId(PsychicOption.UpgradeDeadPlayerNum));
		this.enableUpgrade = allOpt.GetValue<bool>(
			GetRoleOptionId(PsychicOption.IsUpgradeAbility));

		int maxPlayerNum = CachedPlayerControl.AllPlayerControls.Count - 1;

		this.awakeDeadPlayerNum = Mathf.Clamp(
			this.awakeDeadPlayerNum, 0, maxPlayerNum);
		this.awakeHasOtherVision = this.HasOtherVision;

		this.upgradeDeadPlayerNum = Mathf.Clamp(
			this.upgradeDeadPlayerNum, 0, maxPlayerNum);

		if (this.awakeTaskGage <= 0.0f && this.awakeDeadPlayerNum == 0)
        {
            this.awakeRole = true;
            this.HasOtherVision = this.awakeHasOtherVision;
        }
        else
        {
            this.awakeRole = false;
            this.HasOtherVision = false;
        }

		if (this.enableUpgrade && this.upgradeTaskGage <= 0.0f && this.upgradeDeadPlayerNum == 0)
		{

		}

        this.RoleAbilityInit();

    }

    private void sendPhotoInfo()
    {
        if (!this.IsAwake) { return; }


        HudManager hud = FastDestroyableSingleton<HudManager>.Instance;

        string chatText = "";

        MeetingReporter.Instance.AddMeetingChatReport(chatText);
    }
}
