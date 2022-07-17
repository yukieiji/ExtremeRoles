using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
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
            IsSyncTaskAndShootNum
        }

        private int shootNum;
        private int maxShootNum;
        private bool canShootNeutral;
        private bool canShootAssassin;

        private bool enableTaskRelatedSetting;
        private float prevGage;
        private float reduceKillCool;
        private bool isPerm;
        private bool isSyncTaskShootNum;

        private TMPro.TextMeshPro killCountText = null;

        public Sheriff() : base(
            ExtremeRoleId.Sheriff,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Sheriff.ToString(),
            ColorPalette.SheriffOrange,
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
                if ((!this.canShootAssassin && targetPlayerRole.Id == ExtremeRoleId.Assassin) ||
                    targetPlayerRole.Id == ExtremeRoleId.Villain)
                {
                    missShoot(
                        rolePlayer,
                        GameDataContainer.PlayerStatus.Retaliate);
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
                    GameDataContainer.PlayerStatus.MissShot);
                return false;
            }

        }

        public override string GetImportantText(bool isContainFakeTask = true)
        {
            string shotText = Design.ColoedString(
                Palette.ImpostorRed,
                Translation.GetString("impostorShotCall"));

            if (this.canShootNeutral)
            {
                shotText = string.Concat(
                    shotText,
                    Design.ColoedString(
                        this.NameColor,
                        Translation.GetString("andFirst")),
                    Design.ColoedString(
                        ColorPalette.NeutralColor,
                        Translation.GetString("neutralShotCall")));
            }

            string baseString = string.Format("{0}: {1}{2}",
                this.GetColoredRoleName(),
                shotText,
                Design.ColoedString(
                    this.NameColor,
                    Translation.GetString(
                        $"{this.Id}ShortDescription")));

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

                if (gage > this.prevGage)
                {

                    rolePlayer.killTimer = Mathf.Clamp(
                        rolePlayer.killTimer - this.reduceKillCool,
                        0.01f, this.KillCoolTime);

                    if (this.isPerm)
                    {
                        if (!this.HasOtherKillCool)
                        {
                            this.HasOtherKillCool = true;
                            this.KillCoolTime = PlayerControl.GameOptions.KillCooldown;
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
                    }

                }
                this.prevGage = gage;
            }
        }

        private void createText()
        {
            this.killCountText = GameObject.Instantiate(
                FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText.transform.parent);
            updateKillCountText();
            this.killCountText.enableWordWrapping = false;
            this.killCountText.transform.localScale = Vector3.one * 0.5f;
            this.killCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);
            this.killCountText.gameObject.SetActive(true);
        }


        protected override void CreateSpecificOption(
            IOption parentOps)
        {

            CreateBoolOption(
                SheriffOption.CanShootAssassin,
                false, parentOps);

            CreateBoolOption(
                SheriffOption.CanShootNeutral,
                true, parentOps);

            CreateIntOption(
                SheriffOption.ShootNum,
                1, 1, OptionHolder.VanillaMaxPlayerNum - 1, 1,
                parentOps, format: OptionUnit.Shot);

            var enableTaskRelatedOps = CreateBoolOption(
                SheriffOption.EnableTaskRelated,
                false, parentOps);

            CreateFloatOption(
                SheriffOption.ReduceCurKillCool,
                2.0f, 1.0f, 5.0f,
                0.1f, enableTaskRelatedOps,
                format:OptionUnit.Second);

            CreateBoolOption(
                SheriffOption.IsPerm,
                false, enableTaskRelatedOps);

            CreateBoolOption(
                SheriffOption.IsSyncTaskAndShootNum,
                false, enableTaskRelatedOps);

        }

        protected override void RoleSpecificInit()
        {
            this.shootNum = OptionHolder.AllOption[
                GetRoleOptionId(SheriffOption.ShootNum)].GetValue();
            this.canShootNeutral = OptionHolder.AllOption[
                GetRoleOptionId(SheriffOption.CanShootNeutral)].GetValue();
            this.canShootAssassin = OptionHolder.AllOption[
                GetRoleOptionId(SheriffOption.CanShootAssassin)].GetValue();
            this.killCountText = null;

            this.enableTaskRelatedSetting = OptionHolder.AllOption[
                GetRoleOptionId(SheriffOption.EnableTaskRelated)].GetValue();
            this.reduceKillCool = OptionHolder.AllOption[
                GetRoleOptionId(SheriffOption.ReduceCurKillCool)].GetValue();
            this.isPerm = OptionHolder.AllOption[
                GetRoleOptionId(SheriffOption.IsPerm)].GetValue();
            this.isSyncTaskShootNum = OptionHolder.AllOption[
                GetRoleOptionId(SheriffOption.IsSyncTaskAndShootNum)].GetValue();
            this.prevGage = 0.0f;
            
            this.maxShootNum = this.shootNum;

            if (this.isSyncTaskShootNum)
            {
                this.shootNum = 0;
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetDisabled();
                this.CanKill = false;
            }
        }

        private void missShoot(
            PlayerControl rolePlayer,
            GameDataContainer.PlayerStatus replaceReson)
        {

            RPCOperator.Call(
                rolePlayer.NetId,
                RPCOperator.Command.UncheckedMurderPlayer,
                new List<byte>
                { 
                    rolePlayer.PlayerId,
                    rolePlayer.PlayerId,
                    byte.MaxValue 
                });
            RPCOperator.UncheckedMurderPlayer(
                rolePlayer.PlayerId,
                rolePlayer.PlayerId,
                byte.MaxValue);

            RPCOperator.Call(
                rolePlayer.NetId,
                RPCOperator.Command.ReplaceDeadReason,
                new List<byte>
                {
                    rolePlayer.PlayerId,
                    (byte)replaceReson
                });

            ExtremeRolesPlugin.GameDataStore.ReplaceDeadReason(
                rolePlayer.PlayerId, replaceReson);
        }


        private void updateKillButton()
        {

            this.shootNum = System.Math.Clamp(
                this.shootNum - 1, 0, this.maxShootNum);

            if (this.shootNum == 0)
            {
                this.killCountText.gameObject.SetActive(false);
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetDisabled();
                this.CanKill = false;
            }
            updateKillCountText();
        }
        private void updateKillCountText()
        {
            this.killCountText.text = Translation.GetString("buttonCountText") + string.Format(
                Translation.GetString(OptionUnit.Shot.ToString()), this.shootNum);
        }

        public void ResetOnMeetingEnd()
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
}
