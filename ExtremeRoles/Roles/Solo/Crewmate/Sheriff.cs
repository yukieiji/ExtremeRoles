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
            if ((targetPlayerRole.Team == ExtremeRoleType.Impostor) || 
                (targetPlayerRole.Team == ExtremeRoleType.Neutral && this.canShootNeutral))
            {
                updateKillButton();
                return true;
            }
            else
            {

                rolePlayer.RpcMurderPlayer(rolePlayer);
                ExtremeRolesPlugin.GameDataStore.ReplaceDeadReason(
                    rolePlayer.PlayerId, GameDataContainer.PlayerStatus.MissShot);
                return false;
            }

        }

        public void Update(PlayerControl rolePlayer)
        {
            if (this.killCountText != null) { return; }
            createText();
        }

        private void createText()
        {
            this.killCountText = UnityEngine.Object.Instantiate(
                HudManager.Instance.KillButton.cooldownTimerText,
                HudManager.Instance.KillButton.cooldownTimerText.transform.parent);
            this.killCountText.enableWordWrapping = false;
            this.killCountText.transform.localScale = Vector3.one * 0.5f;
            this.killCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);
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
                1, 1, OptionsHolder.VanillaMaxPlayerNum - 1, 1, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.shootNum = OptionsHolder.AllOption[
                GetRoleOptionId((int)SheriffOption.ShootNum)].GetValue();
            this.canShootNeutral = OptionsHolder.AllOption[
                GetRoleOptionId((int)SheriffOption.CanShootNeutral)].GetValue();
        }

        private void updateKillButton()
        {
            this.shootNum = this.shootNum - 1;
            if (this.shootNum == 0)
            {
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
