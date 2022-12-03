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
#if WITHHAT
    [HarmonyPatch]
    public static class HatsTabPatch
    {
        private static List<TMPro.TMP_Text> hatsTabCustomText = new List<TMPro.TMP_Text>();

        private static float inventoryTop = 1.5f;
        private static float inventoryBottom = -2.5f;

        public static CreatorTab Tab = null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
        public static bool HatsTabOnEnablePrefix(HatsTab __instance)
        {
            inventoryTop = __instance.scroller.Inner.position.y - 0.5f;
            inventoryBottom = __instance.scroller.Inner.position.y - 4.5f;

            HatData[] unlockedHats = DestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
            Dictionary<string, List<HatData>> hatPackage = new Dictionary<string, List<HatData>>();

            CustomCosmicTab.DestoryList(hatsTabCustomText);
            CustomCosmicTab.DestoryList(__instance.ColorChips.ToArray().ToList());
            CustomCosmicTab.RemoveAllTabs();

            hatsTabCustomText.Clear();
            __instance.ColorChips.Clear();

            if (CustomCosmicTab.textTemplate == null)
            {
                CustomCosmicTab.textTemplate = Object.Instantiate(
                    PlayerCustomizationMenu.Instance.itemName);
                CustomCosmicTab.textTemplate.gameObject.SetActive(false);
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

            foreach (HatData hatBehaviour in unlockedHats)
            {
                CustomHat hat;
                bool result = ExtremeHatManager.HatData.TryGetValue(
                    hatBehaviour.ProductId, out hat);
                if (result)
                {
                    if (!hatPackage.ContainsKey(hat.Author))
                    {
                        hatPackage.Add(hat.Author, new List<HatData>());
                    }
                    hatPackage[hat.Author].Add(hatBehaviour);
                }
                else
                {
                    if (!hatPackage.ContainsKey(CustomCosmicTab.InnerslothPackageName))
                    {
                        hatPackage.Add(
                            CustomCosmicTab.InnerslothPackageName,
                            new List<HatData>());
                    }
                    hatPackage[CustomCosmicTab.InnerslothPackageName].Add(hatBehaviour);
                }
            }

            float yOffset = __instance.YStart;

            var orderedKeys = hatPackage.Keys.OrderBy((string x) =>
            {
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
                createHatTab(hatPackage[key], key, yOffset, __instance);
                yOffset = (yOffset - (CustomCosmicTab.HeaderSize * __instance.YOffset)) - (
                    (hatPackage[key].Count - 1) / __instance.NumPerRow) * __instance.YOffset - CustomCosmicTab.HeaderSize;
            }

            __instance.scroller.ContentYBounds.max = -(yOffset + 3.0f + CustomCosmicTab.HeaderSize);
            Tab.gameObject.SetActive(true);
            Tab.SetUpButtons(__instance.scroller, hatsTabCustomText);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.Update))]
        public static void HatsTabUpdatePostfix()
        {
            CustomCosmicTab.HideTmpTextPackage(
                hatsTabCustomText, inventoryTop, inventoryBottom);
        }

        private static void createHatTab(
            List<HatData> hats, string packageName, float yStart, HatsTab __instance)
        {
            float offset = yStart;

            CustomCosmicTab.AddTmpTextPackageName(
                __instance, yStart, packageName,
                ref hatsTabCustomText, ref offset);

            int numHats = hats.Count;

            PlayerCustomizationData playerSkinData = DataManager.Player.Customization;

            for (int i = 0; i < numHats; i++)
            {
                HatData hat = hats[i];

                ColorChip colorChip = CustomCosmicTab.SetColorChip(__instance, i, offset);

                int color = __instance.HasLocalPlayer() ?
                    CachedPlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId :
                    playerSkinData.colorID;

                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    colorChip.Button.OnMouseOver.AddListener(
                        (UnityEngine.Events.UnityAction)(() => __instance.SelectHat(hat)));
                    colorChip.Button.OnMouseOut.AddListener(
                        (UnityEngine.Events.UnityAction)(
                            () =>
                            {
                                __instance.SelectHat(
                                    FastDestroyableSingleton<HatManager>.Instance.GetHatById(
                                        playerSkinData.Hat));
                            }));
                    colorChip.Button.OnClick.AddListener(
                        (UnityEngine.Events.UnityAction)(() => __instance.ClickEquip()));
                }
                else
                {
                    colorChip.Button.OnClick.AddListener(
                        (UnityEngine.Events.UnityAction)(() => __instance.SelectHat(hat)));
                }

                colorChip.Inner.SetMaskType(PlayerMaterial.MaskType.ScrollingUI);
                colorChip.Inner.SetHat(hat, color);
                colorChip.Inner.transform.localPosition = hat.ChipOffset;
                colorChip.Tag = hat;
                colorChip.Button.ClickMask = __instance.scroller.Hitbox;
                __instance.ColorChips.Add(colorChip);
            }
        }
    }
#endif
}
