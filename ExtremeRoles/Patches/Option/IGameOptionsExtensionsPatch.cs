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

    private static void Postfix(ref string __result)
    {

        ExtremeGameModeManager egmm = ExtremeGameModeManager.Instance;

        if (egmm == null) { return; }

        List<string> hudOptionPage = new List<string>()
        {
            __result
        };

        List<string> allOptionStr = new List<string>()
        {
            getHudString(AllOptionCreator.CommonOptionKey.PresetSelection),
            createRNGSetting(),
            createRoleSpawnNumOptions()
        };

        if (egmm.RoleSelector.CanUseXion)
        {
            allOptionStr.Add(
                Design.ColoedString(
                    ColorPalette.XionBlue,
                    getHudString(RoleGlobalOption.UseXion)));
        }

        allOptionStr.Add(egmm.ShipOption.ToHudString());

        var allOption = AllOptionHolder.Instance;

        foreach (IOptionInfo option in allOption.GetAllIOption())
        {
            int optionId = option.Id;

            if (Enum.IsDefined(typeof(AllOptionCreator.CommonOptionKey), optionId) ||
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
        int counter = allOption.OptionPage = allOption.OptionPage % numPages;

        __result = string.Concat(
            hudOptionPage[counter].Trim('\r', '\n'),
            "\n\n",
            translate("pressTabForMore"),
            $" ({counter + 1}/{numPages})");

    }

    private static string createRoleSpawnNumOptions()
    {
        StringBuilder entry = new StringBuilder();

        // 生存役職周り
        string optionName = Design.ColoedString(
            new Color(204f / 255f, 204f / 255f, 0, 1f),
            translate("crewmateRoles"));
        int min = getSpawnOptionValue(RoleGlobalOption.MinCrewmateRoles);
        int max = getSpawnOptionValue(RoleGlobalOption.MaxCrewmateRoles);
        if (min > max) { min = max; }
        string optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
        entry.AppendLine($"{optionName}: {optionValue}");

        optionName = Design.ColoedString(
            new Color(204f / 255f, 204f / 255f, 0, 1f),
            translate("neutralRoles"));
        min = getSpawnOptionValue(RoleGlobalOption.MinNeutralRoles);
        max = getSpawnOptionValue(RoleGlobalOption.MaxNeutralRoles);
        if (min > max) { min = max; }
        optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
        entry.AppendLine($"{optionName}: {optionValue}");

        optionName = Design.ColoedString(
            new Color(204f / 255f, 204f / 255f, 0, 1f),
            translate("impostorRoles"));
        min = getSpawnOptionValue(RoleGlobalOption.MinImpostorRoles);
        max = getSpawnOptionValue(RoleGlobalOption.MaxImpostorRoles);

        if (min > max) { min = max; }
        optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
        entry.AppendLine($"{optionName}: {optionValue}");

        entry.AppendLine();

        // 幽霊役職周り
        optionName = Design.ColoedString(
            new Color(204f / 255f, 204f / 255f, 0, 1f),
            translate("crewmateGhostRoles"));
        min = getSpawnOptionValue(RoleGlobalOption.MinCrewmateGhostRoles);
        max = getSpawnOptionValue(RoleGlobalOption.MaxCrewmateGhostRoles);
        if (min > max) { min = max; }
        optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
        entry.AppendLine($"{optionName}: {optionValue}");

        optionName = Design.ColoedString(
            new Color(204f / 255f, 204f / 255f, 0, 1f),
            translate("neutralGhostRoles"));
        min = getSpawnOptionValue(RoleGlobalOption.MinNeutralGhostRoles);
        max = getSpawnOptionValue(RoleGlobalOption.MaxNeutralGhostRoles);
        if (min > max) { min = max; }
        optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
        entry.AppendLine($"{optionName}: {optionValue}");

        optionName = Design.ColoedString(
            new Color(204f / 255f, 204f / 255f, 0, 1f),
            translate("impostorGhostRoles"));
        min = getSpawnOptionValue(RoleGlobalOption.MinImpostorGhostRoles);
        max = getSpawnOptionValue(RoleGlobalOption.MaxImpostorGhostRoles);

        if (min > max) { min = max; }
        optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
        entry.AppendLine($"{optionName}: {optionValue}");

        return entry.ToString().Trim('\r', '\n');
    }

    private static string createRNGSetting()
    {
        StringBuilder rngOptBuilder = new StringBuilder();
        rngOptBuilder.AppendLine(
            getHudString(AllOptionCreator.CommonOptionKey.UseStrongRandomGen));
        rngOptBuilder.AppendLine(
            getHudString(AllOptionCreator.CommonOptionKey.UsePrngAlgorithm));

        return rngOptBuilder.ToString().Trim('\r', '\n');
    }

    private static string translate(string key)
    {
        return Translation.GetString(key);
    }

    private static string getHudString<T>(T optionKey) where T : struct, IConvertible
        => AllOptionHolder.Instance.GetHudString(Convert.ToInt32(optionKey));

    private static int getSpawnOptionValue(RoleGlobalOption optionKey)
        => AllOptionHolder.Instance.GetValue<int>(Convert.ToInt32(optionKey));
}
