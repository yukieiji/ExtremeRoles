using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using ExtremeSkins.Module;
using ExtremeSkins.Helper;
using ExtremeSkins.SkinManager;


namespace ExtremeSkins.Patches.AmongUs.Tab
{
#if WITHVISOR
    [HarmonyPatch]
    public class VisorsTabPatch
    {
        private static List<TMPro.TMP_Text> visorsTabCustomText = new List<TMPro.TMP_Text>();

        private static float inventoryTop = 1.5f;
        private static float inventoryBottom = -2.5f;


        [HarmonyPrefix]
        [HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.OnEnable))]
        public static bool VisorsTabOnEnablePrefix(VisorsTab __instance)
        {
            inventoryTop = __instance.scroller.Inner.position.y - 0.5f;
            inventoryBottom = __instance.scroller.Inner.position.y - 4.5f;

            VisorData[] unlockedVisor = DestroyableSingleton<HatManager>.Instance.GetUnlockedVisors();
            Dictionary<string, List<VisorData>> visorPackage = new Dictionary<string, List<VisorData>>();

            SkinTab.DestoryList(visorsTabCustomText);
            SkinTab.DestoryList(__instance.ColorChips.ToArray().ToList());

            visorsTabCustomText.Clear();
            __instance.ColorChips.Clear();

            if (SkinTab.textTemplate == null)
            {
                SkinTab.textTemplate = PlayerCustomizationMenu.Instance.itemName;
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
                    if (!visorPackage.ContainsKey(SkinTab.InnerslothPackageName))
                    {
                        visorPackage.Add(
                            SkinTab.InnerslothPackageName,
                            new List<VisorData>());
                    }
                    visorPackage[SkinTab.InnerslothPackageName].Add(viData);
                }
            }

            float yOffset = __instance.YStart;

            var orderedKeys = visorPackage.Keys.OrderBy((string x) => {
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
                createVisorTab(visorPackage[key], key, yOffset, __instance);
                yOffset = (yOffset - (SkinTab.HeaderSize * __instance.YOffset)) - (
                    (visorPackage[key].Count - 1) / __instance.NumPerRow) * __instance.YOffset - SkinTab.HeaderSize;
            }

            __instance.scroller.ContentYBounds.max = -(yOffset + 3.0f + SkinTab.HeaderSize);          
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(VisorsTab), nameof(VisorsTab.Update))]
        public static void VisorsTabUpdatePostfix(VisorsTab __instance)
        {
            SkinTab.HideTmpTextPackage(
                visorsTabCustomText, inventoryTop, inventoryBottom);
        }

        private static void createVisorTab(
            List<VisorData> visores, string packageName, float yStart, VisorsTab __instance)
        {
            float offset = yStart;


            SkinTab.AddTmpTextPackageName(
                __instance, yStart, packageName,
                ref visorsTabCustomText, ref offset);

            int numVisor = visores.Count;

            for (int i = 0; i < numVisor; i++)
            {
                VisorData vi = visores[i];

                ColorChip colorChip = SkinTab.SetColorChip(
                    __instance, i, offset);

                if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
                {
                    colorChip.Button.OnMouseOver.AddListener(
                        (UnityEngine.Events.UnityAction)(() => __instance.SelectVisor(vi)));
                    colorChip.Button.OnMouseOut.AddListener(
                        (UnityEngine.Events.UnityAction)(
                            () => __instance.SelectVisor(
                                DestroyableSingleton<HatManager>.Instance.GetVisorById(SaveManager.LastVisor))));
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

                __instance.StartCoroutine(
                    vi.CoLoadViewData((Il2CppSystem.Action<VisorViewData>)((v) => {
                        colorChip.Inner.FrontLayer.sprite = v.IdleFrame;
                })));

                __instance.ColorChips.Add(colorChip);
            }
        }
    }
#endif
}
