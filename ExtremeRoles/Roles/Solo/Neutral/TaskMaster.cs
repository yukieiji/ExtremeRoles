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
        private bool setUpEnd = false;

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
                !this.setUpEnd ||
                GameData.Instance == null) { return; }

            if (!ShipStatus.Instance.enabled) { return; }

            var playerInfo = GameData.Instance.GetPlayerById(
                rolePlayer.PlayerId);

            foreach (var task in playerInfo.Tasks)
            {
                if (!task.Complete) { return; }
            }
            RPCOperator.RoleIsWin(rolePlayer.PlayerId);
            this.IsWin = true;
        }

        public void IntroBeginSetUp()
        {
            return;
        }

        public void IntroEndSetUp()
        {
            List<byte> addTaskId = new List<byte>();

            var player = PlayerControl.LocalPlayer;
            var playerInfo = GameData.Instance.GetPlayerById(
                player.PlayerId);

            for (int i = 0; i < this.addLongTask; ++i)
            {
                addTaskId.Add(GameSystem.GetRandomLongTask());
            }
            for (int i = 0; i < this.addCommonTask; ++i)
            {
                addTaskId.Add(GameSystem.GetRandomCommonTaskId());
            }
            for (int i = 0; i < this.addNormalTask; ++i)
            {
                addTaskId.Add(GameSystem.GetRandomNormalTaskId());
            }

            var shuffled = addTaskId.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();
            foreach (byte taskId in addTaskId)
            {
                int length = playerInfo.Tasks.Count;
                playerInfo.Tasks.Add(
                    new GameData.TaskInfo(taskId, (uint)length));
                playerInfo.Tasks[length].Id = (uint)length;

                NormalPlayerTask normalPlayerTask =
                    UnityEngine.Object.Instantiate<NormalPlayerTask>(
                        ShipStatus.Instance.GetTaskById(taskId),
                        player.transform);
                normalPlayerTask.Id = playerInfo.Tasks[length].Id;
                normalPlayerTask.Owner = player;
                normalPlayerTask.Initialize();

                player.myTasks.Add(normalPlayerTask);
            }
            GameData.Instance.SetDirtyBit(
                1U << (int)player.PlayerId);
            this.setUpEnd = true;
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
            this.setUpEnd = false;
        }
    }
}
