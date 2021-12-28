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
    [HarmonyPatch]
    class GameOptionsDataPatch
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

            StringBuilder entry = new StringBuilder();
            List<string> entries = new List<string>();

            var allOption = OptionsHolder.AllOptions;

            entries.Add(
                optionToString(allOption[(int)OptionsHolder.CommonOptionKey.PresetSelection]));

            entries.Add(
                optionToString(allOption[(int)OptionsHolder.CommonOptionKey.UseStrongRandomGen]));

            var optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("crewmateRoles"));
            var min = allOption[(int)OptionsHolder.CommonOptionKey.MinCremateRoles].GetValue();
            var max = allOption[(int)OptionsHolder.CommonOptionKey.MaxCremateRoles].GetValue();
            if (min > max) min = max;
            var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("neutralRoles"));
            min = allOption[(int)OptionsHolder.CommonOptionKey.MinNeutralRoles].GetValue();
            max = allOption[(int)OptionsHolder.CommonOptionKey.MaxNeutralRoles].GetValue();
            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = Design.ColoedString(
                new Color(204f / 255f, 204f / 255f, 0, 1f),
                translate("impostorRoles"));
            min = allOption[(int)OptionsHolder.CommonOptionKey.MinImpostorRoles].GetValue();
            max = allOption[(int)OptionsHolder.CommonOptionKey.MaxImpostorRoles].GetValue();

            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            entries.Add(entry.ToString().Trim('\r', '\n'));

            foreach (CustomOptionBase option in OptionsHolder.AllOptions.Values)
            {
                if ((option == allOption[(int)OptionsHolder.CommonOptionKey.PresetSelection]) ||
                    (option == allOption[(int)OptionsHolder.CommonOptionKey.UseStrongRandomGen]) ||
                    (option == allOption[(int)OptionsHolder.CommonOptionKey.MinCremateRoles]) ||
                    (option == allOption[(int)OptionsHolder.CommonOptionKey.MaxCremateRoles]) ||
                    (option == allOption[(int)OptionsHolder.CommonOptionKey.MinNeutralRoles]) ||
                    (option == allOption[(int)OptionsHolder.CommonOptionKey.MaxNeutralRoles]) ||
                    (option == allOption[(int)OptionsHolder.CommonOptionKey.MinImpostorRoles]) ||
                    (option == allOption[(int)OptionsHolder.CommonOptionKey.MaxImpostorRoles]))
                {
                    continue;
                }

                if (option.Parent == null)
                {
                    if (!option.Enabled)
                    {
                        continue;
                    }

                    entry = new StringBuilder();
                    if (!option.IsHidden)
                    {
                        entry.AppendLine(optionToString(option));
                    }
                    addChildren(option, ref entry, !option.IsHidden);
                    entries.Add(entry.ToString().Trim('\r', '\n'));
                }
            }

            int maxLines = 28;
            int lineCount = 0;
            string page = "";
            foreach (var e in entries)
            {
                int lines = e.Count(c => c == '\n') + 1;

                if (lineCount + lines > maxLines)
                {
                    pages.Add(page);
                    page = "";
                    lineCount = 0;
                }

                page = page + e + "\n\n";
                lineCount += lines + 1;
            }

            page = page.Trim('\r', '\n');
            if (page != "")
            {
                pages.Add(page);
            }

            int numPages = pages.Count;
            int counter = ExtremeRolesPlugin.OptionsPage = ExtremeRolesPlugin.OptionsPage % numPages;

            __result = pages[counter].Trim('\r', '\n') + "\n\n" + translate("pressTabForMore") + $" ({counter + 1}/{numPages})";

        }

        private static void addChildren(CustomOptionBase option, ref StringBuilder entry, bool indent = true)
        {
            if (!option.Enabled) { return; }

            foreach (var child in option.Children)
            {
                if (!child.IsHidden)
                {
                    entry.AppendLine((indent ? "    " : "") + optionToString(child));
                }
                addChildren(child, ref entry, indent);
            }
        }

        private static string optionToString(CustomOptionBase option)
        {
            if (option == null) { return ""; }
            return $"{option.GetName()}: {option.GetString()}";
        }
        private static string optionsToString(CustomOptionBase option, bool skipFirst = false)
        {
            if (option == null) { return ""; }

            List<string> options = new List<string>();
            if (!option.IsHidden && !skipFirst) options.Add(optionToString(option));
            if (option.Enabled)
            {
                foreach (CustomOptionBase op in option.Children)
                {
                    string str = optionToString(op);
                    if (str != "") options.Add(str);
                }
            }
            return string.Join("\n", options);
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
