using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Alice : SingleRoleBase, IRoleAbility
    {

        public enum AliceOption
        {
            RevartCommonTaskNum,
            RevartLongTaskNum,
            RevartNormalTaskNum,
        }

        public RoleAbilityButtonBase Button
        { 
            get => this.aliceShipBroken;
            set
            {
                this.aliceShipBroken = value;
            }
        }

        public int RevartLongTask = 0;
        public int RevartNormalTask = 0;
        public int RevartCommonTask = 0;

        private RoleAbilityButtonBase aliceShipBroken;

        public Alice(): base(
            ExtremeRoleId.Alice,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Alice.ToString(),
            ColorPalette.AliceGold,
            true, false, true, true)
        {}

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Helper.Translation.GetString("shipBroken"),
                Loader.CreateSpriteFromResources(
                    Path.AliceShipBroken, 115f));
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

        public bool IsAbilityUse()
        {
            return this.IsCommonUse();
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
           if (ExtremeRoleManager.GameRole[killerPlayer.PlayerId].IsImposter())
           {
                this.IsWin = true;
           }
        }

        public bool UseAbility()
        {
            RPCOperator.Call(
                PlayerControl.LocalPlayer.NetId,
                RPCOperator.Command.AliceShipBroken,
                new List<byte> { PlayerControl.LocalPlayer.PlayerId });
            RPCOperator.AliceShipBroken(
                PlayerControl.LocalPlayer.PlayerId);

            return true;
        }

        public static void ShipBroken(byte callerId)
        {
            var alice = (Alice)ExtremeRoleManager.GameRole[callerId];
            var player = PlayerControl.LocalPlayer;
            var playerInfo = GameData.Instance.GetPlayerById(
                player.PlayerId);

            List<byte> addTaskId = new List<byte> ();
            
            for (int i = 0; i < alice.RevartLongTask; ++i)
            {
                addTaskId.Add(Helper.GameSystem.GetRandomLongTask());
            }
            for (int i = 0; i < alice.RevartCommonTask; ++i)
            {
                addTaskId.Add(Helper.GameSystem.GetRandomCommonTaskId());
            }
            for (int i = 0; i < alice.RevartNormalTask; ++i)
            {
                addTaskId.Add(Helper.GameSystem.GetRandomNormalTaskId());
            }

            var shuffled = addTaskId.OrderBy(
                item => RandomGenerator.Instance.Next()).ToList();
            
            for (int i = 0; i < player.myTasks.Count; ++i)
            {
                if (shuffled.Count == 0) { break; }

                bool isTaskComp = player.myTasks[i].IsComplete;
                if (isTaskComp)
                {
                    byte taskId = shuffled[0];
                    shuffled.RemoveAt(0);

                    playerInfo.Tasks[i] = new GameData.TaskInfo(
                        taskId, (uint)i);
                    playerInfo.Tasks[i].Id = (uint)i;

                    NormalPlayerTask normalPlayerTask = 
                        UnityEngine.Object.Instantiate<NormalPlayerTask>(
                            ShipStatus.Instance.GetTaskById(taskId),
                            player.transform);
                    normalPlayerTask.Id = (uint)i;
                    normalPlayerTask.Owner = player;
                    normalPlayerTask.Initialize();

                    player.myTasks[i] = normalPlayerTask;
                }
            }
            GameData.Instance.SetDirtyBit(
                1U << (int)player.PlayerId);

        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            this.CreateAbilityCountOption(
                parentOps, 100);

            CustomOption.Create(
                this.GetRoleOptionId((int)AliceOption.RevartLongTaskNum),
                string.Concat(
                    this.RoleName,
                    AliceOption.RevartLongTaskNum.ToString()),
                1, 0, 15, 1, parentOps);
            CustomOption.Create(
                this.GetRoleOptionId((int)AliceOption.RevartCommonTaskNum),
                string.Concat(
                    this.RoleName,
                    AliceOption.RevartCommonTaskNum.ToString()),
                1, 0, 15, 1, parentOps);
            CustomOption.Create(
                this.GetRoleOptionId((int)AliceOption.RevartNormalTaskNum),
                string.Concat(
                    this.RoleName,
                    AliceOption.RevartNormalTaskNum.ToString()),
                1, 0, 15, 1, parentOps);

        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionHolder.AllOption;
            this.RevartNormalTask = allOption[
                GetRoleOptionId((int)AliceOption.RevartNormalTaskNum)].GetValue();
            this.RevartLongTask = allOption[
                GetRoleOptionId((int)AliceOption.RevartLongTaskNum)].GetValue();
            this.RevartCommonTask = allOption[
                GetRoleOptionId((int)AliceOption.RevartCommonTaskNum)].GetValue();

            this.RoleAbilityInit();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }
    }
}
