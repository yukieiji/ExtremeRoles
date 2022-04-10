using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using ExtremeSkins.Module;
using ExtremeSkins.Helper;
using ExtremeSkins.SkinManager;


namespace ExtremeSkins.Patches.AmongUs.Tab
{
#if WITHNAMEPLATE
    [HarmonyPatch]
    public class NameplatesTabPatch
    {
        private static List<TMPro.TMP_Text> namePlateTabCustomText = new List<TMPro.TMP_Text>();

        private static float inventoryTop = 1.5f;
        private static float inventoryBottom = -2.5f;


        [HarmonyPrefix]
        [HarmonyPatch(typeof(NameplatesTab), nameof(NameplatesTab.OnEnable))]
        public static bool NameplatesTabOnEnablePrefix(NameplatesTab __instance)
        {
            inventoryTop = __instance.scroller.Inner.position.y - 0.5f;
            inventoryBottom = __instance.scroller.Inner.position.y - 4.5f;

            NamePlateData[] unlockedNamePlate = DestroyableSingleton<HatManager>.Instance.GetUnlockedNamePlates();
            Dictionary<string, List<NamePlateData>> namePlatePackage = new Dictionary<string, List<NamePlateData>>();

            SkinTab.DestoryList(namePlateTabCustomText);
            SkinTab.DestoryList(__instance.ColorChips.ToArray().ToList());

            namePlateTabCustomText.Clear();
            __instance.ColorChips.Clear();

            if (SkinTab.textTemplate == null)
            {
                SkinTab.textTemplate = PlayerCustomizationMenu.Instance.itemName;
            }

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
                    if (!namePlatePackage.ContainsKey(SkinTab.InnerslothPackageName))
                    {
                        namePlatePackage.Add(
                            SkinTab.InnerslothPackageName,
                            new List<NamePlateData>());
                    }
                    namePlatePackage[SkinTab.InnerslothPackageName].Add(hatBehaviour);
                }
            }

            float yOffset = __instance.YStart;

            var orderedKeys = namePlatePackage.Keys.OrderBy((string x) => {
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
                createNamePlateTab(namePlatePackage[key], key, yOffset, __instance);
                yOffset = (yOffset - (SkinTab.HeaderSize * __instance.YOffset)) - (
                    (namePlatePackage[key].Count - 1) / __instance.NumPerRow) * __instance.YOffset - SkinTab.HeaderSize;
            }

            __instance.scroller.ContentYBounds.max = -(yOffset + 3.0f + SkinTab.HeaderSize);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(NameplatesTab), nameof(NameplatesTab.Update))]
        public static void NameplatesTabUpdatePostfix(NameplatesTab __instance)
        {
            SkinTab.HideTmpTextPackage(
                namePlateTabCustomText, inventoryTop, inventoryBottom);
        }

        private static void createNamePlateTab(
            List<NamePlateData> namePlates, string packageName, float yStart, NameplatesTab __instance)
        {
            float offset = yStart;


            SkinTab.AddTmpTextPackageName(
                __instance, yStart, packageName,
                ref namePlateTabCustomText, ref offset);

            int numHats = namePlates.Count;

            for (int i = 0; i < numHats; i++)
            {
                NamePlateData np = namePlates[i];

                ColorChip colorChip = SkinTab.SetColorChip(
                    __instance, i, offset);

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

                __instance.StartCoroutine(
                    np.CoLoadViewData((Il2CppSystem.Action<NamePlateViewData>)((n) => {
                        colorChip.gameObject.GetComponent<NameplateChip>().image.sprite = n.Image;
                    __instance.ColorChips.Add(colorChip);
                })));


                __instance.ColorChips.Add(colorChip);
            }
        }
    }
#endif
}
