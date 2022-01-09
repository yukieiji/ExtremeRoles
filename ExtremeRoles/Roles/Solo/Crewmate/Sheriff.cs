using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Sheriff : SingleRoleBase, IRoleUpdate
    {

        public enum SheriffOption
        {
            ShootNum,
            CanShootNeutral
        }

        private int shootNum;
        private bool canShootNeutral;

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
            if ((targetPlayerRole.IsImposter()) || 
                (targetPlayerRole.IsNeutral() && this.canShootNeutral))
            {
                updateKillButton();
                return true;
            }
            else
            {

                rolePlayer.RpcMurderPlayer(rolePlayer);

                RPCOperator.Call(
                    rolePlayer.NetId,
                    RPCOperator.Command.ReplaceDeadReason,
                    new List<byte>
                    {
                        rolePlayer.PlayerId,
                        (byte)GameDataContainer.PlayerStatus.MissShot
                    });

                ExtremeRolesPlugin.GameDataStore.ReplaceDeadReason(
                    rolePlayer.PlayerId, GameDataContainer.PlayerStatus.MissShot);
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
                shotText = string.Format("{0}{1}{2}",
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
                        string.Format("{0}{1}", this.Id, "ShortDescription"))));

            return baseString;

        }

        public void Update(PlayerControl rolePlayer)
        {
            if (this.killCountText == null)
            {
                createText();
            }
        }

        private void createText()
        {
            this.killCountText = GameObject.Instantiate(
                HudManager.Instance.KillButton.cooldownTimerText,
                HudManager.Instance.KillButton.cooldownTimerText.transform.parent);
            updateKillCountText();
            this.killCountText.enableWordWrapping = false;
            this.killCountText.transform.localScale = Vector3.one * 0.5f;
            this.killCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);
            this.killCountText.gameObject.SetActive(true);
        }


        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId((int)SheriffOption.CanShootNeutral),
                Design.ConcatString(
                    this.RoleName,
                    SheriffOption.CanShootNeutral.ToString()),
                true, parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)SheriffOption.ShootNum),
                Design.ConcatString(
                    this.RoleName,
                    SheriffOption.ShootNum.ToString()),
                1, 1, OptionHolder.VanillaMaxPlayerNum - 1, 1, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.shootNum = OptionHolder.AllOption[
                GetRoleOptionId((int)SheriffOption.ShootNum)].GetValue();
            this.canShootNeutral = OptionHolder.AllOption[
                GetRoleOptionId((int)SheriffOption.CanShootNeutral)].GetValue();
            this.killCountText = null;
        }

        private void updateKillButton()
        {
            this.shootNum = this.shootNum - 1;
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
            this.killCountText.text = Translation.GetString("buttonCountText") + string.Format(
                Translation.GetString("unitShots"), this.shootNum);
        }
    }
}
