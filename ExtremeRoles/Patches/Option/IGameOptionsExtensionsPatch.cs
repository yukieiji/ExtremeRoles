using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AmongUs.GameOptions;
using UnityEngine;
using HarmonyLib;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Helper;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GameMode.Option.ShipGlobal;

namespace ExtremeRoles.Patches.Option;

[HarmonyPatch(
    typeof(IGameOptionsExtensions),
    nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
public static class IGameOptionsExtensionsNumImpostorsPatch
{
    public static bool Prefix(ref int __result)
    {
        if (ExtremeGameModeManager.Instance.RoleSelector.IsAdjustImpostorNum) { return true; }

        __result = Math.Clamp(
            GameOptionsManager.Instance.CurrentGameOptions.NumImpostors,
            0, GameData.Instance.PlayerCount);
        return false;
    }
}

[HarmonyPatch(
    typeof(IGameOptionsExtensions),
    nameof(IGameOptionsExtensions.ToHudString))]
public static class IGameOptionsExtensionsToHudStringPatch
{
    private const int maxLines = 28;
    private static int page = 0;

    public static void ChangePage(int num)
    {
        page += num;
    }

    public static void Postfix(ref string __result)
    {
        ExtremeGameModeManager egmm = ExtremeGameModeManager.Instance;

        if (egmm == null) { return; }

        List<string> hudOptionPage = new List<string>()
        {
            __result
        };

        List<string> allOptionStr = new List<string>()
        {
            getHudString(OptionCreator.CommonOptionKey.PresetSelection),
            createRngOptionHudString(),
            createRoleSpawnNumOptionHudString()
        };

        if (egmm.RoleSelector.CanUseXion)
        {
            allOptionStr.Add(
                Design.ColoedString(
                    ColorPalette.XionBlue,
                    getHudString(RoleGlobalOption.UseXion)));
        }

        allOptionStr.Add(egmm.ShipOption.ToHudString());

        var allOption = OptionManager.Instance;

        foreach (IOptionInfo option in allOption.GetAllIOption())
        {
            int optionId = option.Id;

            if (Enum.IsDefined(typeof(OptionCreator.CommonOptionKey), optionId) ||
                Enum.IsDefined(typeof(RoleGlobalOption), optionId) ||
                Enum.IsDefined(typeof(GlobalOption), optionId))
            {
                continue;
            }


            if (option.Parent == null &&
                option.Enabled &&
                egmm.RoleSelector.IsValidRoleOption(option))
            {
                string optionStr = option.ToHudStringWithChildren(option.IsHidden ? 0 : 1);
                allOptionStr.Add(optionStr.Trim('\r', '\n'));
            }
        }
        
        int lineCount = 0;
        StringBuilder pageBuilder = new StringBuilder();
        foreach (string optionStr in allOptionStr)
        {
            int lines = optionStr.Count(c => c == '\n') + 1;

            if (lineCount + lines > maxLines)
            {
                hudOptionPage.Add(pageBuilder.ToString());
                pageBuilder.Clear();
                lineCount = 0;
            }

            pageBuilder
                .Append(optionStr)
                .AppendLine("\n");
            lineCount += lines + 1;
        }

        if (pageBuilder.Length != 0)
        {
            hudOptionPage.Add(
                pageBuilder.ToString().Trim('\r', '\n'));
        }

        int numPages = hudOptionPage.Count;
        page %= numPages;

        __result = string.Concat(
            hudOptionPage[page].Trim('\r', '\n'),
            "\n\n",
            translate("pressTabForMore"),
            $" ({page + 1}/{numPages})");

    }

    private static string createRoleSpawnNumOptionHudString()
    {
        StringBuilder builder = new StringBuilder(512);

        // 生存役職周り
        builder.AppendLine(
            createRoleSpawnNumOptionHudStringLine(
                "crewmateRoles",
                RoleGlobalOption.MinCrewmateRoles,
                RoleGlobalOption.MaxCrewmateRoles));
        builder.AppendLine(
            createRoleSpawnNumOptionHudStringLine(
                "neutralRoles",
                RoleGlobalOption.MinNeutralRoles,
                RoleGlobalOption.MaxNeutralRoles));
        builder.AppendLine(
            createRoleSpawnNumOptionHudStringLine(
                "impostorRoles",
                RoleGlobalOption.MinImpostorRoles,
                RoleGlobalOption.MaxImpostorRoles));
        
        builder.AppendLine();

        // 幽霊役職周り
        builder.AppendLine(
            createRoleSpawnNumOptionHudStringLine(
                "crewmateGhostRoles",
                RoleGlobalOption.MinCrewmateGhostRoles,
                RoleGlobalOption.MaxCrewmateGhostRoles));
        builder.AppendLine(
            createRoleSpawnNumOptionHudStringLine(
                "neutralGhostRoles",
                RoleGlobalOption.MinNeutralGhostRoles,
                RoleGlobalOption.MaxNeutralGhostRoles));
        builder.AppendLine(
            createRoleSpawnNumOptionHudStringLine(
                "impostorGhostRoles",
                RoleGlobalOption.MinImpostorGhostRoles,
                RoleGlobalOption.MaxImpostorGhostRoles));

        return builder.ToString().Trim('\r', '\n');
    }

    private static string createRoleSpawnNumOptionHudStringLine(
        string transKey, RoleGlobalOption minOptKey, RoleGlobalOption maxOptKey)
    {
        string optionName = Design.ColoedString(
            new Color(204f / 255f, 204f / 255f, 0, 1f),
            translate(transKey));
        int min = getSpawnOptionValue(minOptKey);
        int max = getSpawnOptionValue(maxOptKey);
        string optionValueStr = (min >= max) ? $"{max}" : $"{min} - {max}";

        return $"{optionName}: {optionValueStr}";
    }

    private static string createRngOptionHudString()
    {
        StringBuilder rngOptBuilder = new StringBuilder();
        rngOptBuilder.AppendLine(
            getHudString(OptionCreator.CommonOptionKey.UseStrongRandomGen));
        rngOptBuilder.AppendLine(
            getHudString(OptionCreator.CommonOptionKey.UsePrngAlgorithm));

        return rngOptBuilder.ToString().Trim('\r', '\n');
    }

    private static string getHudString<T>(T optionKey) where T : struct, IConvertible
        => OptionManager.Instance.GetHudString(Convert.ToInt32(optionKey));

    private static int getSpawnOptionValue(RoleGlobalOption optionKey)
        => OptionManager.Instance.GetValue<int>((int)optionKey);

    private static string translate(string key)
    {
        return Translation.GetString(key);
    }
}
