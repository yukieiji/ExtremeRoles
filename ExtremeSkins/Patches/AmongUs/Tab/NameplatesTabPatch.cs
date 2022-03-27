using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;

using ExtremeSkins.Module;


namespace ExtremeSkins.Patches.AmongUs.Tab
{
    [HarmonyPatch]
    public class NameplatesTabPatch
    {
        private static TMPro.TMP_Text textTemplate;
        private static List<TMPro.TMP_Text> hatsTabCustomText = new List<TMPro.TMP_Text>();

        private static float inventoryTop = 1.5f;
        private static float inventoryBot = -2.5f;


        private const string innerslothPackageName = "innerslothNamePlate";
        private const float headerSize = 0.8f;
        private const float headerX = 0.8f;
        private const float inventoryZ = -2f;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NameplatesTab), nameof(NameplatesTab.OnEnable))]
        public static bool NameplatesTabOnEnablePrefix(NameplatesTab __instance)
        {
            inventoryTop = __instance.scroller.Inner.position.y - 0.5f;
            inventoryBot = __instance.scroller.Inner.position.y - 4.5f;

            NamePlateData[] unlockedNamePlate = DestroyableSingleton<HatManager>.Instance.GetUnlockedNamePlates();
            Dictionary<string, List<NamePlateData>> namePlatePackage = new Dictionary<string, List<NamePlateData>>();

            destroyList(hatsTabCustomText);
            destroyList(__instance.ColorChips.ToArray().ToList());

            hatsTabCustomText.Clear();
            __instance.ColorChips.Clear();

            textTemplate = PlayerCustomizationMenu.Instance.itemName;

            foreach (NamePlateData hatBehaviour in unlockedNamePlate)
            {
                CustomNamePlate np;
                bool result = ExtremeNamePlateManager.NamePlateData.TryGetValue(
                    hatBehaviour.ProductId, out np);
                if (result)
                {
                    if (!namePlatePackage.ContainsKey(np.Author))
                    {
                        namePlatePackage.Add(np.Author, new List<NamePlateData>());
                    }
                    namePlatePackage[np.Author].Add(hatBehaviour);
                }
                else
                {
                    if (!namePlatePackage.ContainsKey(innerslothPackageName))
                    {
                        namePlatePackage.Add(innerslothPackageName, new List<NamePlateData>());
                    }
                    namePlatePackage[innerslothPackageName].Add(hatBehaviour);
                }
            }

            float yOffset = __instance.YStart;

            var orderedKeys = namePlatePackage.Keys.OrderBy((string x) => {
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
                createNamePlateTab(namePlatePackage[key], key, yOffset, __instance);
                yOffset = (yOffset - (headerSize * __instance.YOffset)) - (
                    (namePlatePackage[key].Count - 1) / __instance.NumPerRow) * __instance.YOffset - headerSize;
            }

            __instance.scroller.ContentYBounds.max = -(yOffset + 3.0f + headerSize);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(NameplatesTab), nameof(NameplatesTab.Update))]
        public static void NameplatesTabUpdatePostfix(NameplatesTab __instance)
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

        private static void createNamePlateTab(
            List<NamePlateData> namePlates, string packageName, float yStart, NameplatesTab __instance)
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

            int numHats = namePlates.Count;

            for (int i = 0; i < numHats; i++)
            {
                NamePlateData np = namePlates[i];

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
                        (UnityEngine.Events.UnityAction)(() => __instance.SelectNameplate(np)));
                    colorChip.Button.OnMouseOut.AddListener(
                        (UnityEngine.Events.UnityAction)(
                            () => __instance.SelectNameplate(
                                DestroyableSingleton<HatManager>.Instance.GetNamePlateById(SaveManager.LastHat))));
                    colorChip.Button.OnClick.AddListener(
                        (UnityEngine.Events.UnityAction)(() => __instance.ClickEquip()));
                }
                else
                {
                    colorChip.Button.OnClick.AddListener(
                        (UnityEngine.Events.UnityAction)(() => __instance.SelectNameplate(np)));
                }

                colorChip.gameObject.GetComponent<NameplateChip>().image.sprite = np.Image;
                colorChip.Inner.transform.localPosition = np.ChipOffset;
                colorChip.Tag = np;
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
