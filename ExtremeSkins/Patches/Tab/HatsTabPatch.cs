using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;

using ExtremeSkins.Module;

namespace ExtremeSkins.Patches.Tab
{
    [HarmonyPatch]
    public class HatsTabPatch
    {
        public static TMPro.TMP_Text textTemplate;
        private static List<TMPro.TMP_Text> hatsTabCustomText = new List<TMPro.TMP_Text>();

        private static float inventoryTop = 1.5f;
        private static float inventoryBot = -2.5f;


        private const string innerslothPackageName = "innerslothHats";
        private const float headerSize = 0.8f;
        private const float headerX = 0.8f;
        private const float inventoryZ = -2f;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.OnEnable))]
        public static bool HatsTabOnEnablePrefix(HatsTab __instance)
        {
            inventoryTop = __instance.scroller.Inner.position.y - 0.5f;
            inventoryBot = __instance.scroller.Inner.position.y - 4.5f;

            HatBehaviour[] unlockedHats = DestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
            Dictionary<string, List<HatBehaviour>> hatPackage = new Dictionary<string, List<HatBehaviour>>();

            destroyList(hatsTabCustomText);
            destroyList(__instance.ColorChips.ToArray().ToList());

            hatsTabCustomText.Clear();
            __instance.ColorChips.Clear();

            textTemplate = PlayerCustomizationMenu.Instance.itemName;

            foreach (HatBehaviour hatBehaviour in unlockedHats)
            {
                CustomHat hat;
                bool result = ExtremeHatManager.HatData.TryGetValue(
                    hatBehaviour.ProductId, out hat);
                if (result)
                {
                    if (!hatPackage.ContainsKey(hat.Author))
                    {
                        hatPackage.Add(hat.Author, new List<HatBehaviour>());
                    }
                    hatPackage[hat.Author].Add(hatBehaviour);
                }
                else
                {
                    if (!hatPackage.ContainsKey(innerslothPackageName))
                    {
                        hatPackage.Add(innerslothPackageName, new List<HatBehaviour>());
                    }
                    hatPackage[innerslothPackageName].Add(hatBehaviour);
                }
            }

            float yOffset = __instance.YStart;

            var orderedKeys = hatPackage.Keys.OrderBy((string x) => {
                if (x == innerslothPackageName)
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
                yOffset = (yOffset - (headerSize * __instance.YOffset)) - (
                    (hatPackage[key].Count - 1) / __instance.NumPerRow) * __instance.YOffset - headerSize;
            }

            __instance.scroller.ContentYBounds.max = -(yOffset + 3.0f + headerSize);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HatsTab), nameof(HatsTab.Update))]
        public static void HatsTabUpdatePostfix(HatsTab __instance)
        {
            // Manually hide all custom TMPro.TMP_Text objects that are outside the ScrollRect
            foreach (TMPro.TMP_Text customText in hatsTabCustomText)
            {
                if (customText != null && customText.transform != null && customText.gameObject != null)
                {
                    bool active = customText.transform.position.y <= inventoryTop && customText.transform.position.y >= inventoryBot;
                    float epsilon = Mathf.Min(
                        Mathf.Abs(customText.transform.position.y - inventoryTop),
                        Mathf.Abs(customText.transform.position.y - inventoryBot));
                    if (active != customText.gameObject.active && epsilon > 0.1f)
                    {
                        customText.gameObject.SetActive(active);
                    }
                }
            }
        }

        private static void createHatTab(
            List<HatBehaviour> hats, string packageName, float yStart, HatsTab __instance)
        {
            float offset = yStart;

            if (textTemplate != null)
            {
                TMPro.TMP_Text title = UnityEngine.Object.Instantiate<TMPro.TMP_Text>(
                    textTemplate, __instance.scroller.Inner);
                title.transform.parent = __instance.scroller.Inner;
                title.transform.localPosition = new Vector3(headerX, yStart, inventoryZ);
                title.alignment = TMPro.TextAlignmentOptions.Center;
                title.fontSize *= 1.25f;
                title.fontWeight = TMPro.FontWeight.Thin;
                title.enableAutoSizing = false;
                title.autoSizeTextContainer = true;
                title.text = Helper.Translation.GetString(packageName);
                offset -= headerSize * __instance.YOffset;
                hatsTabCustomText.Add(title);
            }

            int numHats = hats.Count;

            for (int i = 0; i < numHats; i++)
            {
                HatBehaviour hat = hats[i];

                float xpos = __instance.XRange.Lerp(
                    (i % __instance.NumPerRow) / (__instance.NumPerRow - 1f));
                float ypos = offset - (i / __instance.NumPerRow) * __instance.YOffset;
                ColorChip colorChip = UnityEngine.Object.Instantiate<ColorChip>(
                    __instance.ColorTabPrefab, __instance.scroller.Inner);

                int color = __instance.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : SaveManager.BodyColor;

                colorChip.transform.localPosition = new Vector3(xpos, ypos, inventoryZ);
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

        private static void destroyList<T>(List<T> items) where T : UnityEngine.Object
        {
            if (items == null) { return; }
            foreach (T item in items)
            {
                Object.Destroy(item);
            }
        }

    }

}
