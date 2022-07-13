using System.Collections.Generic;
using System.Linq;

using Hazel;

using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public sealed class TaskMaster : SingleRoleBase, IRoleSpecialSetUp, IRoleUpdate
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
            if (CachedShipStatus.Instance == null ||
                this.IsWin ||
                GameData.Instance == null) { return; }

            if (!CachedShipStatus.Instance.enabled) { return; }

            var playerInfo = GameData.Instance.GetPlayerById(
                rolePlayer.PlayerId);
            if (playerInfo.IsDead || 
                playerInfo.Disconnected || 
                playerInfo.Tasks.Count == 0) { return; }

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
                            CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                            (byte)RPCOperator.Command.TaskMasterSetNewTask,
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
            if (this.Id == targetRole.Id)
            {
                if (OptionHolder.Ship.IsSameNeutralSameWin)
                {
                    return true;
                }
                else
                {
                    return this.IsSameControlId(targetRole);
                }
            }
            else
            {
                return base.IsSameTeam(targetRole);
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

    }
}
