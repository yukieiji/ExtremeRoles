﻿using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Yandere : SingleRoleBase, IRoleUpdate, IRoleMurderPlayerHock, IRoleResetMeeting, IRoleSpecialSetUp
    {
        public PlayerControl OneSidedLover = null;

        private bool hasOneSidedArrow = false;
        private Arrow oneSidedArrow = null;

        private int maxTargetNum = 0;

        private int targetKillReduceRate = 0;
        private float noneTargetKillMultiplier = 0;
        private float defaultKillCool;

        private bool isRunawayNextMeetingEnd;
        private bool isRunaway;

        private float timeLimit = 0f;
        private float timer = 0f;

        private float setTargetRange;
        private float setTargetTime;

        private KillTarget target;

        private Dictionary<byte, float> progress = new Dictionary<byte, float>();

        public class KillTarget
        {
            private bool isUseArrow;

            private Dictionary<byte, Arrow> targetArrow = new Dictionary<byte, Arrow>();
            private Dictionary<byte, PlayerControl> targetPlayer = new Dictionary<byte, PlayerControl>();

            public KillTarget(
                bool isUseArrow)
            {
                this.isUseArrow = isUseArrow;
                this.targetArrow.Clear();
                this.targetPlayer.Clear();
            }

            public void Add(byte playerId)
            {

                var player = Helper.Player.GetPlayerControlById(playerId);

                this.targetPlayer.Add(playerId, player);
                if (this.isUseArrow)
                {
                    this.targetArrow.Add(
                        playerId, new Arrow(
                            new Color32(
                                byte.MaxValue,
                                byte.MaxValue,
                                byte.MaxValue,
                                byte.MinValue)));
                }
                else
                {
                    this.targetArrow.Add(playerId, null);
                }
            }

            public void ArrowActivate(bool active)
            {
                foreach (var arrow in this.targetArrow.Values)
                {
                    if (arrow != null)
                    {
                        arrow.SetActive(active);
                    }
                }
            }

            public bool IsContain(byte playerId) => targetPlayer.ContainsKey(playerId);

            public int Count() => this.targetPlayer.Count;

            public void Remove(byte playerId)
            {
                this.targetPlayer.Remove(playerId);

                if (this.targetArrow[playerId] != null)
                {
                    this.targetArrow[playerId].Clear();
                }

                this.targetArrow.Remove(playerId);
            }

            public void Update()
            {
                List<byte> remove = new List<byte>();

                foreach (var (playerId, playerControl) in this.targetPlayer)
                {
                    if (playerControl.Data.Disconnected || playerControl.Data.IsDead)
                    {
                        remove.Add(playerId);
                    }

                    if (this.targetArrow[playerId] != null && this.isUseArrow)
                    {
                        this.targetArrow[playerId].UpdateTarget(
                            playerControl.GetTruePosition());
                    }
                }

                foreach (var playerId in remove)
                {
                    this.Remove(playerId);
                }
            }
        }

        public enum YandereOption
        {
            TargetKilledKillCoolReduceRate,
            NoneTargetKilledKillCoolMultiplier,
            SetTargetRange,
            SetTargetTime,
            MaxTargetNum,
            RunawayTime,
            HasOneSidedArrow,
            HasTargetArrow,
        }

        public Yandere(): base(
            ExtremeRoleId.Yandere,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Yandere.ToString(),
            ColorPalette.YandereVioletRed,
            false, false, true, false)
        { }

        public override bool TryRolePlayerKillTo(
            PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            if (this.target.IsContain(targetPlayer.PlayerId))
            {
                this.KillCoolTime = this.KillCoolTime * (
                    (100f - this.targetKillReduceRate) / 100f);
            }
            else
            {
                this.KillCoolTime = this.KillCoolTime * this.noneTargetKillMultiplier;
            }

            return true;
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

        public override string GetIntroDescription() => string.Format(
            base.GetIntroDescription(),
            this.OneSidedLover.Data.PlayerName);

        public override string GetImportantText(
            bool isContainFakeTask = true) => string.Format(
                base.GetImportantText(isContainFakeTask),
                this.OneSidedLover.Data.PlayerName);


        public void Update(PlayerControl rolePlayer)
        {

            if (this.progress.ContainsKey(rolePlayer.PlayerId))
            {
                this.progress.Remove(rolePlayer.PlayerId);
            }

            if (Minigame.Instance != null ||
                ShipStatus.Instance == null ||
                GameData.Instance == null ||
                MeetingHud.Instance != null)
            {
                return;
            }
            if (!ShipStatus.Instance.enabled ||
                ExtremeRolesPlugin.GameDataStore.AssassinMeetingTrigger)
            {
                return;
            }

            var playerInfo = GameData.Instance.GetPlayerById(
               rolePlayer.PlayerId);
            if (playerInfo.IsDead || playerInfo.Disconnected) { return; }

            Vector2 oneSideLoverPos = this.OneSidedLover.GetTruePosition();

            // 片思いびとが生きてる時の処理
            if (!this.OneSidedLover.Data.Disconnected && !this.OneSidedLover.Data.IsDead)
            {
                searchTarget(rolePlayer, oneSideLoverPos);
            }
            else
            {
                this.isRunaway = true;
            }
            
            updateOneSideLoverArrow(oneSideLoverPos);
            this.target.Update();

            updateCanKill();

            checkRunawayNextMeeting();
        }

        public void HockMuderPlayer(
            PlayerControl source, PlayerControl target)
        {
            if (this.target.IsContain(target.PlayerId))
            {
                this.target.Remove(target.PlayerId);
            }
        }

        public void IntroBeginSetUp()
        {

            int playerIndex;

            do
            {
                playerIndex = UnityEngine.Random.RandomRange(
                    0, PlayerControl.AllPlayerControls.Count - 1);

                this.OneSidedLover = PlayerControl.AllPlayerControls[playerIndex];

                var role = ExtremeRoleManager.GameRole[this.OneSidedLover.PlayerId];
                if (role.Id != ExtremeRoleId.Yandere)
                {
                    var multiAssignRole = role as MultiAssignRoleBase;
                    if (multiAssignRole != null)
                    {
                        if (multiAssignRole.AnotherRole != null)
                        {

                            if (multiAssignRole.AnotherRole.Id != ExtremeRoleId.Yandere)
                            {
                                break;
                            }
                        }
                    }
                }

            } while (true);

        }

        public void IntroEndSetUp()
        {
            foreach(var player in GameData.Instance.AllPlayers)
            {
                this.progress.Add(player.PlayerId, 0.0f);
            }
        }

        public void ResetOnMeetingEnd()
        {
            if(this.isRunawayNextMeetingEnd)
            {
                this.CanKill = true;
                this.isRunaway = true;
                this.isRunawayNextMeetingEnd = false;
            }
            this.target.ArrowActivate(true);
        }

        public void ResetOnMeetingStart()
        {
            this.KillCoolTime = this.defaultKillCool;
            this.CanKill = false;
            this.isRunaway = false;
            this.timer = 0f;

            this.target.ArrowActivate(false);
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId((int)YandereOption.TargetKilledKillCoolReduceRate),
                string.Concat(
                    this.RoleName,
                    YandereOption.TargetKilledKillCoolReduceRate.ToString()),
                85, 50, 99, 0.5f,
                parentOps, format: OptionUnit.Percentage);

            CustomOption.Create(
                GetRoleOptionId((int)YandereOption.NoneTargetKilledKillCoolMultiplier),
                string.Concat(
                    this.RoleName,
                    YandereOption.NoneTargetKilledKillCoolMultiplier.ToString()),
                1.2f, 1.0f, 2.0f, 0.1f,
                parentOps, format: OptionUnit.Multiplier);

            CustomOption.Create(
                GetRoleOptionId((int)YandereOption.SetTargetRange),
                string.Concat(
                    this.RoleName,
                    YandereOption.SetTargetRange.ToString()),
                1.8f, 0.5f, 5.0f, 0.1f,
                parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)YandereOption.SetTargetTime),
                string.Concat(
                    this.RoleName,
                    YandereOption.SetTargetTime.ToString()),
                2.0f, 0.1f, 7.5f, 0.1f,
                parentOps, format: OptionUnit.Second);

            CustomOption.Create(
                GetRoleOptionId((int)YandereOption.MaxTargetNum),
                string.Concat(
                    this.RoleName,
                    YandereOption.MaxTargetNum.ToString()),
                5, 1, 15, 1, parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)YandereOption.RunawayTime),
                string.Concat(
                    this.RoleName,
                    YandereOption.RunawayTime.ToString()),
                60.0f, 25.0f, 120.0f, 0.25f,
                parentOps, format: OptionUnit.Second);

            CustomOption.Create(
                 GetRoleOptionId((int)YandereOption.HasOneSidedArrow),
                 string.Concat(
                     this.RoleName,
                     YandereOption.HasOneSidedArrow.ToString()),
                 true, parentOps);

            CustomOption.Create(
                GetRoleOptionId((int)YandereOption.HasTargetArrow),
                 string.Concat(
                     this.RoleName,
                     YandereOption.HasTargetArrow.ToString()),
                 true, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionHolder.AllOption;


            this.setTargetRange = allOption[
                GetRoleOptionId((int)YandereOption.SetTargetRange)].GetValue();
            this.setTargetTime = allOption[
                GetRoleOptionId((int)YandereOption.SetTargetTime)].GetValue();

            this.targetKillReduceRate = allOption[
                GetRoleOptionId((int)YandereOption.TargetKilledKillCoolReduceRate)].GetValue();
            this.noneTargetKillMultiplier = allOption[
                GetRoleOptionId((int)YandereOption.NoneTargetKilledKillCoolMultiplier)].GetValue();
            
            this.maxTargetNum = allOption[
                GetRoleOptionId((int)YandereOption.MaxTargetNum)].GetValue();

            this.timeLimit = allOption[
                GetRoleOptionId((int)YandereOption.RunawayTime)].GetValue();

            this.hasOneSidedArrow = allOption[
                GetRoleOptionId((int)YandereOption.HasOneSidedArrow)].GetValue();
            this.target = new KillTarget(
                allOption[GetRoleOptionId((int)YandereOption.HasTargetArrow)].GetValue());

            this.progress.Clear();

            if (this.HasOtherKillCool)
            {
                this.defaultKillCool = this.KillCoolTime;
            }
            else
            {
                this.defaultKillCool = PlayerControl.GameOptions.KillCooldown;
                this.HasOtherKillCool = true;
            }
        }

        private void checkRunawayNextMeeting()
        {
            if (this.isRunaway || this.isRunawayNextMeetingEnd) { return; }

            if (this.target.Count() == 0)
            {
                this.timer += Time.fixedDeltaTime;
                if (this.timer >= this.timeLimit)
                {
                    this.isRunawayNextMeetingEnd = true;
                }
            }
            else
            {
                this.timer = 0.0f;
            }
        }

        private void searchTarget(
            PlayerControl rolePlayer,
            Vector2 pos)
        {
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];

                if (!this.progress.ContainsKey(playerInfo.PlayerId)) { continue; }

                float playerProgress = this.progress[playerInfo.PlayerId];

                if (!playerInfo.Disconnected &&
                    !playerInfo.IsDead && 
                    rolePlayer.PlayerId != playerInfo.PlayerId &&
                    !playerInfo.Object.inVent)
                {
                    PlayerControl @object = playerInfo.Object;
                    if (@object)
                    {
                        Vector2 vector = @object.GetTruePosition() - pos;
                        float magnitude = vector.magnitude;

                        if (magnitude <= this.setTargetRange &&
                            !PhysicsHelpers.AnyNonTriggersBetween(
                                pos, vector.normalized,
                                magnitude, Constants.ShipAndObjectsMask))
                        {
                            playerProgress += Time.fixedDeltaTime;
                        }
                        else
                        {
                            playerProgress = 0.0f;
                        }
                    }
                }
                else
                {
                    playerProgress = 0.0f;
                }

                if (playerProgress >= this.setTargetTime && 
                    !this.target.IsContain(playerInfo.PlayerId) &&
                    this.target.Count() < this.maxTargetNum)
                {
                    this.target.Add(playerInfo.PlayerId);
                    this.progress.Remove(playerInfo.PlayerId);
                }
                else
                {
                    this.progress[playerInfo.PlayerId] = playerProgress;
                }
            }
        }

        private void updateCanKill()
        {
            if (this.isRunaway) { return; }

            this.CanKill = this.target.Count() != 0;
        }

        private void updateOneSideLoverArrow(Vector2 pos)
        {

            if (!this.hasOneSidedArrow) { return; }

            if (this.oneSidedArrow != null)
            {
                this.oneSidedArrow = new Arrow(
                    this.NameColor);
            }
            this.oneSidedArrow.UpdateTarget(pos);
        }
    }
}
