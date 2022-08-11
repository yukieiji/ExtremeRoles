using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

using BepInEx.IL2CPP.Utils.Collections;


namespace ExtremeRoles.Roles.Solo.Neutral
{
    public sealed class Yoko : SingleRoleBase, IRoleUpdate, IRoleResetMeeting, IRoleWinPlayerModifier
    {
        public enum YokoOption
        {
            CanRepairSabo,
            CanUseVent,
            SearchRange,
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
            ref List<GameData.PlayerInfo> pulsWinner)
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

        public override bool IsSameTeam(SingleRoleBase targetRole) =>
            this.IsNeutralSameTeam(targetRole);

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateBoolOption(
                YokoOption.CanRepairSabo,
                false, parentOps);
            CreateBoolOption(
                YokoOption.CanUseVent,
                false, parentOps);
            CreateFloatOption(
                YokoOption.SearchRange,
                7.5f, 5.0f, 15.0f, 0.5f,
                parentOps);
            CreateFloatOption(
                YokoOption.SearchTime,
                10f, 3.0f, 30f, 0.5f,
                parentOps,
                format: OptionUnit.Second);
            CreateIntOption(
                YokoOption.TrueInfoRate,
                50, 25, 80, 5, parentOps,
                format: OptionUnit.Percentage);
        }
        protected override void RoleSpecificInit()
        {
            this.CanRepairSabotage = OptionHolder.AllOption[
                GetRoleOptionId(YokoOption.CanRepairSabo)].GetValue();
            this.UseVent = OptionHolder.AllOption[
                GetRoleOptionId(YokoOption.CanUseVent)].GetValue();
            this.searchRange = OptionHolder.AllOption[
                GetRoleOptionId(YokoOption.SearchRange)].GetValue();
            this.searchTime = OptionHolder.AllOption[
                GetRoleOptionId(YokoOption.SearchTime)].GetValue();
            this.trueInfoGage = OptionHolder.AllOption[
                GetRoleOptionId(YokoOption.TrueInfoRate)].GetValue();
            this.timer = this.searchTime;
        }
        public void ResetOnMeetingEnd()
        {
            return;
        }

        public void ResetOnMeetingStart()
        {
            if (this.tellText != null)
            {
                this.tellText.gameObject.SetActive(false);
            }
        }
        public void Update(PlayerControl rolePlayer)
        {

            if (CachedShipStatus.Instance == null ||
                GameData.Instance == null) { return; }
            
            if (!CachedShipStatus.Instance.enabled ||
                MeetingHud.Instance != null ||
                ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger) { return; }

            if (Minigame.Instance) { return; }

            if (this.timer > 0)
            {
                this.timer -= Time.deltaTime;
                return;
            }
            
            Vector2 truePosition = rolePlayer.GetTruePosition();
            
            this.timer = this.searchTime;
            bool isEnemy = false;
            
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                SingleRoleBase targetRole = ExtremeRoleManager.GameRole[player.PlayerId];

                if (!player.Disconnected &&
                    (player.PlayerId != CachedPlayerControl.LocalPlayer.PlayerId) &&
                    !player.IsDead && !this.IsSameTeam(targetRole))
                {
                    PlayerControl @object = player.Object;
                    if (@object)
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= this.searchRange && this.isEnemy(targetRole))
                        {
                            isEnemy = true;
                            break;
                        }
                    }
                }
            }

            if (this.trueInfoGage <= RandomGenerator.Instance.Next(101))
            {
                isEnemy = !isEnemy;
            }

            string text = Helper.Translation.GetString("notFindEnemy");

            if (isEnemy)
            {
                text = Helper.Translation.GetString("findEnemy");
            }

            rolePlayer.StartCoroutine(
                showText(text).WrapToIl2Cpp());
        }

        private IEnumerator showText(string text)
        {
            if (this.tellText == null)
            {
                this.tellText = Object.Instantiate(
                    Prefab.Text, Camera.main.transform, false);
                this.tellText.transform.localPosition = new Vector3(0.0f, -0.9f, -250.0f);
                this.tellText.alignment = TMPro.TextAlignmentOptions.Center;
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
                role.CanKill() || 
                role.Id == ExtremeRoleId.Fencer)
            {
                return true;
                
            }
            else if (this.isYoko(role))
            {
                return true;
            }
            return false;
        }
        private bool isYoko(SingleRoleBase targetRole)
        {
            var multiAssignRole = targetRole as MultiAssignRoleBase;

            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    return this.isYoko(multiAssignRole.AnotherRole);
                }
            }
            return targetRole.Id == ExtremeRoleId.Yoko;
        }
    }
}
