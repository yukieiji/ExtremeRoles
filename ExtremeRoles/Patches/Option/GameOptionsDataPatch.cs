using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;


namespace ExtremeRoles.Patches.Option
{

    [HarmonyPatch(typeof(GameOptionsData), "GameHostOptions", MethodType.Getter)]
    public static class GameOptionsDataGameHostOptionsPatch
    {
        private static int numImpostors;
        public static void Prefix()
        {
            if (GameOptionsData.hostOptionsData == null)
            {
                GameOptionsData.hostOptionsData = GameOptionsData.LoadGameHostOptions();
            }

            numImpostors = GameOptionsData.hostOptionsData.NumImpostors;
        }

        public static void Postfix(ref GameOptionsData __result)
        {
            __result.NumImpostors = numImpostors;
        }
    }

    [HarmonyPatch]
    public static class GameOptionsDataPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(GameOptionsData).GetMethods().Where(
                x => x.ReturnType == typeof(string) &&
                x.GetParameters().Length == 1 &&
                x.GetParameters()[0].ParameterType == typeof(int));
        }

        private static void Postfix(ref string __result)
        {

            List<string> pages = new List<string>();
            pages.Add(__result);
        
            List<string> allOptionStr = new List<string>();

            var allOption = OptionHolder.AllOption;

            allOptionStr.Add(
                CustomOption.OptionToString(allOption[(int)OptionHolder.CommonOptionKey.PresetSelection]));

            StringBuilder rngOptBuilder = new StringBuilder();
            rngOptBuilder.AppendLine(
                CustomOption.OptionToString(allOption[(int)OptionHolder.CommonOptionKey.UseStrongRandomGen]));
            rngOptBuilder.AppendLine(
                CustomOption.OptionToString(allOption[(int)OptionHolder.CommonOptionKey.UsePrngAlgorithm]));
            allOptionStr.Add(rngOptBuilder.ToString().Trim('\r', '\n'));

            allOptionStr.Add(createRoleSpawnNumOptions());

            allOptionStr.Add(
                Design.ColoedString(
                    ColorPalette.XionBlue,
                    CustomOption.OptionToString(
                        allOption[(int)OptionHolder.CommonOptionKey.UseXion])));

            StringBuilder modOptionStrBuilder = new StringBuilder();

            foreach (OptionHolder.CommonOptionKey id in Enum.GetValues(typeof(OptionHolder.CommonOptionKey)))
            {
                switch(id)
                {
                    case OptionHolder.CommonOptionKey.PresetSelection:
                    case OptionHolder.CommonOptionKey.UseStrongRandomGen:
                    case OptionHolder.CommonOptionKey.UsePrngAlgorithm:
                    case OptionHolder.CommonOptionKey.MinCrewmateRoles:
                    case OptionHolder.CommonOptionKey.MaxCrewmateRoles:
                    case OptionHolder.CommonOptionKey.MinNeutralRoles:
                    case OptionHolder.CommonOptionKey.MaxNeutralRoles:
                    case OptionHolder.CommonOptionKey.MinImpostorRoles:
                    case OptionHolder.CommonOptionKey.MaxImpostorRoles:
                    case OptionHolder.CommonOptionKey.MinCrewmateGhostRoles:
                    case OptionHolder.CommonOptionKey.MaxCrewmateGhostRoles:
                    case OptionHolder.CommonOptionKey.MinNeutralGhostRoles:
                    case OptionHolder.CommonOptionKey.MaxNeutralGhostRoles:
                    case OptionHolder.CommonOptionKey.MinImpostorGhostRoles:
                    case OptionHolder.CommonOptionKey.MaxImpostorGhostRoles:
                    case OptionHolder.CommonOptionKey.UseXion:
                        continue;
                    default:
                        break;
                }
                string optionStr = CustomOption.OptionToString(allOption[(int)id]);
                if (optionStr != string.Empty) { modOptionStrBuilder.AppendLine(optionStr); }
            }

            allOptionStr.Add(modOptionStrBuilder.ToString().Trim('\r', '\n'));

            foreach (IOption option in OptionHolder.AllOption.Values)
            {
                if (Enum.IsDefined(typeof(OptionHolder.CommonOptionKey), option.Id))
                {
                    continue;
                }


                if (option.Parent == null)
                {
                    if (!option.Enabled)
                    {
                        continue;
                    }

                    StringBuilder optionStrBuilder = new StringBuilder();
                    if (!option.IsHidden)
                    {
                        optionStrBuilder.AppendLine(CustomOption.OptionToString(option));
                    }

                    addChildren(option, ref optionStrBuilder, option.IsHidden ? 0 : 1);
                    allOptionStr.Add(optionStrBuilder.ToString().Trim('\r', '\n'));
                }
            }

            int maxLines = 28;
            int lineCount = 0;
            string page = "";
            foreach (string optionStr in allOptionStr)
            {
                int lines = optionStr.Count(c => c == '\n') + 1;

                if (lineCount + lines > maxLines)
                {
                    pages.Add(page);
                    page = "";
                    lineCount = 0;
                }

                page = string.Concat(page, optionStr, "\n\n");
                lineCount += lines + 1;
            }

            page = page.Trim('\r', '\n');
            if (page != "")
            {
                pages.Add(page);
            }

            int numPages = pages.Count;
            int counter = OptionHolder.OptionsPage = OptionHolder.OptionsPage % numPages;

            __result = string.Concat(
                pages[counter].Trim('\r', '\n'),
                "\n\n",
                translate("pressTabForMore"),
                $" ({counter + 1}/{numPages})");

        }

        private static void addChildren(IOption option, ref StringBuilder entry, int indentCount = 0)
        {

            string indent = "";

            if (indentCount != 0)
            {
                indent = string.Concat(
                    Enumerable.Repeat("    ", indentCount));
            }

            foreach (var child in option.Children)
            {

                string optionString = CustomOption.OptionToString(child);
                if (optionString != string.Empty)
                {
                    entry.AppendLine(
                        string.Concat(
                            indent,
                            optionString));
                }
                if (indentCount == 0)
                {
                    addChildren(child, ref entry, 0);
                }
                else
                {
                    addChildren(child, ref entry, indentCount + 1);
                }
            }
        }

        private static string createRoleSpawnNumOptions()
        {
            var allOption = OptionHolder.AllOption;

            StringBuilder entry = new StringBuilder();

            // 生存役職周り
            string optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("crewmateRoles"));
            int min = allOption[(int)OptionHolder.CommonOptionKey.MinCrewmateRoles].GetValue();
            int max = allOption[(int)OptionHolder.CommonOptionKey.MaxCrewmateRoles].GetValue();
            if (min > max) { min = max; }
            string optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("neutralRoles"));
            min = allOption[(int)OptionHolder.CommonOptionKey.MinNeutralRoles].GetValue();
            max = allOption[(int)OptionHolder.CommonOptionKey.MaxNeutralRoles].GetValue();
            if (min > max) { min = max; }
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("impostorRoles"));
            min = allOption[(int)OptionHolder.CommonOptionKey.MinImpostorRoles].GetValue();
            max = allOption[(int)OptionHolder.CommonOptionKey.MaxImpostorRoles].GetValue();

            if (min > max) { min = max; }
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            entry.AppendLine("");

            // 幽霊役職周り
            optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("crewmateGhostRoles"));
            min = allOption[(int)OptionHolder.CommonOptionKey.MinCrewmateGhostRoles].GetValue();
            max = allOption[(int)OptionHolder.CommonOptionKey.MaxCrewmateGhostRoles].GetValue();
            if (min > max) { min = max; }
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("neutralGhostRoles"));
            min = allOption[(int)OptionHolder.CommonOptionKey.MinNeutralGhostRoles].GetValue();
            max = allOption[(int)OptionHolder.CommonOptionKey.MaxNeutralGhostRoles].GetValue();
            if (min > max) { min = max; }
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("impostorGhostRoles"));
            min = allOption[(int)OptionHolder.CommonOptionKey.MinImpostorGhostRoles].GetValue();
            max = allOption[(int)OptionHolder.CommonOptionKey.MaxImpostorGhostRoles].GetValue();

            if (min > max) { min = max; }
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            return entry.ToString().Trim('\r', '\n');
        }

        private static string translate(string key)
        {
            return Translation.GetString(key);
        }

    }

    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.GetAdjustedNumImpostors))]
    public static class GameOptionsGetAdjustedNumImpostorsPatch
    {
        public static bool Prefix(GameOptionsData __instance, ref int __result)
        {
            __result = PlayerControl.GameOptions.NumImpostors;
            return false;
        }
    }
}
