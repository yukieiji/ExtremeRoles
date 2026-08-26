using AmongUs.GameOptions;
using ExtremeRoles.Compat;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using HarmonyLib;
using UnityEngine;

namespace ExtremeRoles.Patches;

public static class MatchInfoGuideHelper
{
	public static void CreateModRoleEntry(MatchInfoGuide instance)
	{
		if (AmongUsClient.Instance == null ||
			TutorialManager.InstanceExists)
		{
			return;
		}

		var parent = instance.settingsTabs[2].GetComponent<Scroller>().Inner;
		var prehab = instance.MatchInfoRolePanelPrefab;

		int num = 0;

		foreach (var role in Roles.ExtremeRoleManager.NormalRole.Values)
		{
			if (role.Loader.TryGetValue(RoleCommonOption.SpawnRate, out int selection) &&
				role.Loader.TryGetValue(RoleCommonOption.RoleNum, out int roleNum) &&
				selection > 0 &&
				roleNum > 0)
			{
				createRoleEntry(prehab, parent, role.GetColoredRoleName(true), roleNum, selection);
				num++;
			}
		}

		foreach (var combRole in Roles.ExtremeRoleManager.CombRole.Values)
		{
			if (combRole.Loader.TryGetValue(RoleCommonOption.SpawnRate, out int selection) &&
				combRole.Loader.TryGetValue(RoleCommonOption.RoleNum, out int roleNum) &&
				selection > 0 &&
				roleNum > 0)
			{
				createRoleEntry(prehab, parent, combRole.GetOptionName(), roleNum, selection);
				num++;
			}
		}

		foreach (var role in GhostRoles.ExtremeGhostRoleManager.AllGhostRole.Values)
		{
			if (role.Loader.TryGetValue(RoleCommonOption.SpawnRate, out int selection) &&
				role.Loader.TryGetValue(RoleCommonOption.RoleNum, out int roleNum) &&
				selection > 0 &&
				roleNum > 0)
			{
				createRoleEntry(prehab, parent, role.GetColoredRoleName(), roleNum, selection);
				num++;
			}
		}

		foreach (RoleBehaviour roleBehaviour in DestroyableSingleton<RoleManager>.Instance.AllRoles)
		{
			if (roleBehaviour.Role != RoleTypes.Crewmate && roleBehaviour.Role != RoleTypes.Impostor && 
				roleBehaviour.Role != RoleTypes.CrewmateGhost && roleBehaviour.Role != RoleTypes.ImpostorGhost && 
				GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(roleBehaviour.Role) > 0)
			{
				num++;
			}
		}
		instance.rolesEnabledMessage.SetActive(num == 0);
		instance.MatchInfoRoleScroller.SetYBoundsMax(Mathf.Clamp(Mathf.Ceil(num / 2f) + instance.RoleEntryBoundsModifier, 0f, 999f));
		instance.MatchInfoRoleMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);
	}

	private static void createRoleEntry(
		MatchInfoRolePanel prefab,
		Transform parent,
		string roleName, int num, int chancePerGame)
	{
		var info = Object.Instantiate(prefab, parent);

		info.roleName.text = roleName;
		info.roleDescription.text = "";
		info.roleIcon.sprite = null;
		info.roleCount.text = $"{num} at {chancePerGame}%";
		info.roleIcon.material.SetInt(PlayerMaterial.MaskLayer, 50);
		info.roleName.fontMaterial.SetFloat(info.STENCIL_NAME, 50f);
		info.roleDescription.fontMaterial.SetFloat(info.STENCIL_NAME, 50f);
		info.roleCount.fontMaterial.SetFloat(info.STENCIL_NAME, 50f);
	}

	public static void CreateModSettingEntry(MatchInfoGuide instance)
	{
		if (!OptionManager.Instance.TryGetTab(OptionTab.GeneralTab, out var tab))
		{
			return;
		}

		addModSettingEntry(instance, tab, (int)OptionCreator.CommonOption.RandomOption);
		addModSettingEntry(instance, tab, (int)OptionCreator.CommonOption.RandomOption);
		
		addRoleSpawnNumOption(instance, tab);

		addModSettingEntry(instance, tab, ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Xion));
		addModSettingEntry(instance, tab, (int)SpawnOptionCategory.LiberalSetting);

		foreach (var key in System.Enum.GetValues<ShipGlobalOptionCategory>())
		{
			addModSettingEntry(instance, tab, (int)key);
		}

		foreach (int id in CompatModManager.Instance.GetIntegrateOptionCategoryId())
		{
			addModSettingEntry(instance, tab, id);
		}

		instance.matchInfoSettingsMaskArea.material.SetInt(PlayerMaterial.MaskLayer, 50);
	}

	private static void addRoleSpawnNumOption(MatchInfoGuide instance, OptionTabContainer tab)
	{
		// 生存役職周り
		addSpawnNumOption(instance, tab, SpawnOptionCategory.RoleSpawnCategory, "Roles", true);
		// 幽霊役職周り
		addSpawnNumOption(instance, tab, SpawnOptionCategory.GhostRoleSpawnCategory, "GhostRoles");
	}

	private static void addSpawnNumOption(
		MatchInfoGuide instance,
		OptionTabContainer tab,
		SpawnOptionCategory categoryId,
		string transKey,
		bool includeLiberal = false)
	{
		if (!tab.TryGetCategory((int)categoryId, out var category))
		{
			return;
		}

		addRoleTeamSpawnNumOption(
			instance,
			category,
			$"crewmate{transKey}",
			RoleSpawnOption.MinCrewmate,
			RoleSpawnOption.MaxCrewmate);

		addRoleTeamSpawnNumOption(
			instance,
			category,
			$"neutral{transKey}",
			RoleSpawnOption.MinNeutral,
			RoleSpawnOption.MaxNeutral);

		addRoleTeamSpawnNumOption(
			instance,
			category,
			$"impostor{transKey}",
			RoleSpawnOption.MinImpostor,
			RoleSpawnOption.MaxImpostor);

		if (!includeLiberal)
		{
			return;
		}

		addRoleTeamSpawnNumOption(
			instance,
			category,
			$"liberal{transKey}",
			RoleSpawnOption.MinLiberal,
			RoleSpawnOption.MaxLiberal);
	}

	private static void addRoleTeamSpawnNumOption(
		MatchInfoGuide instance,
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

		addModSettingEntry(instance, optionName, optionValueStr);
	}

	private static int getSpawnOptionValue(OptionCategory category, RoleSpawnOption optionKey)
		=> category.GetValue<int>((int)optionKey);

	private static void addModSettingEntry(
		MatchInfoGuide instance,
		OptionTabContainer tab,
		int categoryId)
	{
		if (!tab.TryGetCategory(categoryId, out var category))
		{
			return;
		}

		foreach (var opt in category.Options)
		{
			if (opt.Activator.Parent is null)
			{
				addModSettingEntoryWithChildren(instance, opt);
			}
		}
	}

	private static void addModSettingEntoryWithChildren(MatchInfoGuide instance, IOption option)
	{
		if (option.IsViewActive)
		{
			addModSettingEntry(instance, option);
		 }
		addChildrenOptionHudString(instance, option);
	}

	private static void addChildrenOptionHudString(
		MatchInfoGuide instance,
		IOption parentOption)
	{
		if (!OptionManager.Instance.TryGetChild(parentOption, out var child))
		{
			return;
		}
		foreach (var option in child)
		{
			if (option.IsViewActive)
			{
				addModSettingEntry(instance, option);
			}

			addChildrenOptionHudString(instance, option);
		}
	}

	private static void addModSettingEntry(MatchInfoGuide instance, in IOption option)
	{
		addModSettingEntry(instance, option.TransedTitle, option.TransedValue);
	}

	private static void addModSettingEntry(MatchInfoGuide instance, string title, string value)
	{
		var obj = Object.Instantiate(instance.MatchInfoSettingPrefab, instance.settingsScrollArea);
		if (obj.TryGetComponent<MatchInfoGuideSettingLabel>(out var component))
		{
			component.SetInfo(title, value);
		}
		instance.NormalModeSettings.Add(obj);
	}
}

[HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.CreateNormalModeSettings))]
public static class MatchInfoGuideCreateNormalModeSettingsPatch
{
	public static void Postfix(MatchInfoGuide __instance)
	{
		MatchInfoGuideHelper.CreateModSettingEntry(__instance);
		MatchInfoGuideHelper.CreateModRoleEntry(__instance);
	}
}

[HarmonyPatch(typeof(MatchInfoGuide), nameof(MatchInfoGuide.CreateHnSModeSettings))]
public static class MatchInfoGuideCreateHnSModeSettingsSettingsPatch
{
	public static void Postfix(MatchInfoGuide __instance)
	{
		MatchInfoGuideHelper.CreateModSettingEntry(__instance);
		MatchInfoGuideHelper.CreateModRoleEntry(__instance);

		if (__instance.rolesEnabledMessage.activeSelf)
		{
			__instance.TabButtons[2].gameObject.SetActive(true);
		}
	}
}