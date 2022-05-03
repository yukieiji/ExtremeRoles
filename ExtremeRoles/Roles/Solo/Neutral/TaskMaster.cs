using System.Collections.Generic;
using System.Linq;

using Hazel;

using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class TaskMaster : SingleRoleBase, IRoleSpecialSetUp, IRoleUpdate
    {
        public enum TaskMasterOption
        {
            CanUseSabotage,
            AddCommonTaskNum,
            AddLongTaskNum,
            AddNormalTaskNum,
        }

        private int addLongTask = 0;
        private int addNormalTask = 0;
        private int addCommonTask = 0;
        private List<int> addTask = new List<int>();

        public TaskMaster() : base(
            ExtremeRoleId.TaskMaster,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.TaskMaster.ToString(),
            ColorPalette.NeutralColor,
            false, true, true, true)
        { }

        public void Update(PlayerControl rolePlayer)
        {
            if (ShipStatus.Instance == null ||
                this.IsWin ||
                GameData.Instance == null) { return; }

            if (!ShipStatus.Instance.enabled) { return; }

            var playerInfo = GameData.Instance.GetPlayerById(
                rolePlayer.PlayerId);
            if (playerInfo.IsDead || playerInfo.Disconnected) { return; }

            int compCount = 0;

            for (int i = 0; i < playerInfo.Tasks.Count; ++i)
            {
                if (playerInfo.Tasks[i].Complete)
                {
                    if (this.addTask.Count == 0)
                    {
                        ++compCount;
                    }
                    else
                    {
                        var shuffled = this.addTask.OrderBy(
                            item => RandomGenerator.Instance.Next()).ToList();
                        int taskIndex = shuffled[0];

                        Helper.Logging.Debug($"SetTaskId:{taskIndex}");

                        this.addTask.Remove(taskIndex);

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
            if (compCount == playerInfo.Tasks.Count)
            {
                RPCOperator.RoleIsWin(rolePlayer.PlayerId);
                this.IsWin = true;
            }
        }

        public void IntroBeginSetUp()
        {
            return;
        }

        public void IntroEndSetUp()
        {
            for (int i = 0; i < this.addLongTask; ++i)
            {
                this.addTask.Add(GameSystem.GetRandomLongTask());
            }
            for (int i = 0; i < this.addCommonTask; ++i)
            {
                this.addTask.Add(GameSystem.GetRandomCommonTaskId());
            }
            for (int i = 0; i < this.addNormalTask; ++i)
            {
                this.addTask.Add(GameSystem.GetRandomNormalTaskId());
            }
        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            if(OptionHolder.Ship.IsSameNeutralSameWin)
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
            CreateBoolOption(
                TaskMasterOption.CanUseSabotage,
                true, parentOps);
            CreateIntOption(
                TaskMasterOption.AddCommonTaskNum,
                1, 0, 15, 1, parentOps);
            CreateIntOption(
                TaskMasterOption.AddLongTaskNum,
                1, 0, 15, 1, parentOps);
            CreateIntOption(
                TaskMasterOption.AddNormalTaskNum,
                1, 0, 15, 1, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionHolder.AllOption;
            this.UseSabotage = allOption[
                GetRoleOptionId(TaskMasterOption.CanUseSabotage)].GetValue();
            this.addLongTask = allOption[
                GetRoleOptionId(TaskMasterOption.AddLongTaskNum)].GetValue();
            this.addNormalTask = allOption[
                GetRoleOptionId(TaskMasterOption.AddNormalTaskNum)].GetValue();
            this.addCommonTask = allOption[
                GetRoleOptionId(TaskMasterOption.AddCommonTaskNum)].GetValue();
            this.addTask.Clear();
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

    }
}
