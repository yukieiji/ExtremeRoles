using System;
using System.Text;

using ExtremeRoles.Helper;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Compat;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GameMode.Option.ShipGlobal;


using ExtremeRoles.Roles;
using ExtremeRoles.Module.CustomOption.OLDS;



namespace ExtremeRoles.Module.InfoOverlay.Model.Panel;

#nullable enable

public sealed class GlobalSettingInfoModel : IInfoOverlayPanelModel
{
	private StringBuilder printOption = new StringBuilder();
	private OptionTabContainer? container;

	public (string, string) GetInfoText()
	{
		this.printOption.Clear();
		if (container is null)
		{
			if (!OptionManager.Instance.TryGetTab(OptionTab.GeneralTab, out var tab))
			{
				return ("", "");
			}
			container = tab;
		}

		foreach (var key in Enum.GetValues<OptionCreator.CommonOption>())
		{
			tryAddHudString(container, (int)key, this.printOption);
		}

		this.printOption.AppendLine();

		this.printOption.AppendLine($"・{Tr.GetString("RoleSpawnCategory")}");
		addRoleSpawnNumOptionHudString(container, this.printOption);

		this.printOption.AppendLine();

		tryAddHudString(
			container,
			ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Xion),
			this.printOption);

		this.printOption.AppendLine();

		foreach (var key in Enum.GetValues<ShipGlobalOptionCategory>())
		{
			tryAddHudString(container, (int)key, this.printOption);
		}

		this.printOption.AppendLine();

		string integrateOption = CompatModManager.Instance.GetIntegrateOptionHudString();
		if (!string.IsNullOrEmpty(integrateOption))
		{
			this.printOption.Append(integrateOption);
		}

		this.printOption.AppendLine();

		return (
			$"<size=135%>{Tr.GetString("vanilaOptions")}</size>\n\n{
				GameOptionsManager.Instance.currentGameOptions.ToHudString(
					PlayerControl.AllPlayerControls.Count)}",
			$"<size=135%>{Tr.GetString("gameOption")}</size>\n\n{this.printOption}"
		);
	}

	private static void tryAddHudString(OptionTabContainer tab, int categoryId, in StringBuilder builder)
	{
		if (!tab.TryGetCategory(categoryId, out var category))
		{
			return;
		}
		category.AddHudString(builder);
	}

	private static void addRoleSpawnNumOptionHudString(OptionTabContainer tab, in StringBuilder builder)
	{
		// 生存役職周り
		addSpawnNumOptionHudString(tab, SpawnOptionCategory.RoleSpawnCategory, builder, "Roles");
		// 幽霊役職周り
		addSpawnNumOptionHudString(tab, SpawnOptionCategory.GhostRoleSpawnCategory, builder, "GhostRoles");
	}

	private static void addSpawnNumOptionHudString(
		OptionTabContainer tab,
		SpawnOptionCategory categoryId,
		in StringBuilder builder,
		string transKey)
	{
		if (!tab.TryGetCategory((int)categoryId, out var category))
		{
			return;
		}

		builder.AppendLine(
			createRoleSpawnNumOptionHudStringLine(
				category,
				$"crewmate{transKey}",
				RoleSpawnOption.MinCrewmate,
				RoleSpawnOption.MaxCrewmate));
		builder.AppendLine(
			createRoleSpawnNumOptionHudStringLine(
				category,
				$"neutral{transKey}",
				RoleSpawnOption.MinNeutral,
				RoleSpawnOption.MaxNeutral));
		builder.AppendLine(
			createRoleSpawnNumOptionHudStringLine(
				category,
				$"impostor{transKey}",
				RoleSpawnOption.MinImpostor,
				RoleSpawnOption.MaxImpostor));
	}

	private static string createRoleSpawnNumOptionHudStringLine(
		OptionCategory category,
		string transKey,
		RoleSpawnOption minOptKey,
		RoleSpawnOption maxOptKey)
	{
		string optionName = Design.ColoredString(
			new UnityEngine.Color(204f / 255f, 204f / 255f, 0, 1f),
			Tr.GetString(transKey));
		int min = getSpawnOptionValue(category, minOptKey);
		int max = getSpawnOptionValue(category, maxOptKey);
		string optionValueStr = (min >= max) ? $"{max}" : $"{min} - {max}";

		return $"{optionName}: {optionValueStr}";
	}

	private static int getSpawnOptionValue(OptionCategory category, RoleSpawnOption optionKey)
		=> category.GetValue<int>((int)optionKey);

	public void UpdateVisual()
	{

	}
}
