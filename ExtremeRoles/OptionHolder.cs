using System;
using System.Linq;

using UnityEngine;
using BepInEx.Configuration;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Extension.Manager;

namespace ExtremeRoles;

public static class OptionHolder
{
    private const int singleRoleOptionStartOffset = 256;
    private const int combRoleOptionStartOffset = 5000;
    private const int ghostRoleOptionStartOffset = 10000;
    private const int maxPresetNum = 20;

    public static readonly string[] SpawnRate = new string[] {
        "0%", "10%", "20%", "30%", "40%",
        "50%", "60%", "70%", "80%", "90%", "100%" };

    public static readonly string[] Range = new string[] { "short", "middle", "long" };

    private static IRegionInfo[] defaultRegion;

    private static Color defaultOptionColor => new Color(204f / 255f, 204f / 255f, 0, 1f);

    public enum CommonOptionKey : int
    {
        PresetSelection = 0,

        UseStrongRandomGen,
        UsePrngAlgorithm,
    }

    public static void Create()
    {

        defaultRegion = ServerManager.DefaultRegions;

        createConfigOption();

        Roles.ExtremeRoleManager.GameRole.Clear();

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
        
        MeetingReporter.Reset();

        Client.GhostsSeeRole = ConfigParser.GhostsSeeRoles.Value;
        Client.GhostsSeeTask = ConfigParser.GhostsSeeTasks.Value;
        Client.GhostsSeeVote = ConfigParser.GhostsSeeVotes.Value;
        Client.ShowRoleSummary = ConfigParser.ShowRoleSummary.Value;
        Client.HideNamePlate = ConfigParser.HideNamePlate.Value;
    }

    public static void UpdateRegion()
    {
        ServerManager serverManager = DestroyableSingleton<ServerManager>.Instance;
        IRegionInfo[] regions = defaultRegion;

        // Only ExtremeRoles!!
        var exrOfficialTokyo = new DnsRegionInfo(
            "168.138.196.31",
            ServerManagerExtension.ExROfficialServerTokyoManinName,
            StringNames.NoTranslation,
            "168.138.196.31",
            22023,
            false);

        var customRegion = new DnsRegionInfo(
            ConfigParser.Ip.Value,
            ServerManagerExtension.FullCustomServerName,
            StringNames.NoTranslation,
            ConfigParser.Ip.Value,
            ConfigParser.Port.Value,
            false);

        regions = regions.Concat(
            new IRegionInfo[]
            {
                exrOfficialTokyo.Cast<IRegionInfo>(),
                customRegion.Cast<IRegionInfo>()
            }).ToArray();

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
