using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using HarmonyLib;

using ExtremeRoles.Extension.UnityEvents;

using ExtremeSkins.Module;
using ExtremeSkins.Helper;

using AmongUs.Data;
using AmongUs.Data.Player;

using ExRLoader = ExtremeRoles.Resources.UnityObjectLoader;

namespace ExtremeSkins.Patches.AmongUs.Tab
{
#if WITHVISOR
    [HarmonyPatch]
    public static class VisorsTabPatch
    {
        private static List<TMPro.TMP_Text> visorsTabCustomText = new List<TMPro.TMP_Text>();

        private static float inventoryTop = 1.5f;
        private static float inventoryBottom = -2.5f;

        public static CreatorTab? Tab = null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.OnEnable))]
        public static bool VisorsTabOnEnablePrefix(VisorsTab __instance)
        {
            inventoryTop = __instance.scroller.Inner.position.y - 0.5f;
            inventoryBottom = __instance.scroller.Inner.position.y - 4.5f;

            VisorData[] unlockedVisor = HatManager.Instance.GetUnlockedVisors();
            Dictionary<string, List<VisorData>> visorPackage = new Dictionary<string, List<VisorData>>();

            CustomCosmicTab.DestoryList(visorsTabCustomText);
            CustomCosmicTab.DestoryList(__instance.ColorChips.ToArray().ToList());
            CustomCosmicTab.RemoveAllTabs();

            visorsTabCustomText.Clear();
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
                    ExRLoader.LoadFromResources<GameObject>(
                        CustomCosmicTab.CreatorTabAssetBundle,
                        CustomCosmicTab.CreatorTabAssetPrefab,
						Assembly.GetExecutingAssembly()),
                    __instance.transform);
                Tab = obj.GetComponent<CreatorTab>();
                obj.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            }

			CustomCosmicTab.EnableTab(__instance);

			foreach (VisorData viData in unlockedVisor)
            {
                if (CosmicStorage<CustomVisor>.TryGet(
						viData.ProductId, out var vi) &&
					vi != null)
                {
                    if (!visorPackage.ContainsKey(vi.Author))
                    {
                        visorPackage.Add(vi.Author, new List<VisorData>());
                    }
                    visorPackage[vi.Author].Add(viData);
                }
                else
                {
                    if (!visorPackage.ContainsKey(CustomCosmicTab.InnerslothPackageName))
                    {
                        visorPackage.Add(
                            CustomCosmicTab.InnerslothPackageName,
                            new List<VisorData>());
                    }
                    visorPackage[CustomCosmicTab.InnerslothPackageName].Add(viData);
                }
            }

            float yOffset = __instance.YStart;

            var orderedKeys = visorPackage.Keys.OrderBy((string x) => {
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
                createVisorTab(visorPackage[key], key, yOffset, __instance);
                yOffset = (yOffset - (CustomCosmicTab.HeaderSize * __instance.YOffset)) - (
                    (visorPackage[key].Count - 1) / __instance.NumPerRow) * __instance.YOffset - CustomCosmicTab.HeaderSize;
            }

            __instance.scroller.ContentYBounds.max = -(yOffset + 3.0f + CustomCosmicTab.HeaderSize);

            Tab.gameObject.SetActive(true);
            Tab.SetUpButtons(__instance.scroller, visorsTabCustomText);

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.Update))]
        public static void VisorsTabUpdatePostfix(VisorsTab __instance)
        {
            CustomCosmicTab.HideTmpTextPackage(
                visorsTabCustomText, inventoryTop, inventoryBottom);
        }

        private static void createVisorTab(
            List<VisorData> visores, string packageName, float yStart, VisorsTab __instance)
        {
            float offset = yStart;


            CustomCosmicTab.AddTmpTextPackageName(
                __instance, yStart, packageName,
                ref visorsTabCustomText, ref offset);

            int numVisor = visores.Count;

            PlayerCustomizationData playerSkinData = DataManager.Player.Customization;

			int index = 0;
            foreach(var vi in visores)
            {
                ColorChip colorChip = CustomCosmicTab.SetColorChip(
                    __instance, index, offset);

                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    colorChip.Button.OnMouseOver.AddListener(() => __instance.SelectVisor(vi));
                    colorChip.Button.OnMouseOut.AddListener(
                            () => __instance.SelectVisor(
                                HatManager.Instance.GetVisorById(
                                    playerSkinData.Visor)));
                    colorChip.Button.OnClick.AddListener(() => __instance.ClickEquip());
                }
                else
                {
                    colorChip.Button.OnClick.AddListener(() => __instance.SelectVisor(vi));
                }

                colorChip.Inner.transform.localPosition = vi.ChipOffset;
                colorChip.ProductId = vi.ProductId;
                colorChip.Button.ClickMask = __instance.scroller.Hitbox;
                colorChip.Tag = vi.ProdId;

                int color = __instance.HasLocalPlayer() ?
                    PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId :
					DataManager.Player.Customization.Color;

				__instance.UpdateMaterials(colorChip.Inner.FrontLayer, vi);
				if (CosmicStorage<CustomVisor>.TryGet(
						vi.ProductId, out var customVi) &&
					customVi != null)
				{
					CustomCosmicTab.SetPreviewToRawSprite(colorChip, customVi.Preview, color);
				}
				else
				{
					vi.SetPreview(colorChip.Inner.FrontLayer, color);
				}

                __instance.ColorChips.Add(colorChip);

				index++;
				if (!HatManager.Instance.CheckLongModeValidCosmetic(
						vi.ProdId, __instance.PlayerPreview.GetIgnoreLongMode()))
				{
					colorChip.SetUnavailable();
				}
			}
        }
    }
#endif
}
