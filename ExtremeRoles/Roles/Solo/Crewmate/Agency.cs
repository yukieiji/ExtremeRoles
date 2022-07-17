using System.Linq;
using System.Collections.Generic;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Agency : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        public enum AgencyOption
        {
            MaxTaskNum,
            TakeTaskRange
        }

        public enum TakeTaskType
        {
            Normal,
            Long,
            Common
        }

        public RoleAbilityButtonBase Button
        {
            get => this.takeTaskButton;
            set
            {
                this.takeTaskButton = value;
            }
        }

        public byte TargetPlayer = byte.MaxValue;
        public List<TakeTaskType> TakeTask = new List<TakeTaskType>();

        private int maxTakeTask;
        private float takeTaskRange;
        private RoleAbilityButtonBase takeTaskButton;

        public Agency() : base(
            ExtremeRoleId.Agency,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Agency.ToString(),
            ColorPalette.AgencyYellowGreen,
            false, true, false, false)
        { }

        public static void TakeTargetPlayerTask(
            byte targetPlayerId, List<int> removeTaskId)
        {

            PlayerControl targetPlayer = Player.GetPlayerControlById(
                targetPlayerId);

            foreach (PlayerTask task in targetPlayer.myTasks.GetFastEnumerator())
            {
                if (task == null) { continue; }

                var textTask = task.gameObject.GetComponent<ImportantTextTask>();
                if (textTask != null) { continue; }
                if (GameSystem.SaboTask.Contains(task.TaskType))  { continue; }

                if (removeTaskId.Contains((int)task.Id))
                {
                    targetPlayer.CompleteTask(task.Id);
                    task.OnRemove();
                }
            }

            GameData.Instance.SetDirtyBit(
                1U << (int)targetPlayer.PlayerId);
        }

        public static void ReplaceToNewTask(byte playerId, int index, int taskIndex)
        {

            var player = Player.GetPlayerControlById(
                playerId);

            if (player == null) { return; }

            byte taskId = (byte)taskIndex;

            if (GameSystem.SetPlayerNewTask(
                ref player, taskId, (uint)index))
            {
                player.Data.Tasks[index] = new GameData.TaskInfo(
                    taskId, (uint)index);
                player.Data.Tasks[index].Id = (uint)index;

                GameData.Instance.SetDirtyBit(
                    1U << (int)player.PlayerId);
            }
        }


        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("takeTask"),
                Loader.CreateSpriteFromResources(
                    Path.AgencyTakeTask));
            this.Button.SetLabelToCrewmate();
        }

        public bool UseAbility()
        {

            var targetRole = ExtremeRoleManager.GameRole[this.TargetPlayer];

            int takeNum = UnityEngine.Random.RandomRange(1, this.maxTakeTask);
            
            if (!targetRole.HasTask)
            {
                int totakTaskNum = GameData.Instance.TotalTasks;
                int compTaskNum = GameData.Instance.CompletedTasks;

                float taskGauge = (float)compTaskNum / (float)totakTaskNum;
                if (taskGauge > 0.9f)
                {
                    takeNum = 0;
                }
                else if (0.75 < taskGauge && taskGauge <= 0.9f)
                {
                    takeNum = UnityEngine.Random.RandomRange(0, 2);
                }
                else if (0.5 < taskGauge && taskGauge <= 0.75f)
                {
                    takeNum = UnityEngine.Random.RandomRange(0, this.maxTakeTask);
                }
            }


            if (takeNum == 0) { return true; }

            byte playerId = PlayerControl.LocalPlayer.PlayerId;

            GameData.PlayerInfo targetPlayerInfo = GameData.Instance.GetPlayerById(
                this.TargetPlayer);

            var shuffleTaskIndex = Enumerable.Range(
                0, targetPlayerInfo.Tasks.Count).ToList().OrderBy(
                    item => RandomGenerator.Instance.Next()).ToList();
            int takeTask = 0;
            List<int> getTaskId = new List<int>();

            foreach (int i in shuffleTaskIndex)
            {
                if (takeTask >= takeNum) { break; }

                if (targetPlayerInfo.Tasks[i].Complete) { continue; }

                int taskId = (int)targetPlayerInfo.Tasks[i].TypeId;

                if (CachedShipStatus.Instance.CommonTasks.FirstOrDefault(
                    (NormalPlayerTask t) => t.Index == taskId) != null)
                {
                    this.TakeTask.Add(TakeTaskType.Common);
                }
                else if (CachedShipStatus.Instance.LongTasks.FirstOrDefault(
                    (NormalPlayerTask t) => t.Index == taskId) != null)
                {
                    this.TakeTask.Add(TakeTaskType.Long);
                }
                else if (CachedShipStatus.Instance.NormalTasks.FirstOrDefault(
                    (NormalPlayerTask t) => t.Index == taskId) != null)
                {
                    this.TakeTask.Add(TakeTaskType.Normal);
                }
                ++takeTask;
                getTaskId.Add((int)targetPlayerInfo.Tasks[i].Id);
            }

            if (getTaskId.Count == 0) { return true; }


            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RPCOperator.Command.AgencyTakeTask,
                Hazel.SendOption.Reliable, -1);
            writer.Write(this.TargetPlayer);
            writer.Write(getTaskId.Count);

            foreach (int taskid in getTaskId)
            {
                writer.Write(taskid);
            }
            
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            TakeTargetPlayerTask(this.TargetPlayer, getTaskId);
            this.TargetPlayer = byte.MaxValue;

            return true;
        }

        public bool IsAbilityUse()
        {

            this.TargetPlayer = byte.MaxValue;

            PlayerControl target = Player.GetPlayerTarget(
                CachedPlayerControl.LocalPlayer, this,
                this.takeTaskRange);

            if (target != null)
            {
                this.TargetPlayer = target.PlayerId;
            }

            return this.IsCommonUse() && this.TargetPlayer != byte.MaxValue;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (CachedShipStatus.Instance == null ||
                GameData.Instance == null) { return; }

            if (!CachedShipStatus.Instance.enabled ||
                this.TakeTask.Count == 0) { return; }

            var playerInfo = GameData.Instance.GetPlayerById(
                rolePlayer.PlayerId);

            for (int i = 0; i < playerInfo.Tasks.Count; ++i)
            {
                if (playerInfo.Tasks[i].Complete)
                {
                    TakeTaskType taskType = this.TakeTask[0];
                    this.TakeTask.RemoveAt(0);

                    int taskIndex;

                    switch (taskType)
                    {
                        case TakeTaskType.Normal:
                            taskIndex = GameSystem.GetRandomNormalTaskId();
                            break;
                        case TakeTaskType.Long:
                            taskIndex = GameSystem.GetRandomLongTask();
                            break;
                        case TakeTaskType.Common:
                            taskIndex = GameSystem.GetRandomCommonTaskId();
                            break;
                        default:
                            continue;
                    }

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                        CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                        (byte)RPCOperator.Command.AgencySetNewTask,
                        Hazel.SendOption.Reliable, -1);
                    writer.Write(rolePlayer.PlayerId);
                    writer.Write(i);
                    writer.Write(taskIndex);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    ReplaceToNewTask(rolePlayer.PlayerId, i, taskIndex);
                    break;
                }
            }
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {

            CreateIntOption(
                AgencyOption.MaxTaskNum,
                2, 1, 3, 1, parentOps);
            CreateFloatOption(
                AgencyOption.TakeTaskRange,
                1.0f, 0.5f, 2.0f, 0.1f,
                parentOps);

            this.CreateAbilityCountOption(
                parentOps, 2, 5);
        }

        protected override void RoleSpecificInit()
        {
            this.maxTakeTask = OptionHolder.AllOption[
                GetRoleOptionId(AgencyOption.MaxTaskNum)].GetValue() + 1;
            this.takeTaskRange = OptionHolder.AllOption[
                GetRoleOptionId(AgencyOption.TakeTaskRange)].GetValue();

            this.RoleAbilityInit();

            this.TakeTask.Clear();

        }
    }
}
