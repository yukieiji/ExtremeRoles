using System;
using System.Collections.Generic;
using System.Reflection;

using Hazel;

using ExtremeRoles.Roles;

namespace ExtremeRoles.Helper
{
    public static class GameSystem
    {
        public static HashSet<TaskTypes> SaboTask = new HashSet<TaskTypes>()
        {
            TaskTypes.FixLights,
            TaskTypes.RestoreOxy,
            TaskTypes.ResetReactor,
            TaskTypes.ResetSeismic,
            TaskTypes.FixComms,
            TaskTypes.StopCharles
        };

        private static HashSet<TaskTypes> ignoreTask = new HashSet<TaskTypes>()
        {
            TaskTypes.FixWiring,
            TaskTypes.VentCleaning,
        };

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
        public static bool IsFreePlay
        {
            get
            {
                return AmongUsClient.Instance.GameMode == GameModes.FreePlay;
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

            int index = RandomGenerator.Instance.Next(taskIndex.Count);

            return (byte)taskIndex[index];
        }

        public static int GetRandomLongTask()
        {
            if (ShipStatus.Instance == null) { return byte.MaxValue; }

            List<int> taskIndex = getTaskIndex(
                ShipStatus.Instance.LongTasks);

            int index = RandomGenerator.Instance.Next(taskIndex.Count);

            return taskIndex[index];
        }

        public static int GetRandomNormalTaskId()
        {
            if (ShipStatus.Instance == null) { return byte.MaxValue; }

            List<int> taskIndex = getTaskIndex(
                ShipStatus.Instance.NormalTasks);

            int index = RandomGenerator.Instance.Next(taskIndex.Count);

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

        public static void SetPlayerNewTask(
            ref PlayerControl player,
            byte taskId, uint gameControlTaskId)
        {
            NormalPlayerTask normalPlayerTask =
                UnityEngine.Object.Instantiate<NormalPlayerTask>(
                    ShipStatus.Instance.GetTaskById(taskId),
                    player.transform);
            normalPlayerTask.Id = gameControlTaskId;
            normalPlayerTask.Owner = player;
            normalPlayerTask.Initialize();

            for (int i = 0; i < player.myTasks.Count; ++i)
            {
                var textTask = player.myTasks[i].gameObject.GetComponent<ImportantTextTask>();
                if (textTask != null) { continue; }

                if (SaboTask.Contains(player.myTasks[i].TaskType)) { continue; }

                if (player.myTasks[i].IsComplete)
                {
                    var removeTask = player.myTasks[i];
                    player.myTasks[i] = normalPlayerTask;

                    removeTask.OnRemove();
                    UnityEngine.Object.Destroy(
                        removeTask.gameObject);
                    break;
                }
            }
        }


        public static void ShareVersion()
        {

            Version ver = Assembly.GetExecutingAssembly().GetName().Version;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RPCOperator.Command.ShareVersion,
                Hazel.SendOption.Reliable, -1);
            writer.Write(ver.Major);
            writer.Write(ver.Minor);
            writer.Write(ver.Build);
            writer.Write(ver.Revision);
            writer.WritePacked(AmongUsClient.Instance.ClientId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            RPCOperator.AddVersionData(
                ver.Major, ver.Minor,
                ver.Build, ver.Revision,
                AmongUsClient.Instance.ClientId);
        }


        private static List<int> getTaskIndex(
            NormalPlayerTask[] tasks)
        {
            List<int> index = new List<int>();
            for (int i = 0; i < tasks.Length; ++i)
            {
                if (!ignoreTask.Contains(tasks[i].TaskType))
                {
                    index.Add(tasks[i].Index);
                }
            }

            return index;
        }

    }
}
