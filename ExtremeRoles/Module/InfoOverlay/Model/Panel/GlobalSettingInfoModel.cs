using System;
using System.Text;
using System.Linq;

using ExtremeRoles.Helper;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Compat;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GameMode.Option.ShipGlobal;

namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class GlobalSettingInfoModel : IInfoOverlayPanelModel
{
	private StringBuilder printOption = new StringBuilder();

	public (string, string) GetInfoText()
	{
		this.printOption.Clear();

		foreach (OptionCreator.CommonOptionKey key in Enum.GetValues(
			typeof(OptionCreator.CommonOptionKey)))
		{
			if (key == OptionCreator.CommonOptionKey.PresetSelection) { continue; }

			addOptionString(ref this.printOption, key);
		}

		addRoleSpawnNumOptionHudString(ref this.printOption);
		addOptionString(ref this.printOption, RoleGlobalOption.UseXion);

		foreach (GlobalOption key in Enum.GetValues(typeof(GlobalOption)))
		{
			addOptionString(ref this.printOption, key);
		}

		string integrateOption = CompatModManager.Instance.GetIntegrateOptionHudString();
		if (!string.IsNullOrEmpty(integrateOption))
		{
			this.printOption.Append(integrateOption);
		}

		return (
			$"<size=125%>{Translation.GetString("vanilaOptions")}</size>\n{IGameOptionsExtensions.SettingsStringBuilder.ToString()}",
			$"<size=125%>{Translation.GetString("gameOption")}</size>\n{this.printOption}"
		);
	}

	private static void addOptionString<T>(
		ref StringBuilder builder, T optionKey) where T : struct, IConvertible
	{
		if (!OptionManager.Instance.TryGetIOption(
			Convert.ToInt32(optionKey), out IOptionInfo? option) ||
			option is null ||
			option.IsHidden)
		{
			return;
		}

		string optStr = option.ToHudString();
		if (optStr != string.Empty)
		{
			builder.AppendLine(optStr);
		}
	}

	private static void addRoleSpawnNumOptionHudString(ref StringBuilder builder)
	{
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
	}

	private static string createRoleSpawnNumOptionHudStringLine(
		string transKey, RoleGlobalOption minOptKey, RoleGlobalOption maxOptKey)
	{
		string optionName = Design.ColoedString(
						new UnityEngine.Color(204f / 255f, 204f / 255f, 0, 1f),
						Translation.GetString(transKey));
		int min = getSpawnOptionValue(minOptKey);
		int max = getSpawnOptionValue(maxOptKey);
		string optionValueStr = (min >= max) ? $"{max}" : $"{min} - {max}";

		return $"{optionName}: {optionValueStr}";
	}

	private static int getSpawnOptionValue(RoleGlobalOption optionKey)
		=> OptionManager.Instance.GetValue<int>((int)optionKey);
}
