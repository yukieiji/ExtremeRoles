using System.Collections.Generic;

using TMPro;

using UnityEngine;

using ExtremeSkins.Patches.AmongUs.Tab;

namespace ExtremeSkins.Helper
{
    public static class CustomCosmicTab
    {
        public static TMP_Text textTemplate;

        public const string InnerslothPackageName = "innerslothMake";

        public const string CreatorTabAssetBundle = "ExtremeSkins.Resources.Asset.creatortab.asset";
        public const string CreatorTabAssetPrefab = "assets/extremeskins/creatortab.prefab";

        public const float HeaderSize = 0.8f;
        private const float headerX = 0.8f;
        private const float inventoryZ = -2f;

        public static void AddTmpTextPackageName(
            InventoryTab instance,
            float yPos,
            string packageName,
            ref List<TMP_Text> textList,
            ref float offset)
        {
            if (textTemplate != null)
            {
                TMP_Text title = Object.Instantiate(
                    textTemplate, instance.scroller.Inner);
                title.transform.SetParent(instance.scroller.Inner);
                title.transform.localPosition = new Vector3(headerX, yPos, inventoryZ);
                title.alignment = TextAlignmentOptions.Center;
                title.fontSize *= 1.25f;
                title.fontWeight = FontWeight.Thin;
                title.enableAutoSizing = false;
                title.autoSizeTextContainer = true;
                title.text = Translation.GetString(packageName);
                title.gameObject.SetActive(true);
                offset -= HeaderSize * instance.YOffset;
                textList.Add(title);
            }
        }

        public static void RemoveAllTabs()
        {
            if (HatsTabPatch.Tab != null)
            {
                Object.Destroy(HatsTabPatch.Tab.gameObject);
                HatsTabPatch.Tab = null;
            }
            if (NameplatesTabPatch.Tab != null)
            {
                Object.Destroy(NameplatesTabPatch.Tab.gameObject);
                NameplatesTabPatch.Tab = null;
            }
            if (VisorsTabPatch.Tab != null)
            {
                Object.Destroy(VisorsTabPatch.Tab.gameObject);
                VisorsTabPatch.Tab = null;
            }
        }

        public static void DestoryList<T>(List<T> items) where T : Object
        {
            if (items == null) { return; }
            foreach (T item in items)
            {
                Object.Destroy(item);
            }
        }

        public static void HideTmpTextPackage(
            List<TMP_Text> packageText,
            float inventoryTop,
            float inventoryBottom)
        {
            // Manually hide all custom TMPro.TMP_Text objects that are outside the ScrollRect
            foreach (TMP_Text customText in packageText)
            {
                if (customText != null && 
                    customText.transform != null && 
                    customText.gameObject != null)
                {
                    bool active = 
                        customText.transform.position.y <= inventoryTop && 
                        customText.transform.position.y >= inventoryBottom;
                    float epsilon = Mathf.Min(
                        Mathf.Abs(customText.transform.position.y - inventoryTop),
                        Mathf.Abs(customText.transform.position.y - inventoryBottom));
                    if (active != customText.gameObject.active && epsilon > 0.1f)
                    {
                        customText.gameObject.SetActive(active);
                    }
                }
            }
        }

        public static ColorChip SetColorChip(
            InventoryTab instance, int setIndex, float offset)
        {
            float xPos = instance.XRange.Lerp(
                    (setIndex % instance.NumPerRow) / (instance.NumPerRow - 1f));
            float yPos = offset - (setIndex / instance.NumPerRow) * instance.YOffset;
            ColorChip colorChip = UnityEngine.Object.Instantiate<ColorChip>(
                instance.ColorTabPrefab, instance.scroller.Inner);

            colorChip.transform.localPosition = new Vector3(xPos, yPos, inventoryZ);

            return colorChip;
        }

    }
}
