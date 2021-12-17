using System;
using System.Collections.Generic;

using ExtremeRoles.Roles;


namespace ExtremeRoles.Helper
{
    public class Task
    {
        public static void ClearAllTasks(ref PlayerControl player)
        {
            if (player == null) { return; }
            for (int i = 0; i < player.myTasks.Count; i++)
            {
                PlayerTask playerTask = player.myTasks[i];
                playerTask.OnRemove();
                UnityEngine.Object.Destroy(playerTask.gameObject);
            }
            player.myTasks.Clear();

            if (player.Data != null && player.Data.Tasks != null)
            {
                player.Data.Tasks.Clear();
            }
        }

        public static Tuple<int, int> GetTaskInfo(
            GameData.PlayerInfo playerInfo)
        {
            int TotalTasks = 0;
            int CompletedTasks = 0;
            if (!(playerInfo.Disconnected) &&
                 (playerInfo.Tasks != null) &&
                 (playerInfo.Object) &&
                 (playerInfo.Role) &&
                 (playerInfo.Role.TasksCountTowardProgress) &&
                 (PlayerControl.GameOptions.GhostsDoTasks || !playerInfo.IsDead) &&
                  ExtremeRoleManager.GameRole[playerInfo.PlayerId].HasTask
                )
            {

                for (int j = 0; j < playerInfo.Tasks.Count; ++j)
                {
                    ++TotalTasks;
                    if (playerInfo.Tasks[j].Complete)
                    {
                        ++CompletedTasks;
                    }
                }
            }
            return Tuple.Create(CompletedTasks, TotalTasks);
        }

        public static byte GetRandomCommonTaskId()
        {
            if (ShipStatus.Instance == null) { return Byte.MaxValue; }

            List<int> taskIndex = GetTaskIndex(
                ShipStatus.Instance.CommonTasks);

            int index = UnityEngine.Random.RandomRange(0, taskIndex.Count);

            return (byte)taskIndex[index];
        }

        public static byte GetRandomLongTask()
        {
            if (ShipStatus.Instance == null) { return Byte.MaxValue; }

            List<int> taskIndex = GetTaskIndex(
                ShipStatus.Instance.LongTasks);

            int index = UnityEngine.Random.RandomRange(0, taskIndex.Count);

            return (byte)taskIndex[index];
        }

        public static byte GetRandomNormalTaskId()
        {
            if (ShipStatus.Instance == null) { return Byte.MaxValue; }

            List<int> taskIndex = GetTaskIndex(
                ShipStatus.Instance.NormalTasks);

            int index = UnityEngine.Random.RandomRange(0, taskIndex.Count);

            return (byte)taskIndex[index];
        }

        public static void SetTask(
            GameData.PlayerInfo playerInfo,
            byte bytedTaskIndex)
        {
            NormalPlayerTask task = ShipStatus.Instance.GetTaskById(bytedTaskIndex);

            PlayerControl player = playerInfo.Object;

            int index = playerInfo.Tasks.Count;
            playerInfo.Tasks.Add(new GameData.TaskInfo(bytedTaskIndex, (uint)index));
            playerInfo.Tasks[index].Id = (uint)index;

            task.Id = bytedTaskIndex;
            task.Owner = player;
            task.Initialize();

            player.myTasks.Add(task);
            player.SetDirtyBit(1U << (int)player.PlayerId);
        }

        private static List<int> GetTaskIndex(
            NormalPlayerTask[] tasks)
        {
            List<int> index = new List<int>();
            for (int i = 0; i < tasks.Length; ++i)
            {
                index.Add(tasks[i].Index);
            }

            return index;
        }

    }
}
