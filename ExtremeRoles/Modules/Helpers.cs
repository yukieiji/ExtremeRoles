using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnhollowerBaseLib;
using UnityEngine;

using ExtremeRoles.Roles;

namespace ExtremeRoles.Modules
{
    public enum MurderAttemptResult
    {
        PerformKill,
        SuppressKill,
        BlankKill
    }

    public static class Helpers
    {

        public static bool ShowButtons
        {
            get
            {
                return (
                    !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
                    !MeetingHud.Instance &&
                    !ExileController.Instance);
            }
        }

        public static bool IsGameLobby
        {
            get
            {
                return (
                    AmongUsClient.Instance.GameState !=
                    InnerNet.InnerNetClient.GameStates.Started
                );
            }
        }

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
        public static string ConcatString(string baseString, string addString)
        {
            return string.Format(
                "{0}{1}",
                baseString,
                addString);
        }

        public static string Cs(Color c, string s)
        {
            return string.Format(
                "<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>",
                ToByte(c.r),
                ToByte(c.g),
                ToByte(c.b),
                ToByte(c.a), s);
        }

        public static void DebugLog(string msg)
        {
#if DEBUG
            if (ExtremeRolesPlugin.DebugMode.Value)
            {
                ExtremeRolesPlugin.Logger.LogInfo(msg);
            }
#endif
        }

        public static PlayerControl GetPlayerControlById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == id) { return player; }
            }
            return null;
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

                for (int j = 0; j < playerInfo.Tasks.Count; j++)
                {
                    TotalTasks++;
                    if (playerInfo.Tasks[j].Complete)
                    {
                        CompletedTasks++;
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

        public static bool IsBlocked(PlayerTask task, PlayerControl pc)
        {
            if (task == null || pc == null || pc != PlayerControl.LocalPlayer) return false;

            bool isLights = task.TaskType == TaskTypes.FixLights;
            bool isComms = task.TaskType == TaskTypes.FixComms;
            bool isReactor = task.TaskType == TaskTypes.StopCharles || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.ResetReactor;
            bool isO2 = task.TaskType == TaskTypes.RestoreOxy;

            return false;
        }

        public static bool IsBlocked(Console console, PlayerControl pc)
        {
            if (console == null || pc == null || pc != PlayerControl.LocalPlayer)
            {
                return false;
            }

            PlayerTask task = console.FindTask(pc);
            return IsBlocked(task, pc);
        }

        public static bool IsBlocked(SystemConsole console, PlayerControl pc)
        {
            if (console == null || pc == null || pc != PlayerControl.LocalPlayer)
            {
                return false;
            }

            string name = console.name;
            bool isSecurity = name == "task_cams" || name == "Surv_Panel" || name == "SurvLogConsole" || name == "SurvConsole";
            bool isVitals = name == "panel_vitals";
            bool isButton = name == "EmergencyButton" || name == "EmergencyConsole" || name == "task_emergency";

            return false;
        }

        public static bool IsBlocked(IUsable target, PlayerControl pc)
        {
            if (target == null) return false;

            Console targetConsole = target.TryCast<Console>();
            SystemConsole targetSysConsole = target.TryCast<SystemConsole>();
            MapConsole targetMapConsole = target.TryCast<MapConsole>();
            if ((targetConsole != null && IsBlocked(targetConsole, pc)) ||
                (targetSysConsole != null && IsBlocked(targetSysConsole, pc)) ||
                (targetMapConsole != null))
            {
                return true;
            }
            return false;
        }

        

        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit)
        {
            try
            {
                Texture2D texture = loadTextureFromResources(path);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            }
            catch
            {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }
            return null;
        }

        public static Texture2D loadTextureFromResources(string path)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteTexture = new byte[stream.Length];
                var read = stream.Read(byteTexture, 0, (int)stream.Length);
                LoadImage(texture, byteTexture, false);
                return texture;
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from resources: " + path);
            }
            return null;
        }

        public static Texture2D loadTextureFromDisk(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    byte[] byteTexture = File.ReadAllBytes(path);
                    LoadImage(texture, byteTexture, false);
                    return texture;
                }
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from disk: " + path);
            }
            return null;
        }

        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage;
        private static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            if (iCall_LoadImage == null)
                iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2cppArray = (Il2CppStructArray<byte>)data;
            return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }


        public static Dictionary<byte, PlayerControl> allPlayersById()
        {
            Dictionary<byte, PlayerControl> res = new Dictionary<byte, PlayerControl>();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                res.Add(player.PlayerId, player);
            return res;
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

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
    }
}
