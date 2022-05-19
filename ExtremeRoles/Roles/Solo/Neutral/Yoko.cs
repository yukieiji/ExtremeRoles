using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Yoko : SingleRoleBase, IRoleUpdate, IRoleWinPlayerModifier
    {
        public enum YokoOption
        {
            TellRange,
            SearchTime,
            TrueInfoRate,
        }

        private float searchRange;
        private float searchTime;
        private float timer;
        private int trueInfoGage;

        private TMPro.TextMeshPro tellText;

        private HashSet<ExtremeRoleId> noneEnemy = new HashSet<ExtremeRoleId>()
        {
            ExtremeRoleId.Villain,
            ExtremeRoleId.Vigilante,
            ExtremeRoleId.Missionary,
            ExtremeRoleId.Lover,
        };

        public Yoko() : base(
            ExtremeRoleId.Yoko,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Yoko.ToString(),
            ColorPalette.YokoShion,
            false, false, false, false,
            true, false, true, false, false)
        { }

        public void ModifiedWinPlayer(
            GameData.PlayerInfo rolePlayerInfo,
            GameOverReason reason,
            ref Il2CppSystem.Collections.Generic.List<WinningPlayerData> winner,
            ref List<PlayerControl> pulsWinner)
        {
            if (rolePlayerInfo.IsDead || rolePlayerInfo.Disconnected) { return; }

            switch ((RoleGameOverReason)reason)
            {
                case (RoleGameOverReason)GameOverReason.HumansByTask:
                case (RoleGameOverReason)GameOverReason.ImpostorBySabotage:
                case RoleGameOverReason.AssassinationMarin:
                    break;
                case RoleGameOverReason.YokoAllDeceive:
                    this.AddWinner(rolePlayerInfo, winner, pulsWinner);
                    break;
                default:
                    pulsWinner.Clear();
                    winner.Clear();
                    winner.Add(new WinningPlayerData(rolePlayerInfo));
                    ExtremeRolesPlugin.GameDataStore.EndReason = (GameOverReason)RoleGameOverReason.YokoAllDeceive;
                    break;
            }
        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            var multiAssignRole = targetRole as MultiAssignRoleBase;

            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.IsSameTeam(multiAssignRole.AnotherRole);
                }
            }
            if (OptionHolder.Ship.IsSameNeutralSameWin)
            {
                return this.Id == targetRole.Id;
            }
            else
            {
                return (this.Id == targetRole.Id) && this.IsSameControlId(targetRole);
            }
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CreateFloatOption(
                YokoOption.TellRange,
                15.0f, 7.0f, 30.0f, 0.5f,
                parentOps);
            CreateFloatOption(
                YokoOption.SearchTime,
                10f, 3.0f, 30f, 0.5f,
                parentOps);
            CreateIntOption(
                YokoOption.TrueInfoRate,
                50, 25, 80, 5, parentOps,
                format:OptionUnit.Percentage);
        }
        protected override void RoleSpecificInit()
        {
            this.searchRange = OptionHolder.AllOption[
                GetRoleOptionId(YokoOption.TellRange)].GetValue();
            this.searchTime = OptionHolder.AllOption[
                GetRoleOptionId(YokoOption.SearchTime)].GetValue();
            this.trueInfoGage = OptionHolder.AllOption[
                GetRoleOptionId(YokoOption.TrueInfoRate)].GetValue();
            this.timer = this.searchTime;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (ShipStatus.Instance == null ||
                GameData.Instance == null) { return; }
            if (!ShipStatus.Instance.enabled ||
                ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger) { return; }

            if (Minigame.Instance) { return; }

            if (this.timer > 0)
            {
                this.timer -= Time.deltaTime;
                return;
            }

            Vector2 truePosition = rolePlayer.GetTruePosition();
            this.timer = this.searchTime;
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                SingleRoleBase targetRole = ExtremeRoleManager.GameRole[player.PlayerId];

                if (!player.Disconnected &&
                    (player.PlayerId != PlayerControl.LocalPlayer.PlayerId) &&
                    !player.IsDead && !this.IsSameTeam(targetRole))
                {
                    PlayerControl @object = player.Object;
                    if (@object)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= this.searchRange && this.isEnemy(targetRole))
                        {
                            this.showText(Helper.Translation.GetString("findEnemy"));
                            return;
                        }
                    }
                }
            }
            this.showText(Helper.Translation.GetString("notFindEnemy"));
        }

        private IEnumerator showText(string text)
        {
            if (this.tellText == null)
            {
                this.tellText = Object.Instantiate(
                    Prefab.Text, Camera.main.transform, false);
                this.tellText.transform.localPosition = new Vector3(-4.0f, -2.75f, -250.0f);
                this.tellText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
                this.tellText.gameObject.layer = 5;
            }
            this.tellText.text = text;
            this.tellText.gameObject.SetActive(true);

            yield return new WaitForSeconds(3.5f);

            this.tellText.gameObject.SetActive(false);

        }
        private bool isEnemy(SingleRoleBase role)
        {

            if (this.noneEnemy.Contains(role.Id))
            {
                return false;
            }
            else if (
                role.IsImpostor() ||
                role.CanKill || role.Id == ExtremeRoleId.Fencer)
            {
                if (this.trueInfoGage > RandomGenerator.Instance.Next(101))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}
