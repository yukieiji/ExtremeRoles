using System.Collections.Generic;
using System.Linq;

using Hazel;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using AmongUs.GameOptions;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class SlaveDriver : SingleRoleBase, IRoleUpdate, IRoleResetMeeting, IRoleMurderPlayerHook
    {
  
        private List<(byte, List<int>)> specialAttackResult;

        private int noneTaskPlayerAttakBonusChance;
        private int noneTaskPlayerSpecialAttackChance;

        private int setNormalTaskNum;
        private int setLongTaskNum;
        private int setCommonTaskNum;

        private float reduceRate;
        private float specialAttackReduceRate;
        private float defaultKillCoolTime;
        private float noneBonusKillTaskRange;
        private float aliveCheckTime;
        private float specialAttackStartTime;
        private float timer;

        private byte rolePlayerId;
        private byte specialAttackPlayerId;

        public enum SlaveDriverOption
        {
            TaskProgressRange,
            KillCoolReduceRate,
            SpecialAttackTimer,
            SpecialAttackKillCoolReduceRate,
            SpecialAttackAliveTimer,
            NoneTaskPlayerAttakBonusChance,
            NoneTaskPlayerSpecialAttackChance,
            AdditionalNormalTaskNum,
            AdditionalLongTaskNum,
            AdditionalCommonTaskNum,
        }

        public SlaveDriver() : base(
            ExtremeRoleId.SlaveDriver,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.SlaveDriver.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }


        public override bool TryRolePlayerKillTo(
            PlayerControl rolePlayer,
            PlayerControl targetPlayer)
        {

            this.rolePlayerId = rolePlayer.PlayerId;

            var targetRole = ExtremeRoleManager.GameRole[targetPlayer.PlayerId];

            this.specialAttackPlayerId = byte.MaxValue;
            this.KillCoolTime = this.defaultKillCoolTime;

            if (targetRole.HasTask())
            {
                int targetPlayerTaskNum = targetPlayer.Data.Tasks.Count;
                int targetPlayerCompTask = 0;

                foreach (var task in targetPlayer.Data.Tasks.GetFastEnumerator())
                {
                    if (task.Complete)
                    {
                        ++targetPlayerCompTask;
                    }
                }

                int taskHasPlayerNum = 0;
                int taskHasDeadPlayerNum = 0;

                foreach (var playerInfo in 
                    GameData.Instance.AllPlayers.GetFastEnumerator())
                {
                    var role = ExtremeRoleManager.GameRole[playerInfo.PlayerId];

                    if (!playerInfo.Disconnected && role.HasTask())
                    {
                        ++taskHasPlayerNum;

                        if (playerInfo.IsDead)
                        {
                            ++taskHasDeadPlayerNum;
                        }
                    }
                }

                float approximateTaskGage = (float)taskHasDeadPlayerNum / (float)taskHasPlayerNum;
                float targetPlayerTaskGage = (float)targetPlayerCompTask / (float)targetPlayerTaskNum;

                int totalTaskNum = GameData.Instance.TotalTasks;
                int compTaskNum = GameData.Instance.CompletedTasks;
                float totalTaskGauge = (float)compTaskNum / (float)totalTaskNum;

                float diff = totalTaskGauge - targetPlayerTaskGage;

                // ゲーム開始時から一定時間経過後、全体のタスク進捗が生存者に対して少ない(この時、誤差許容は行わない)
                if (approximateTaskGage > totalTaskGauge &&
                    this.timer >= this.aliveCheckTime)
                {
                    setSpecialAttackPlayerKillCool();
                    this.specialAttackPlayerId = targetPlayer.PlayerId;
                }
                // ゲーム開始時から一定時間経過後、全体のタスク進捗から大きく離れてる(50％以上) or タスク完了数が0～2
                else if ((0.5f < diff || (0 <= targetPlayerCompTask && targetPlayerCompTask <= 2)) && 
                    this.timer >= this.specialAttackStartTime)
                {
                    setSpecialAttackPlayerKillCool();
                    this.specialAttackPlayerId = targetPlayer.PlayerId;
                }
                else if (totalTaskGauge > (targetPlayerTaskGage + this.noneBonusKillTaskRange))
                {
                    setBonusPlayerKillCool();
                }
            }
            else
            {
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < this.noneTaskPlayerSpecialAttackChance)
                {
                    setSpecialAttackPlayerKillCool();
                }
                else if (chance < this.noneTaskPlayerAttakBonusChance)
                {
                    setBonusPlayerKillCool();
                }
            }

            return true;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {

            CreateIntOption(
                SlaveDriverOption.TaskProgressRange,
                10, 5, 20, 1, parentOps,
                format: OptionUnit.Percentage);

            CreateIntOption(
                SlaveDriverOption.KillCoolReduceRate,
                25, 1, 50, 1, parentOps,
                format: OptionUnit.Percentage);

            CreateFloatOption(
                SlaveDriverOption.SpecialAttackTimer,
                60f, 30f, 120f, 0.5f, parentOps,
                format: OptionUnit.Second);

            CreateFloatOption(
                SlaveDriverOption.SpecialAttackAliveTimer,
                600f, 300f, 900f, 30.0f, parentOps,
                format: OptionUnit.Second);

            CreateIntOption(
                SlaveDriverOption.SpecialAttackKillCoolReduceRate,
                50, 25, 75, 1, parentOps,
                format: OptionUnit.Percentage);

            CreateIntOption(
                SlaveDriverOption.NoneTaskPlayerAttakBonusChance,
                50, 20, 100, 1, parentOps,
                format: OptionUnit.Percentage);

            CreateIntOption(
                SlaveDriverOption.NoneTaskPlayerSpecialAttackChance,
                10, 5, 25, 1, parentOps,
                format: OptionUnit.Percentage);

            CreateIntOption(
                SlaveDriverOption.AdditionalCommonTaskNum,
                1, 0, 5, 1, parentOps);

            CreateIntOption(
                SlaveDriverOption.AdditionalNormalTaskNum,
                1, 0, 5, 1, parentOps);

            CreateIntOption(
                SlaveDriverOption.AdditionalLongTaskNum,
                1, 0, 5, 1, parentOps);

        }

        protected override void RoleSpecificInit()
        {

            if (!this.HasOtherKillCool)
            {
                this.HasOtherKillCool = true;
                this.KillCoolTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
                    FloatOptionNames.KillCooldown);
            }

            var allOption = OptionHolder.AllOption;

            this.noneBonusKillTaskRange = (float)allOption[
                GetRoleOptionId(SlaveDriverOption.TaskProgressRange)].GetValue() / 100f;

            this.reduceRate = allOption[
                GetRoleOptionId(SlaveDriverOption.KillCoolReduceRate)].GetValue();
            this.specialAttackReduceRate = allOption[
                GetRoleOptionId(SlaveDriverOption.SpecialAttackKillCoolReduceRate)].GetValue();
            this.specialAttackStartTime = allOption[
                GetRoleOptionId(SlaveDriverOption.SpecialAttackTimer)].GetValue();
            this.aliveCheckTime = allOption[
                GetRoleOptionId(SlaveDriverOption.SpecialAttackAliveTimer)].GetValue();

            this.noneTaskPlayerAttakBonusChance = allOption[
                GetRoleOptionId(SlaveDriverOption.NoneTaskPlayerAttakBonusChance)].GetValue();
            this.noneTaskPlayerSpecialAttackChance = allOption[
                GetRoleOptionId(SlaveDriverOption.NoneTaskPlayerSpecialAttackChance)].GetValue();

            this.setNormalTaskNum = allOption[
               GetRoleOptionId(SlaveDriverOption.AdditionalNormalTaskNum)].GetValue();
            this.setLongTaskNum = allOption[
                GetRoleOptionId(SlaveDriverOption.AdditionalLongTaskNum)].GetValue();
            this.setCommonTaskNum = allOption[
                GetRoleOptionId(SlaveDriverOption.AdditionalCommonTaskNum)].GetValue();


            this.defaultKillCoolTime = this.KillCoolTime;
            this.specialAttackResult = new List<(byte, List<int>)>();
        }

        public void HookMuderPlayer(PlayerControl source, PlayerControl target)
        {
            if (source.PlayerId == this.rolePlayerId &&
                target.PlayerId == this.specialAttackPlayerId)
            {
                List<int> newTaskId = new List<int>();

                for (int i = 0; i < this.setLongTaskNum; ++i)
                {
                    newTaskId.Add(Helper.GameSystem.GetRandomLongTask());
                }
                for (int i = 0; i < this.setCommonTaskNum; ++i)
                {
                    newTaskId.Add(Helper.GameSystem.GetRandomCommonTaskId());
                }
                for (int i = 0; i < this.setNormalTaskNum; ++i)
                {
                    newTaskId.Add(Helper.GameSystem.GetRandomNormalTaskId());
                }

                var shuffled = newTaskId.OrderBy(
                    item => RandomGenerator.Instance.Next()).ToList();

                this.specialAttackResult.Add(
                    (target.PlayerId, shuffled));
                this.specialAttackPlayerId = byte.MaxValue;
            }
        }

        public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
        {
            return;
        }

        public void ResetOnMeetingStart()
        {
            this.KillCoolTime = this.defaultKillCoolTime;
            this.specialAttackPlayerId = byte.MaxValue;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (CachedShipStatus.Instance == null ||
                GameData.Instance == null) { return; }
            if (!CachedShipStatus.Instance.enabled) { return; }

            if (MeetingHud.Instance == null && this.timer < this.aliveCheckTime)
            {
                this.timer += Time.deltaTime;
            }

            if (this.specialAttackResult.Count == 0) { return; }

            List<(byte, List<int>)> removeResult = new List<(byte, List<int>)>();

            foreach (var (playerId, newTask) in this.specialAttackResult)
            {
                var playerInfo = GameData.Instance.GetPlayerById(
                    playerId);

                for (int i = 0; i < playerInfo.Tasks.Count; ++i)
                {
                    if (playerInfo.Tasks[i].Complete)
                    {
                        int taskIndex = newTask[0];
                        newTask.RemoveAt(0);
                        Helper.Logging.Debug($"SetTaskId:{taskIndex}");
                        Helper.GameSystem.RpcReplaceNewTask(playerId, i, taskIndex);

                        if (newTask.Count == 0)
                        {
                            removeResult.Add((playerId, newTask));
                        }
                        break;
                    }
                }
            }

            foreach (var item in removeResult)
            {
                this.specialAttackResult.Remove(item);
            }

        }

        private void setBonusPlayerKillCool()
        {
            this.KillCoolTime = this.defaultKillCoolTime * ((100f - this.reduceRate) / 100f);
        }
        private void setSpecialAttackPlayerKillCool()
        {
            this.KillCoolTime = this.defaultKillCoolTime * ((100f - this.specialAttackReduceRate) / 100f);
        }
    }
}
