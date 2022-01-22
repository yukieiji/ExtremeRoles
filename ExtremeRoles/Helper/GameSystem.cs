using System;
using System.Collections.Generic;

using ExtremeRoles.Roles;

namespace ExtremeRoles.Helper
{
    public static class GameSystem
    {
        public static bool IsLobby
        {
            get
            {
                return (
                    AmongUsClient.Instance.GameState !=
                    InnerNet.InnerNetClient.GameStates.Started
                );
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

        public static int GetRandomCommonTaskId()
        {
            if (ShipStatus.Instance == null) { return byte.MaxValue; }

            List<int> taskIndex = getTaskIndex(
                ShipStatus.Instance.CommonTasks);

            int index = UnityEngine.Random.RandomRange(0, taskIndex.Count);

            return (byte)taskIndex[index];
        }

        public static int GetRandomLongTask()
        {
            if (ShipStatus.Instance == null) { return byte.MaxValue; }

            List<int> taskIndex = getTaskIndex(
                ShipStatus.Instance.LongTasks);

            int index = UnityEngine.Random.RandomRange(0, taskIndex.Count);

            return taskIndex[index];
        }

        public static int GetRandomNormalTaskId()
        {
            if (ShipStatus.Instance == null) { return byte.MaxValue; }

            List<int> taskIndex = getTaskIndex(
                ShipStatus.Instance.NormalTasks);

            int index = UnityEngine.Random.RandomRange(0, taskIndex.Count);

            return taskIndex[index];
        }

        public static void SetTask(
            GameData.PlayerInfo playerInfo,
            int taskIndex)
        {
            NormalPlayerTask task = ShipStatus.Instance.GetTaskById((byte)taskIndex);

            PlayerControl player = playerInfo.Object;

            int index = playerInfo.Tasks.Count;
            playerInfo.Tasks.Add(new GameData.TaskInfo((byte)taskIndex, (uint)index));
            playerInfo.Tasks[index].Id = (uint)index;

            task.Id = (uint)index;
            task.Owner = player;
            task.Initialize();

            player.myTasks.Add(task);
            player.SetDirtyBit(1U << (int)player.PlayerId);
        }

        private static List<int> getTaskIndex(
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
