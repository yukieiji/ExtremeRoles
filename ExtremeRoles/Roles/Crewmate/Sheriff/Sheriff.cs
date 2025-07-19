using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;


using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Sheriff : SingleRoleBase, IRoleUpdate, IRoleResetMeeting
{

    public enum SheriffOption
    {
        ShootNum,
        CanShootAssassin,
        CanShootNeutral,
        EnableTaskRelated,
        ReduceCurKillCool,
        IsPerm,
        IsSyncTaskAndShootNum,
        IsEnableShootTaskGageOption,
        SyncShootTaskGage
    }

    private int shootNum;
    private int maxShootNum;
    private bool canShootNeutral;
    private bool canShootAssassin;

    private bool enableTaskRelatedSetting;
    private float prevGage;
    private float reduceKillCool;
    private float syncShootTaskGage;
    private bool isPerm;
    private bool isSyncTaskShootNum;

    private TMPro.TextMeshPro killCountText = null;

    public Sheriff() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Sheriff,
			ColorPalette.SheriffOrange),
        true, true, false, false)
    { }

    public override bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        var targetPlayerRole = ExtremeRoleManager.GameRole[
            targetPlayer.PlayerId];


        if ((targetPlayerRole.IsImpostor()) ||
            (targetPlayerRole.IsNeutral() && this.canShootNeutral))
        {
			var id = targetPlayerRole.Core.Id;
            if ((!this.canShootAssassin && id is ExtremeRoleId.Assassin) ||
				id is ExtremeRoleId.Villain)
            {
                missShoot(
                    rolePlayer,
                    ExtremeShipStatus.PlayerStatus.Retaliate);
                return false;
            }
            else
            {
                updateKillButton();
                return true;
            }
        }
        else
        {

            missShoot(
                rolePlayer,
                ExtremeShipStatus.PlayerStatus.MissShot);
            return false;
        }

    }

    public override string GetImportantText(bool isContainFakeTask = true)
    {
        string shotText = Design.ColoedString(
            Palette.ImpostorRed,
            Tr.GetString("impostorShotCall"));

        if (this.canShootNeutral)
        {
            shotText = string.Concat(
                shotText,
                Design.ColoedString(
                    this.Core.Color,
                    Tr.GetString("andFirst")),
                Design.ColoedString(
                    ColorPalette.NeutralColor,
                    Tr.GetString("neutralShotCall")));
        }

        string baseString = string.Format("{0}: {1}{2}",
            this.GetColoredRoleName(),
            shotText,
            Design.ColoedString(
                this.Core.Color,
                Tr.GetString(
                    $"{this.Core.Id}ShortDescription")));

        return baseString;

    }

    public void Update(PlayerControl rolePlayer)
    {
        if (this.killCountText == null)
        {
            createText();
        }
        if (this.enableTaskRelatedSetting)
        {

            float gage = Player.GetPlayerTaskGage(rolePlayer);

            if (gage >= (this.prevGage + this.syncShootTaskGage))
            {
                if (this.CanKill)
                {
                    rolePlayer.killTimer = Mathf.Clamp(
                        rolePlayer.killTimer - this.reduceKillCool,
                        0.01f, this.KillCoolTime);
                }

                if (this.isPerm)
                {
                    if (!this.HasOtherKillCool)
                    {
                        this.HasOtherKillCool = true;
                        this.KillCoolTime = Player.DefaultKillCoolTime;
                    }
                    this.KillCoolTime = Mathf.Clamp(
                        this.KillCoolTime - this.reduceKillCool,
                        0.01f, this.KillCoolTime);
                }

                if (this.isSyncTaskShootNum)
                {
                    this.shootNum = System.Math.Clamp(
                        this.shootNum + 1, this.shootNum, this.maxShootNum);
                    this.CanKill = true;
                    updateKillCountText();
                    if (this.shootNum > 0)
                    {
                        this.killCountText.gameObject.SetActive(true);
                    }
                }
                this.prevGage = gage;
            }
        }
    }

    private void createText()
    {
		this.killCountText = ICountBehavior.CreateCountText(
			HudManager.Instance.KillButton);
        updateKillCountText();
        this.killCountText.name = ExtremeAbilityButton.AditionalInfoName;
		this.killCountText.gameObject.SetActive(true);
    }


    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateBoolOption(
            SheriffOption.CanShootAssassin,
            false);

        factory.CreateBoolOption(
            SheriffOption.CanShootNeutral,
            true);

        factory.CreateIntOption(
            SheriffOption.ShootNum,
            1, 1, GameSystem.VanillaMaxPlayerNum - 1, 1,
            format: OptionUnit.Shot);

        var enableTaskRelatedOps = factory.CreateBoolOption(
            SheriffOption.EnableTaskRelated,
            false);

        factory.CreateFloatOption(
            SheriffOption.ReduceCurKillCool,
            2.0f, 1.0f, 5.0f,
            0.1f, enableTaskRelatedOps,
            format:OptionUnit.Second);

        factory.CreateBoolOption(
            SheriffOption.IsPerm,
            false, enableTaskRelatedOps);

        var syncOpt = factory.CreateBoolOption(
            SheriffOption.IsSyncTaskAndShootNum,
            false, enableTaskRelatedOps);;
        factory.CreateIntOption(
            SheriffOption.SyncShootTaskGage,
            5, 5, 100, 1,
            syncOpt, format: OptionUnit.Percentage);
    }

    protected override void RoleSpecificInit()
    {

        var loader = this.Loader;

        this.shootNum = loader.GetValue<SheriffOption, int>(
            SheriffOption.ShootNum);
		this.canShootNeutral = loader.GetValue<SheriffOption, bool>(
			SheriffOption.CanShootNeutral);
        this.canShootAssassin = loader.GetValue<SheriffOption, bool>(
            SheriffOption.CanShootAssassin);
        this.killCountText = null;

        this.enableTaskRelatedSetting = loader.GetValue<SheriffOption, bool>(
            SheriffOption.EnableTaskRelated);
        this.reduceKillCool = loader.GetValue<SheriffOption, float>(
            SheriffOption.ReduceCurKillCool);
        this.isPerm = loader.GetValue<SheriffOption, bool>(
            SheriffOption.IsPerm);
        this.isSyncTaskShootNum = loader.GetValue<SheriffOption, bool>(
            SheriffOption.IsSyncTaskAndShootNum);
        this.syncShootTaskGage = loader.GetValue<SheriffOption, int>(
            SheriffOption.SyncShootTaskGage) / 100.0f;

        this.prevGage = 0.0f;

        this.maxShootNum = this.shootNum;

        if (this.enableTaskRelatedSetting && this.isSyncTaskShootNum)
        {
            this.shootNum = 0;
            HudManager.Instance.KillButton.SetDisabled();
            this.CanKill = false;
        }
    }

    private void missShoot(
        PlayerControl rolePlayer,
        ExtremeShipStatus.PlayerStatus replaceReson)
    {

        Player.RpcUncheckMurderPlayer(
            rolePlayer.PlayerId,
            rolePlayer.PlayerId,
            byte.MaxValue);

        ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
            rolePlayer.PlayerId,
            replaceReson);
    }

    private void updateKillButton()
    {

        this.shootNum = System.Math.Clamp(
            this.shootNum - 1, 0, this.maxShootNum);

        if (this.shootNum == 0)
        {
            this.killCountText.gameObject.SetActive(false);
            HudManager.Instance.KillButton.SetDisabled();
            this.CanKill = false;
        }
        updateKillCountText();
    }
    private void updateKillCountText()
    {
        this.killCountText.text = Tr.GetString(
            ICountBehavior.DefaultButtonCountText,
			this.shootNum);
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        if (this.killCountText != null)
        {
            this.killCountText.gameObject.SetActive(true);
        }
    }

    public void ResetOnMeetingStart()
    {
        if (this.killCountText != null)
        {
            this.killCountText.gameObject.SetActive(false);
        }
    }
}
