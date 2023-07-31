using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Performance;

using ExtremeSkins.Module;
using ExtremeSkins.Helper;
using ExtremeSkins.SkinManager;

using AmongUs.Data;
using AmongUs.Data.Player;
using Innersloth.Assets;

using ExRLoader = ExtremeRoles.Resources.Loader;

#nullable enable

namespace ExtremeSkins.Patches.AmongUs.Tab;

#if WITHNAMEPLATE
[HarmonyPatch]
public static class NameplatesTabPatch
{
    private static List<TMPro.TMP_Text> namePlateTabCustomText = new List<TMPro.TMP_Text>();

    private static float inventoryTop = 1.5f;
    private static float inventoryBottom = -2.5f;

    public static CreatorTab? Tab = null;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NameplatesTab), nameof(NameplatesTab.OnEnable))]
    public static bool NameplatesTabOnEnablePrefix(NameplatesTab __instance)
    {
        inventoryTop = __instance.scroller.Inner.position.y - 0.5f;
        inventoryBottom = __instance.scroller.Inner.position.y - 4.5f;

        NamePlateData[] unlockedNamePlate = DestroyableSingleton<HatManager>.Instance.GetUnlockedNamePlates();
        Dictionary<string, List<NamePlateData>> namePlatePackage = new Dictionary<string, List<NamePlateData>>();

        CustomCosmicTab.DestoryList(namePlateTabCustomText);
        CustomCosmicTab.DestoryList(__instance.ColorChips.ToArray().ToList());
        CustomCosmicTab.RemoveAllTabs();

        namePlateTabCustomText.Clear();
        __instance.ColorChips.Clear();

        if (CustomCosmicTab.TextTemplate == null)
        {
            CustomCosmicTab.TextTemplate = Object.Instantiate(
                PlayerCustomizationMenu.Instance.itemName);
            CustomCosmicTab.TextTemplate.gameObject.SetActive(false);
        }

        if (Tab == null)
        {
            GameObject obj = Object.Instantiate(
                ExRLoader.GetUnityObjectFromResources<GameObject>(
                    CustomCosmicTab.CreatorTabAssetBundle,
                    CustomCosmicTab.CreatorTabAssetPrefab),
                __instance.transform);
            Tab = obj.GetComponent<CreatorTab>();
            obj.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
        }

		CustomCosmicTab.EnableTab(__instance);
		__instance.PlayerPreview.gameObject.SetActive(false);
		__instance.StartCoroutine(__instance.CoLoadNameplatePreview());

		foreach (NamePlateData npData in unlockedNamePlate)
        {
            if (ExtremeNamePlateManager.NamePlateData.TryGetValue(
					npData.ProductId, out CustomNamePlate? np) &&
				np != null)
            {
                if (!namePlatePackage.ContainsKey(np.Author))
                {
                    namePlatePackage.Add(np.Author, new List<NamePlateData>());
                }
                namePlatePackage[np.Author].Add(npData);
            }
            else
            {
                if (!namePlatePackage.ContainsKey(CustomCosmicTab.InnerslothPackageName))
                {
                    namePlatePackage.Add(
                        CustomCosmicTab.InnerslothPackageName,
                        new List<NamePlateData>());
                }
                namePlatePackage[CustomCosmicTab.InnerslothPackageName].Add(npData);
            }
        }

        float yOffset = __instance.YStart;

        var orderedKeys = namePlatePackage.Keys.OrderBy((string x) => {
            if (x == CustomCosmicTab.InnerslothPackageName)
            {
                return 0;
            }
            else
            {
                return 100;
            }
        });

        foreach (string key in orderedKeys)
        {
            createNamePlateTab(namePlatePackage[key], key, yOffset, __instance);
            yOffset = (yOffset - (CustomCosmicTab.HeaderSize * __instance.YOffset)) - (
                (namePlatePackage[key].Count - 1) / __instance.NumPerRow) * __instance.YOffset - CustomCosmicTab.HeaderSize;
        }

		if (unlockedNamePlate.Length != 0)
		{
			__instance.GetDefaultSelectable().PlayerEquippedForeground.SetActive(true);
		}
		__instance.plateId = DataManager.Player.Customization.NamePlate;
		__instance.currentNameplateIsEquipped = true;

		__instance.scroller.ContentYBounds.max = -(yOffset + 3.0f + CustomCosmicTab.HeaderSize);

        Tab.gameObject.SetActive(true);
        Tab.SetUpButtons(__instance.scroller, namePlateTabCustomText);

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NameplatesTab), nameof(NameplatesTab.Update))]
    public static void NameplatesTabUpdatePostfix(NameplatesTab __instance)
    {
        CustomCosmicTab.HideTmpTextPackage(
            namePlateTabCustomText, inventoryTop, inventoryBottom);
    }

    private static void createNamePlateTab(
        List<NamePlateData> namePlates, string packageName, float yStart, NameplatesTab __instance)
    {
        float offset = yStart;


        CustomCosmicTab.AddTmpTextPackageName(
            __instance, yStart, packageName,
            ref namePlateTabCustomText, ref offset);

        int numHats = namePlates.Count;

        PlayerCustomizationData playerSkinData = DataManager.Player.Customization;

        for (int i = 0; i < numHats; i++)
        {
            NamePlateData np = namePlates[i];

            ColorChip colorChip = CustomCosmicTab.SetColorChip(
                __instance, i, offset);

			var button = colorChip.Button;

			button.ClickMask = __instance.scroller.Hitbox;

            if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
            {
				button.OnMouseOver.AddListener(() => __instance.SelectNameplate(np));
				button.OnMouseOut.AddListener(() => __instance.SelectNameplate(
                    FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById(
                        playerSkinData.NamePlate)));
				button.OnClick.AddListener(() => __instance.ClickEquip());
            }
            else
            {
				button.OnClick.AddListener(() => __instance.SelectNameplate(np));
            }

            __instance.StartCoroutine(
			__instance.CoLoadAssetAsync(
				np.Cast<IAddressableAssetProvider<NamePlateViewData>>(),
				(Il2CppSystem.Action<NamePlateViewData>)((n) => {
					colorChip.GetComponent<NameplateChip>().image.sprite = n.Image;
				})));
            colorChip.ProductId = np.ProdId;
            __instance.ColorChips.Add(colorChip);
        }
    }
}
#endif
