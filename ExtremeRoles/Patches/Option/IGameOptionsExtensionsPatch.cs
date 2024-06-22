using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using HarmonyLib;

using ExtremeRoles.Extension.Strings;
using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.Compat;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GameMode.Option.ShipGlobal;

using ExtremeRoles.Module.CustomOption;

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
    public const int MaxLines = 28;
    private static int page = 0;

    public static void ChangePage(int num)
    {
        page += num;
    }

    public static void Postfix(ref string __result)
    {
        ExtremeGameModeManager egmm = ExtremeGameModeManager.Instance;

        if (egmm == null) { return; }

        List<string> hudOptionPage =
		[
            __result
        ];

        List<string> allOptionStr =
		[
            getHudString(OptionCreator.PresetOptionKey.PresetSelection),
			getHudString(OptionCreator.PresetOptionKey.UseRaiseHand),
			createRngOptionHudString(),
            createRoleSpawnNumOptionHudString()
        ];

        if (egmm.RoleSelector.CanUseXion)
        {
            allOptionStr.Add(
                Design.ColoedString(
                    ColorPalette.XionBlue,
                    getHudString(RoleSpawnOption.UseXion)));
        }

		egmm.ShipOption.AddHudString(allOptionStr);

        var allOption = OptionManager.Instance;

        foreach (IOptionInfo option in allOption.GetAllIOption())
        {
            int optionId = option.Id;

            if (Enum.IsDefined(typeof(OptionCreator.PresetOptionKey), optionId) ||
                Enum.IsDefined(typeof(RoleSpawnOption), optionId) ||
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

		string integrateOption = CompatModManager.Instance.GetIntegrateOptionHudString();
		if (!string.IsNullOrEmpty(integrateOption))
		{
			allOptionStr.Add(integrateOption);
		}

        int lineCount = 0;
        StringBuilder pageBuilder = new StringBuilder();
        foreach (string optionStr in allOptionStr)
        {
			int lines = optionStr.CountLine();

            if (lineCount + lines > MaxLines)
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
        builder
			.AppendLine(
				createRoleSpawnNumOptionHudStringLine(
					"crewmateRoles",
					RoleSpawnOption.MinCrewmateRoles,
					RoleSpawnOption.MaxCrewmateRoles))
			.AppendLine(
				createRoleSpawnNumOptionHudStringLine(
					"neutralRoles",
					RoleSpawnOption.MinNeutralRoles,
					RoleSpawnOption.MaxNeutralRoles))
			.AppendLine(
				createRoleSpawnNumOptionHudStringLine(
					"impostorRoles",
					RoleSpawnOption.MinImpostorRoles,
					RoleSpawnOption.MaxImpostorRoles))
			.AppendLine()

        // 幽霊役職周り
			.AppendLine(
				createRoleSpawnNumOptionHudStringLine(
					"crewmateGhostRoles",
					RoleSpawnOption.MinCrewmateGhostRoles,
					RoleSpawnOption.MaxCrewmateGhostRoles))
			.AppendLine(
				createRoleSpawnNumOptionHudStringLine(
					"neutralGhostRoles",
					RoleSpawnOption.MinNeutralGhostRoles,
					RoleSpawnOption.MaxNeutralGhostRoles))
			.AppendLine(
				createRoleSpawnNumOptionHudStringLine(
					"impostorGhostRoles",
					RoleSpawnOption.MinImpostorGhostRoles,
					RoleSpawnOption.MaxImpostorGhostRoles));

        return builder.ToString().Trim('\r', '\n');
    }

    private static string createRoleSpawnNumOptionHudStringLine(
        string transKey, RoleSpawnOption minOptKey, RoleSpawnOption maxOptKey)
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
        rngOptBuilder
			.AppendLine(
				getHudString(OptionCreator.PresetOptionKey.UseStrongRandomGen))
			.AppendLine(
				getHudString(OptionCreator.PresetOptionKey.UsePrngAlgorithm));

        return rngOptBuilder.ToString().Trim('\r', '\n');
    }

    private static string getHudString<T>(T optionKey) where T : struct, IConvertible
	{
		var option = OptionManager.Instance.GetIOption(Convert.ToInt32(optionKey));
		return option.ToHudString();
	}

    private static int getSpawnOptionValue(RoleSpawnOption optionKey)
        => OptionManager.Instance.GetValue<int>((int)optionKey);

    private static string translate(string key)
    {
        return Translation.GetString(key);
    }
}
