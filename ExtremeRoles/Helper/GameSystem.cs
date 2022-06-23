using System;
using System.Collections.Generic;
using System.Reflection;

using Hazel;

using ExtremeRoles.Roles;
using ExtremeRoles.Performance;

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
            if (CachedShipStatus.Instance == null) { return byte.MaxValue; }

            List<int> taskIndex = getTaskIndex(
                CachedShipStatus.Instance.CommonTasks);

            int index = RandomGenerator.Instance.Next(taskIndex.Count);

            return (byte)taskIndex[index];
        }

        public static int GetRandomLongTask()
        {
            if (CachedShipStatus.Instance == null) { return byte.MaxValue; }

            List<int> taskIndex = getTaskIndex(
                CachedShipStatus.Instance.LongTasks);

            int index = RandomGenerator.Instance.Next(taskIndex.Count);

            return taskIndex[index];
        }

        public static int GetRandomNormalTaskId()
        {
            if (CachedShipStatus.Instance == null) { return byte.MaxValue; }

            List<int> taskIndex = getTaskIndex(
                CachedShipStatus.Instance.NormalTasks);

            int index = RandomGenerator.Instance.Next(taskIndex.Count);

            return taskIndex[index];
        }

        public static void SetTask(
            GameData.PlayerInfo playerInfo,
            int taskIndex)
        {
            NormalPlayerTask task = CachedShipStatus.Instance.GetTaskById((byte)taskIndex);

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

        public static bool SetPlayerNewTask(
            ref PlayerControl player,
            byte taskId, uint gameControlTaskId)
        {
            NormalPlayerTask addTask = CachedShipStatus.Instance.GetTaskById(taskId);
            if (addTask == null) { return false; }

            for (int i = 0; i < player.myTasks.Count; ++i)
            {
                var textTask = player.myTasks[i].gameObject.GetComponent<ImportantTextTask>();
                if (textTask != null) { continue; }

                if (SaboTask.Contains(player.myTasks[i].TaskType)) { continue; }
                if (ExtremeRolesPlugin.Compat.IsModMap)
                {
                    if (ExtremeRolesPlugin.Compat.ModMap.IsCustomSabotageTask(
                            player.myTasks[i].TaskType)) { continue; }
                }

                if (player.myTasks[i].IsComplete)
                {
                    NormalPlayerTask normalPlayerTask = UnityEngine.Object.Instantiate(
                        addTask, player.transform);
                    normalPlayerTask.Id = gameControlTaskId;
                    normalPlayerTask.Owner = player;
                    normalPlayerTask.Initialize();

                    var removeTask = player.myTasks[i];
                    player.myTasks[i] = normalPlayerTask;

                    removeTask.OnRemove();
                    UnityEngine.Object.Destroy(
                        removeTask.gameObject);
                    return true;
                }
            }
            return false;
        }


        public static void ShareVersion()
        {

            Version ver = Assembly.GetExecutingAssembly().GetName().Version;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                 CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
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
