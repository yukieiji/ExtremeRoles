using System.Collections.Generic;
using System.Linq;

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
            AddCommonTaskNum,
            AddLongTaskNum,
            AddNormalTaskNum,
        }

        private int addLongTask = 0;
        private int addNormalTask = 0;
        private int addCommonTask = 0;
        private List<byte> addTask = new List<byte>();

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

            int compCount = 1;

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
                        byte taskId = shuffled[0];
                        this.addTask.Remove(taskId);

                        RPCOperator.Call(
                            PlayerControl.LocalPlayer.NetId,
                            RPCOperator.Command.TaskMasterSetNetTask,
                            new List<byte> { rolePlayer.PlayerId, (byte)i, taskId });
                        ReplaceToNewTask(rolePlayer.PlayerId, i, taskId);
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
            CustomOption.Create(
                this.GetRoleOptionId((int)TaskMasterOption.AddCommonTaskNum),
                string.Concat(
                    this.RoleName,
                    TaskMasterOption.AddCommonTaskNum.ToString()),
                1, 0, 15, 1, parentOps);
            CustomOption.Create(
                this.GetRoleOptionId((int)TaskMasterOption.AddLongTaskNum),
                string.Concat(
                    this.RoleName,
                    TaskMasterOption.AddLongTaskNum.ToString()),
                1, 0, 15, 1, parentOps);
            CustomOption.Create(
                this.GetRoleOptionId((int)TaskMasterOption.AddNormalTaskNum),
                string.Concat(
                    this.RoleName,
                    TaskMasterOption.AddNormalTaskNum.ToString()),
                1, 0, 15, 1, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionHolder.AllOption;
            this.addLongTask = allOption[
                GetRoleOptionId((int)TaskMasterOption.AddLongTaskNum)].GetValue();
            this.addNormalTask = allOption[
                GetRoleOptionId((int)TaskMasterOption.AddNormalTaskNum)].GetValue();
            this.addCommonTask = allOption[
                GetRoleOptionId((int)TaskMasterOption.AddCommonTaskNum)].GetValue();
            this.addTask.Clear();
        }

        public static void ReplaceToNewTask(byte playerId, int index, byte taskId)
        {
     
            var player = Player.GetPlayerControlById(
                playerId);
            var playerInfo = GameData.Instance.GetPlayerById(
                player.PlayerId);

            Logging.Debug($"Replace Start");

            playerInfo.Tasks[index] = new GameData.TaskInfo(
                taskId, (uint)index);
            playerInfo.Tasks[index].Id = (uint)index;

            NormalPlayerTask normalPlayerTask =
                UnityEngine.Object.Instantiate<NormalPlayerTask>(
                    ShipStatus.Instance.GetTaskById(taskId),
                    player.transform);
            normalPlayerTask.Id = (uint)index;
            normalPlayerTask.Owner = player;
            normalPlayerTask.Initialize();

            player.myTasks[index] = normalPlayerTask;

            GameData.Instance.SetDirtyBit(
                1U << (int)player.PlayerId);
        }

    }
}
