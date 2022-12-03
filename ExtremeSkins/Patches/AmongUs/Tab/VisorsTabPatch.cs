using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;

using ExtremeRoles.Performance;

using ExtremeSkins.Module;
using ExtremeSkins.Helper;
using ExtremeSkins.SkinManager;

using AmongUs.Data;
using AmongUs.Data.Player;

using ExRLoader = ExtremeRoles.Resources.Loader;

namespace ExtremeSkins.Patches.AmongUs.Tab
{
#if WITHVISOR
    [HarmonyPatch]
    public static class VisorsTabPatch
    {
        private static List<TMPro.TMP_Text> visorsTabCustomText = new List<TMPro.TMP_Text>();

        private static float inventoryTop = 1.5f;
        private static float inventoryBottom = -2.5f;

        public static CreatorTab Tab = null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.OnEnable))]
        public static bool VisorsTabOnEnablePrefix(VisorsTab __instance)
        {
            inventoryTop = __instance.scroller.Inner.position.y - 0.5f;
            inventoryBottom = __instance.scroller.Inner.position.y - 4.5f;

            VisorData[] unlockedVisor = DestroyableSingleton<HatManager>.Instance.GetUnlockedVisors();
            Dictionary<string, List<VisorData>> visorPackage = new Dictionary<string, List<VisorData>>();

            CustomCosmicTab.DestoryList(visorsTabCustomText);
            CustomCosmicTab.DestoryList(__instance.ColorChips.ToArray().ToList());
            CustomCosmicTab.RemoveAllTabs();

            visorsTabCustomText.Clear();
            __instance.ColorChips.Clear();

            if (CustomCosmicTab.textTemplate == null)
            {
                CustomCosmicTab.textTemplate = PlayerCustomizationMenu.Instance.itemName;
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


            foreach (VisorData viData in unlockedVisor)
            {
                CustomVisor vi;
                bool result = ExtremeVisorManager.VisorData.TryGetValue(
                    viData.ProductId, out vi);
                if (result)
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

            for (int i = 0; i < numVisor; i++)
            {
                VisorData vi = visores[i];

                ColorChip colorChip = CustomCosmicTab.SetColorChip(
                    __instance, i, offset);

                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    colorChip.Button.OnMouseOver.AddListener(
                        (UnityEngine.Events.UnityAction)(() => __instance.SelectVisor(vi)));
                    colorChip.Button.OnMouseOut.AddListener(
                        (UnityEngine.Events.UnityAction)(
                            () => __instance.SelectVisor(
                                DestroyableSingleton<HatManager>.Instance.GetVisorById(
                                    playerSkinData.Visor))));
                    colorChip.Button.OnClick.AddListener(
                        (UnityEngine.Events.UnityAction)(() => __instance.ClickEquip()));
                }
                else
                {
                    colorChip.Button.OnClick.AddListener(
                        (UnityEngine.Events.UnityAction)(() => __instance.SelectVisor(vi)));
                }

                colorChip.Inner.transform.localPosition = vi.ChipOffset;
                colorChip.ProductId = vi.ProductId;
                colorChip.Button.ClickMask = __instance.scroller.Hitbox;
                colorChip.Tag = vi.ProdId;
                
                int color = __instance.HasLocalPlayer() ? 
                    CachedPlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : 
                    playerSkinData.colorID;

                __instance.StartCoroutine(
                    vi.CoLoadViewData((Il2CppSystem.Action<VisorViewData>)((v) => {
                        colorChip.Inner.FrontLayer.sprite = v.IdleFrame;
                        __instance.UpdateSpriteMaterialColor(colorChip, v, color);
                })));

                __instance.ColorChips.Add(colorChip);
            }
        }
    }
#endif
}
