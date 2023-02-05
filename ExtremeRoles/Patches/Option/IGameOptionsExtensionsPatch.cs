using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AmongUs.GameOptions;
using UnityEngine;
using HarmonyLib;

using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Option.ShipGlobal;

namespace ExtremeRoles.Patches.Option
{
    [HarmonyPatch(
        typeof(IGameOptionsExtensions),
        nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    public static class IGameOptionsExtensionsNumImpostorsPatch
    {
        public static bool Prefix(ref int __result)
        {
            var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;

            if (currentGameOptions.GameMode != GameModes.Normal) { return true; }

            __result = currentGameOptions.GetInt(Int32OptionNames.NumImpostors);
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

            if (ExtremeGameModeManager.Instance == null) { return; }

            List<string> hudOptionPage = new List<string>()
            {
                __result
            };

            List<string> allOptionStr = new List<string>()
            {
                getOption(OptionHolder.CommonOptionKey.PresetSelection).ToHudString(),
                createRNGSetting(),
                createRoleSpawnNumOptions()
            };

            // Xionが使えるかはIRoleSelectorで定義されてるのでそれから
            allOptionStr.Add(
                Design.ColoedString(
                    ColorPalette.XionBlue,
                    getOption(OptionHolder.CommonOptionKey.UseXion).ToHudString()));

            allOptionStr.Add(ExtremeGameModeManager.Instance.ShipOption.ToHudString());

            foreach (IOption option in OptionHolder.AllOption.Values)
            {
                int optionId = option.Id;

                if (Enum.IsDefined(typeof(OptionHolder.CommonOptionKey), optionId) ||
                    Enum.IsDefined(typeof(GlobalOption), optionId))
                {
                    continue;
                }


                if (option.Parent == null &&
                    option.Enabled &&
                    ExtremeGameModeManager.Instance.RoleSelector.IsValidRoleOption(option))
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
            int counter = OptionHolder.OptionsPage = OptionHolder.OptionsPage % numPages;

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
            int min = getOption(OptionHolder.CommonOptionKey.MinCrewmateRoles).GetValue();
            int max = getOption(OptionHolder.CommonOptionKey.MaxCrewmateRoles).GetValue();
            if (min > max) { min = max; }
            string optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("neutralRoles"));
            min = getOption(OptionHolder.CommonOptionKey.MinNeutralRoles).GetValue();
            max = getOption(OptionHolder.CommonOptionKey.MaxNeutralRoles).GetValue();
            if (min > max) { min = max; }
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("impostorRoles"));
            min = getOption(OptionHolder.CommonOptionKey.MinImpostorRoles).GetValue();
            max = getOption(OptionHolder.CommonOptionKey.MaxImpostorRoles).GetValue();

            if (min > max) { min = max; }
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            entry.AppendLine();

            // 幽霊役職周り
            optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("crewmateGhostRoles"));
            min = getOption(OptionHolder.CommonOptionKey.MinCrewmateGhostRoles).GetValue();
            max = getOption(OptionHolder.CommonOptionKey.MaxCrewmateGhostRoles).GetValue();
            if (min > max) { min = max; }
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("neutralGhostRoles"));
            min = getOption(OptionHolder.CommonOptionKey.MinNeutralGhostRoles).GetValue();
            max = getOption(OptionHolder.CommonOptionKey.MaxNeutralGhostRoles).GetValue();
            if (min > max) { min = max; }
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("impostorGhostRoles"));
            min = getOption(OptionHolder.CommonOptionKey.MinImpostorGhostRoles).GetValue();
            max = getOption(OptionHolder.CommonOptionKey.MaxImpostorGhostRoles).GetValue();

            if (min > max) { min = max; }
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            return entry.ToString().Trim('\r', '\n');
        }

        private static string createRNGSetting()
        {
            StringBuilder rngOptBuilder = new StringBuilder();
            rngOptBuilder.AppendLine(
                getOption(OptionHolder.CommonOptionKey.UseStrongRandomGen).ToHudString());
            rngOptBuilder.AppendLine(
                getOption(OptionHolder.CommonOptionKey.UsePrngAlgorithm).ToHudString());

            return rngOptBuilder.ToString().Trim('\r', '\n');
        }

        private static string translate(string key)
        {
            return Translation.GetString(key);
        }

        private static IOption getOption(OptionHolder.CommonOptionKey optionKey)
            => OptionHolder.AllOption[(int)optionKey];
    }
}
