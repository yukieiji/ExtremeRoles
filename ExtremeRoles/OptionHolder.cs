using System;
using System.Linq;

using UnityEngine;
using BepInEx.Configuration;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module;
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

        ClientOption.Create();

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

        var opt = ClientOption.Instance;

        var customRegion = new DnsRegionInfo(
            opt.Ip.Value,
            ServerManagerExtension.FullCustomServerName,
            StringNames.NoTranslation,
            opt.Ip.Value,
            opt.Port.Value,
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
}
