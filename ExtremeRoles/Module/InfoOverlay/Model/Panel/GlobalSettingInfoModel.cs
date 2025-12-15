using System;
using System.Text;

using ExtremeRoles.Compat;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;

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

		this.printOption.AppendLine($"<size=135%>{Tr.GetString("gameOption")}</size>");
		addHudString(container, (int)OptionCreator.CommonOption.RandomOption, this.printOption);

		this.printOption.AppendLine($"{Tr.GetString("OptionCategory")}: {Tr.GetString("RoleSpawnCategory")}");
		addRoleSpawnNumOptionHudString(container, this.printOption);
		this.printOption.AppendLine();

		addHudString(
			container,
			ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Xion),
			this.printOption);

		addHudString(
			container,
			(int)SpawnOptionCategory.LiberalSetting,
			this.printOption);

		foreach (var key in Enum.GetValues<ShipGlobalOptionCategory>())
		{
			addHudString(container, (int)key, this.printOption);
		}

		foreach (int id in CompatModManager.Instance.GetIntegrateOptionCategoryId())
		{
			addHudString(container, id, this.printOption);
		}

		return (
			$"<size=135%>{Tr.GetString("vanilaOptions")}</size>\n\n{
				GameOptionsManager.Instance.currentGameOptions.ToHudString(
					PlayerControl.AllPlayerControls.Count)}",
			this.printOption.ToString()
		);
	}

	private static void addHudString(OptionTabContainer tab, int categoryId, in StringBuilder builder)
	{
		if (!tab.TryGetCategory(categoryId, out var category))
		{
			return;
		}

		builder.AppendLine($"{Tr.GetString("OptionCategory")}: {category.TransedName}");
	
		foreach (var opt in category.Options)
		{
			if (opt.Activator.Parent is null)
			{
				IInfoOverlayPanelModel.AddHudStringWithChildren(builder, opt);
			}
		}
		builder.AppendLine();
	}

	private static void addRoleSpawnNumOptionHudString(OptionTabContainer tab, in StringBuilder builder)
	{
		// 生存役職周り
		addSpawnNumOptionHudString(tab, SpawnOptionCategory.RoleSpawnCategory, builder, "Roles", true);
		// 幽霊役職周り
		addSpawnNumOptionHudString(tab, SpawnOptionCategory.GhostRoleSpawnCategory, builder, "GhostRoles");
	}

	private static void addSpawnNumOptionHudString(
		OptionTabContainer tab,
		SpawnOptionCategory categoryId,
		in StringBuilder builder,
		string transKey,
		bool includeLiberal = false)
	{
		if (!tab.TryGetCategory((int)categoryId, out var category))
		{
			return;
		}

		builder.Append('・');
		builder.AppendLine(
			createRoleSpawnNumOptionHudStringLine(
				category,
				$"crewmate{transKey}",
				RoleSpawnOption.MinCrewmate,
				RoleSpawnOption.MaxCrewmate));

		builder.Append('・');
		builder.AppendLine(
			createRoleSpawnNumOptionHudStringLine(
				category,
				$"neutral{transKey}",
				RoleSpawnOption.MinNeutral,
				RoleSpawnOption.MaxNeutral));
		
		builder.Append('・');
		builder.AppendLine(
			createRoleSpawnNumOptionHudStringLine(
				category,
				$"impostor{transKey}",
				RoleSpawnOption.MinImpostor,
				RoleSpawnOption.MaxImpostor));

		if (!includeLiberal)
		{
			return;
		}

		builder.Append('・');
		builder.AppendLine(
			createRoleSpawnNumOptionHudStringLine(
				category,
				$"liberal{transKey}",
				RoleSpawnOption.MinLiberal,
				RoleSpawnOption.MaxLiberal));
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
