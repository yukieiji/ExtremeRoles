using System;
using System.Text;
using System.Linq;

using ExtremeRoles.Helper;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Compat;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GameMode.Option.ShipGlobal;

using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.NewOption.OLDS;

using OldRoleGlobalOption = ExtremeRoles.Module.NewOption.OLDS.RoleGlobalOption;

namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class GlobalSettingInfoModel : IInfoOverlayPanelModel
{
	private StringBuilder printOption = new StringBuilder();

	public (string, string) GetInfoText()
	{
		this.printOption.Clear();

		foreach (OptionCreator.PresetOptionKey key in Enum.GetValues(
			typeof(OptionCreator.PresetOptionKey)))
		{
			if (key == OptionCreator.PresetOptionKey.PresetSelection) { continue; }

			addOptionString(ref this.printOption, key);
		}

		addRoleSpawnNumOptionHudString(ref this.printOption);
		addOptionString(ref this.printOption, OldRoleGlobalOption.UseXion);

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
			$"<size=135%>{Translation.GetString("vanilaOptions")}</size>\n\n{IGameOptionsExtensions.SettingsStringBuilder.ToString()}",
			$"<size=135%>{Translation.GetString("gameOption")}</size>\n\n{this.printOption}"
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
				RoleSpawnOption.MinCrewmateRoles,
				RoleSpawnOption.MaxCrewmateRoles));
		builder.AppendLine(
			createRoleSpawnNumOptionHudStringLine(
				"neutralRoles",
				RoleSpawnOption.MinNeutralRoles,
				RoleSpawnOption.MaxNeutralRoles));
		builder.AppendLine(
			createRoleSpawnNumOptionHudStringLine(
				"impostorRoles",
				RoleSpawnOption.MinImpostorRoles,
				RoleSpawnOption.MaxImpostorRoles));

		// 幽霊役職周り
		builder.AppendLine(
			createRoleSpawnNumOptionHudStringLine(
				"crewmateGhostRoles",
				RoleSpawnOption.MinCrewmateGhostRoles,
				RoleSpawnOption.MaxCrewmateGhostRoles));
		builder.AppendLine(
			createRoleSpawnNumOptionHudStringLine(
				"neutralGhostRoles",
				RoleSpawnOption.MinNeutralGhostRoles,
				RoleSpawnOption.MaxNeutralGhostRoles));
		builder.AppendLine(
			createRoleSpawnNumOptionHudStringLine(
				"impostorGhostRoles",
				RoleSpawnOption.MinImpostorGhostRoles,
				RoleSpawnOption.MaxImpostorGhostRoles));
	}

	private static string createRoleSpawnNumOptionHudStringLine(
		string transKey, RoleSpawnOption minOptKey, RoleSpawnOption maxOptKey)
	{
		string optionName = Design.ColoedString(
						new UnityEngine.Color(204f / 255f, 204f / 255f, 0, 1f),
						Translation.GetString(transKey));
		int min = getSpawnOptionValue(minOptKey);
		int max = getSpawnOptionValue(maxOptKey);
		string optionValueStr = (min >= max) ? $"{max}" : $"{min} - {max}";

		return $"{optionName}: {optionValueStr}";
	}

	private static int getSpawnOptionValue(RoleSpawnOption optionKey)
		=> OptionManager.Instance.GetValue<int>((int)optionKey);
}
