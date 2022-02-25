using System.Linq;
using System.Collections.Generic;

using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Agency : SingleRoleBase, IRoleAbility, IRoleUpdate
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
            byte rolePlayerId, byte targetPlayerId, byte getTaskNum)
        {

            PlayerControl targetPlayer = Player.GetPlayerControlById(
                targetPlayerId);
            var agency = (Agency)ExtremeRoleManager.GameRole[rolePlayerId];


            var shuffleTaskIndex = Enumerable.Range(
                0, targetPlayer.myTasks.Count).ToList().OrderBy(
                    item => RandomGenerator.Instance.Next()).ToList();

            int takeTask = 0;
            
            foreach (int i in shuffleTaskIndex)
            {
                if (takeTask >= (int)getTaskNum) { break; }
                if (targetPlayer.myTasks[i].IsComplete) { continue; }

                var replaceTask = targetPlayer.myTasks[i];
                var importantText = replaceTask.gameObject.GetComponent<ImportantTextTask>();

                if (importantText != null) { continue; }

                TaskTypes type = replaceTask.TaskType;
                if (type == TaskTypes.FixLights ||
                    type == TaskTypes.FixComms ||
                    type == TaskTypes.StopCharles ||
                    type == TaskTypes.ResetSeismic ||
                    type == TaskTypes.ResetReactor ||
                    type == TaskTypes.RestoreOxy) { continue; }

                GameData.TaskInfo taskInfo = targetPlayer.Data.Tasks[(int)replaceTask.Id];
                int taskId = (int)taskInfo.TypeId;

                if (ShipStatus.Instance.CommonTasks.FirstOrDefault(
                    (NormalPlayerTask t) => t.Index == (int)taskId) != null)
                {
                    agency.TakeTask.Add(TakeTaskType.Common);
                }
                else if (ShipStatus.Instance.LongTasks.FirstOrDefault(
                    (NormalPlayerTask t) => t.Index == (int)taskId) != null)
                {
                    agency.TakeTask.Add(TakeTaskType.Long);
                }
                else if (ShipStatus.Instance.NormalTasks.FirstOrDefault(
                    (NormalPlayerTask t) => t.Index == (int)taskId) != null)
                {
                    agency.TakeTask.Add(TakeTaskType.Normal);
                }

                ++takeTask;
                targetPlayer.CompleteTask(replaceTask.Id);
                replaceTask.OnRemove();
            }

            if (PlayerControl.LocalPlayer.PlayerId != rolePlayerId)
            {
                agency.TakeTask.Clear();
            }
            GameData.Instance.SetDirtyBit(
                1U << (int)targetPlayer.PlayerId);
        }

        public static void ReplaceToNewTask(byte playerId, int index, int taskIndex)
        {

            var player = Player.GetPlayerControlById(
                playerId);
            var playerInfo = GameData.Instance.GetPlayerById(
                player.PlayerId);

            byte taskId = (byte)taskIndex;

            playerInfo.Tasks[index] = new GameData.TaskInfo(
                taskId, (uint)index);
            playerInfo.Tasks[index].Id = (uint)index;

            NormalPlayerTask normalPlayerTask =
                UnityEngine.Object.Instantiate(
                    ShipStatus.Instance.GetTaskById(taskId),
                    player.transform);
            normalPlayerTask.Id = (uint)index;
            normalPlayerTask.Owner = player;
            normalPlayerTask.Initialize();

            for (int i = 0; i < player.myTasks.Count; ++i)
            {
                if (player.myTasks[i].IsComplete)
                {
                    player.myTasks[i] = normalPlayerTask;
                    break;
                }
            }

            GameData.Instance.SetDirtyBit(
                1U << (int)player.PlayerId);
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

            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.AgencyTakeTask,
                new List<byte>
                {
                    playerId,
                    this.TargetPlayer,
                    (byte)takeNum,
                });
            RPCOperator.AgencyTakeTask(
                playerId, this.TargetPlayer, (byte)takeNum);
            this.TargetPlayer = byte.MaxValue;

            return true;
        }

        public bool IsAbilityUse()
        {

            this.TargetPlayer = byte.MaxValue;

            PlayerControl target = Player.GetPlayerTarget(
                PlayerControl.LocalPlayer, this,
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
            if (ShipStatus.Instance == null ||
                GameData.Instance == null) { return; }

            if (!ShipStatus.Instance.enabled ||
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
                        PlayerControl.LocalPlayer.NetId,
                        (byte)RPCOperator.Command.TaskMasterSetNewTask,
                        Hazel.SendOption.Reliable, -1);
                    writer.Write(rolePlayer.PlayerId);
                    writer.Write(i);
                    writer.Write(taskIndex);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    ReplaceToNewTask(rolePlayer.PlayerId, i, taskIndex);
                }
            }
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {

            CustomOption.Create(
               GetRoleOptionId((int)AgencyOption.MaxTaskNum),
               string.Concat(
                   this.RoleName,
                   AgencyOption.MaxTaskNum.ToString()),
               2, 1, 3, 1,
               parentOps, format: "unitPercentage");

            CustomOption.Create(
               GetRoleOptionId((int)AgencyOption.TakeTaskRange),
               string.Concat(
                   this.RoleName,
                   AgencyOption.TakeTaskRange.ToString()),
               1.0f, 0.0f, 2.0f, 0.1f,
               parentOps);

            this.CreateAbilityCountOption(
                parentOps, 2, 5);
        }

        protected override void RoleSpecificInit()
        {
            this.maxTakeTask = OptionHolder.AllOption[
                GetRoleOptionId((int)AgencyOption.MaxTaskNum)].GetValue() + 1;
            this.takeTaskRange = OptionHolder.AllOption[
                GetRoleOptionId((int)AgencyOption.TakeTaskRange)].GetValue();

            this.RoleAbilityInit();
        }
    }
}
