using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Hazel;
using BepInEx.Configuration;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;

namespace ExtremeRoles
{
    public static class OptionHolder
    {
        private const int singleRoleOptionStartOffset = 256;
        private const int combRoleOptionStartOffset = 5000;
        private const int ghostRoleOptionStartOffset = 10000;
        private const int chunkSize = 50;
        private const int maxPresetNum = 20;

        public static readonly string[] SpawnRate = new string[] {
            "0%", "10%", "20%", "30%", "40%",
            "50%", "60%", "70%", "80%", "90%", "100%" };

        public static readonly string[] Range = new string[] { "short", "middle", "long" };

        public static string ConfigPreset
        {
            get => $"Preset:{selectedPreset}";
        }

        public static int OptionsPage = 1;

        public static Dictionary<int, IOption> AllOption = new Dictionary<int, IOption>();
        

        private static int selectedPreset = 0;
        private static bool isBlockShare = false;

        private static IRegionInfo[] defaultRegion;

        private static Color defaultOptionColor => new Color(204f / 255f, 204f / 255f, 0, 1f);

        public enum CommonOptionKey : int
        {
            PresetSelection = 0,

            UseStrongRandomGen,
            UsePrngAlgorithm,
        }

        public static void ExecuteWithBlockOptionShare(Action func)
        {
            isBlockShare = true;
            try
            {
                func();
            }
            catch (Exception e)
            {
                ExtremeRolesPlugin.Logger.LogInfo($"BlockShareExcuteFailed!!:{e}");
            }
            isBlockShare = false;
        }

        public static void Create()
        {

            defaultRegion = ServerManager.DefaultRegions;

            createConfigOption();

            Roles.ExtremeRoleManager.GameRole.Clear();
            AllOption.Clear();

            new IntCustomOption(
                (int)CommonOptionKey.PresetSelection, Design.ColoedString(
                    defaultOptionColor,
                    CommonOptionKey.PresetSelection.ToString()),
                1, 1, maxPresetNum, 1, null, true,
                format: OptionUnit.Preset);

            var strongGen = new BoolCustomOption(
                (int)CommonOptionKey.UseStrongRandomGen, Design.ColoedString(
                    defaultOptionColor,
                    CommonOptionKey.UseStrongRandomGen.ToString()), true);
            new SelectionCustomOption(
                (int)CommonOptionKey.UsePrngAlgorithm, Design.ColoedString(
                    defaultOptionColor,
                    CommonOptionKey.UsePrngAlgorithm.ToString()),
                new string[]
                {
                    "Pcg32XshRr", "Pcg64RxsMXs",
                    "Xorshift64", "Xorshift128",
                    "Xorshiro256StarStar",
                    "Xorshiro512StarStar",
                    "RomuMono", "RomuTrio", "RomuQuad",
                    "Seiran128", "Shioi128", "JFT32",
                },
                strongGen, invert: true);

            IRoleSelector.CreateRoleGlobalOption();
            IShipGlobalOption.Create();

            Roles.ExtremeRoleManager.CreateNormalRoleOptions(
                singleRoleOptionStartOffset);

            Roles.ExtremeRoleManager.CreateCombinationRoleOptions(
                combRoleOptionStartOffset);

            GhostRoles.ExtremeGhostRoleManager.CreateGhostRoleOption(
                ghostRoleOptionStartOffset);
        }

        public static void Load()
        {
            // 不具合等が発生しないようにブロック機能を有効化する
            isBlockShare = false;
            
            // ランダム生成機を設定を読み込んで作成
            RandomGenerator.Initialize();

            // ゲームモードのオプションロード
            ExtremeGameModeManager.Instance.Load();

            // 各役職を設定を読み込んで初期化する
            Roles.ExtremeRoleManager.Initialize();
            GhostRoles.ExtremeGhostRoleManager.Initialize();

            // 各種マップモジュール等のオプション値を読み込む
            Patches.MiniGame.VitalsMinigameUpdatePatch.LoadOptionValue();
            Patches.MiniGame.SecurityHelper.LoadOptionValue();
            Patches.MapOverlay.MapCountOverlayUpdatePatch.LoadOptionValue();

            Client.GhostsSeeRole = ConfigParser.GhostsSeeRoles.Value;
            Client.GhostsSeeTask = ConfigParser.GhostsSeeTasks.Value;
            Client.GhostsSeeVote = ConfigParser.GhostsSeeVotes.Value;
            Client.ShowRoleSummary = ConfigParser.ShowRoleSummary.Value;
            Client.HideNamePlate = ConfigParser.HideNamePlate.Value;
        }


        public static void SwitchPreset(int newPreset)
        {
            selectedPreset = newPreset;

            // オプションの共有でネットワーク帯域とサーバーに負荷をかけて人が落ちたりするので共有を一時的に無効化して実行
            ExecuteWithBlockOptionShare(
                () =>
                {
                    foreach (IOption option in AllOption.Values)
                    {
                        if (option.Id == 0) { continue; }
                        option.SwitchPreset();
                    }
                });
        }

        public static void ShareOptionSelections()
        {
            if (isBlockShare) { return; }

            if (PlayerControl.AllPlayerControls.Count <= 1 ||
                AmongUsClient.Instance?.AmHost == false &&
                PlayerControl.LocalPlayer == null) { return; }

            var splitOption = AllOption.Select((x, i) =>
                new { data = x, indexgroup = i / chunkSize })
                .GroupBy(x => x.indexgroup, x => x.data)
                .Select(y => y.Select(x => x));

            foreach (var chunkedOption in splitOption)
            {
                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.ShareOption))
                {
                    caller.WriteByte((byte)chunkedOption.Count());
                    foreach (var (id, option) in chunkedOption)
                    {
                        caller.WritePackedInt(id);
                        caller.WritePackedInt(option.CurSelection);
                    }
                }
            }
        }
        public static void ShareOption(int numberOfOptions, MessageReader reader)
        {
            try
            {
                for (int i = 0; i < numberOfOptions; i++)
                {
                    int optionId = reader.ReadPackedInt32();
                    int selection = reader.ReadPackedInt32();
                    lock (AllOption)
                    {
                        AllOption[optionId].UpdateSelection(selection);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Error($"Error while deserializing options:{e.Message}");
            }
        }

        public static void UpdateRegion()
        {
            ServerManager serverManager = DestroyableSingleton<ServerManager>.Instance;
            IRegionInfo[] regions = defaultRegion;

            var CustomRegion = new DnsRegionInfo(
                ConfigParser.Ip.Value,
                "custom",
                StringNames.NoTranslation,
                ConfigParser.Ip.Value,
                ConfigParser.Port.Value,
                false);
            regions = regions.Concat(new IRegionInfo[] { CustomRegion.Cast<IRegionInfo>() }).ToArray();
            ServerManager.DefaultRegions = regions;
            serverManager.AvailableRegions = regions;
        }

        private static void createConfigOption()
        {
            var config = ExtremeRolesPlugin.Instance.Config;

            ConfigParser.GhostsSeeTasks = config.Bind(
                "ClientOption", "GhostCanSeeRemainingTasks", true);
            ConfigParser.GhostsSeeRoles = config.Bind(
                "ClientOption", "GhostCanSeeRoles", true);
            ConfigParser.GhostsSeeVotes = config.Bind(
                "ClientOption", "GhostCanSeeVotes", true);
            ConfigParser.ShowRoleSummary = config.Bind(
                "ClientOption", "IsShowRoleSummary", true);
            ConfigParser.HideNamePlate = config.Bind(
                "ClientOption", "IsHideNamePlate", false);

            ConfigParser.StreamerModeReplacementText = config.Bind(
                "ClientOption",
                "ReplacementRoomCodeText",
                "Playing with Extreme Roles");

            ConfigParser.Ip = config.Bind(
                "ClientOption", "CustomServerIP", "127.0.0.1");
            ConfigParser.Port = config.Bind(
                "ClientOption", "CustomServerPort", (ushort)22023);
        }

        public static class ConfigParser
        {
            public static ConfigEntry<string> StreamerModeReplacementText { get; set; }
            public static ConfigEntry<bool> GhostsSeeTasks { get; set; }
            public static ConfigEntry<bool> GhostsSeeRoles { get; set; }
            public static ConfigEntry<bool> GhostsSeeVotes { get; set; }
            public static ConfigEntry<bool> ShowRoleSummary { get; set; }
            public static ConfigEntry<bool> HideNamePlate { get; set; }
            public static ConfigEntry<string> Ip { get; set; }
            public static ConfigEntry<ushort> Port { get; set; }
        }

        public static class Client
        {
            public static bool GhostsSeeRole = true;
            public static bool GhostsSeeTask = true;
            public static bool GhostsSeeVote = true;
            public static bool ShowRoleSummary = true;
            public static bool HideNamePlate = false;
        }
    }
}
