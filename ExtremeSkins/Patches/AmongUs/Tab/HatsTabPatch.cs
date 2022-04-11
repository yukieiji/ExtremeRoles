using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;

using ExtremeSkins.Module;
using ExtremeSkins.Helper;
using ExtremeSkins.SkinManager;

namespace ExtremeSkins.Patches.AmongUs.Tab
{
#if WITHHAT
    [HarmonyPatch]
    public class HatsTabPatch
    {
        private static List<TMPro.TMP_Text> hatsTabCustomText = new List<TMPro.TMP_Text>();

        private static float inventoryTop = 1.5f;
        private static float inventoryBottom = -2.5f;


        [HarmonyPrefix]
        [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
        public static bool HatsTabOnEnablePrefix(HatsTab __instance)
        {
            inventoryTop = __instance.scroller.Inner.position.y - 0.5f;
            inventoryBottom = __instance.scroller.Inner.position.y - 4.5f;

            HatData[] unlockedHats = DestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
            Dictionary<string, List<HatData>> hatPackage = new Dictionary<string, List<HatData>>();

            SkinTab.DestoryList(hatsTabCustomText);
            SkinTab.DestoryList(__instance.ColorChips.ToArray().ToList());

            hatsTabCustomText.Clear();
            __instance.ColorChips.Clear();

            if (SkinTab.textTemplate == null)
            {
                SkinTab.textTemplate = PlayerCustomizationMenu.Instance.itemName;
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
                    if (!hatPackage.ContainsKey(SkinTab.InnerslothPackageName))
                    {
                        hatPackage.Add(
                            SkinTab.InnerslothPackageName,
                            new List<HatData>());
                    }
                    hatPackage[SkinTab.InnerslothPackageName].Add(hatBehaviour);
                }
            }

            float yOffset = __instance.YStart;

            var orderedKeys = hatPackage.Keys.OrderBy((string x) => {
                if (x == SkinTab.InnerslothPackageName)
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
                yOffset = (yOffset - (SkinTab.HeaderSize * __instance.YOffset)) - (
                    (hatPackage[key].Count - 1) / __instance.NumPerRow) * __instance.YOffset - SkinTab.HeaderSize;
            }

            __instance.scroller.ContentYBounds.max = -(yOffset + 3.0f + SkinTab.HeaderSize);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.Update))]
        public static void HatsTabUpdatePostfix(HatsTab __instance)
        {
            SkinTab.HideTmpTextPackage(
                hatsTabCustomText, inventoryTop, inventoryBottom);
        }

        private static void createHatTab(
            List<HatData> hats, string packageName, float yStart, HatsTab __instance)
        {
            float offset = yStart;

            SkinTab.AddTmpTextPackageName(
                __instance, yStart, packageName,
                ref hatsTabCustomText, ref offset);

            int numHats = hats.Count;

            for (int i = 0; i < numHats; i++)
            {
                HatData hat = hats[i];

                ColorChip colorChip = SkinTab.SetColorChip(__instance, i, offset);

                int color = __instance.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : SaveManager.BodyColor;

                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    colorChip.Button.OnMouseOver.AddListener(
                        (UnityEngine.Events.UnityAction)(() => __instance.SelectHat(hat)));
                    colorChip.Button.OnMouseOut.AddListener(
                        (UnityEngine.Events.UnityAction)(
                            () => __instance.SelectHat(DestroyableSingleton<HatManager>.Instance.GetHatById(SaveManager.LastHat))));
                    colorChip.Button.OnClick.AddListener(
                        (UnityEngine.Events.UnityAction)(() => __instance.ClickEquip()));
                }
                else
                {
                    colorChip.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => __instance.SelectHat(hat)));
                }

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
