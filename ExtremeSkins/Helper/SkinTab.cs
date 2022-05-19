using System.Collections.Generic;

using TMPro;

using UnityEngine;

namespace ExtremeSkins.Helper
{
    public static class SkinTab
    {
        public static TMP_Text textTemplate;

        public const string InnerslothPackageName = "innerslothMake";

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
                TMP_Text title = UnityEngine.Object.Instantiate<TMP_Text>(
                    textTemplate, instance.scroller.Inner);
                title.transform.parent = instance.scroller.Inner;
                title.transform.localPosition = new Vector3(headerX, yPos, inventoryZ);
                title.alignment = TMPro.TextAlignmentOptions.Center;
                title.fontSize *= 1.25f;
                title.fontWeight = TMPro.FontWeight.Thin;
                title.enableAutoSizing = false;
                title.autoSizeTextContainer = true;
                title.text = Helper.Translation.GetString(packageName);
                offset -= HeaderSize * instance.YOffset;
                textList.Add(title);
            }
        }

        public static void DestoryList<T>(List<T> items) where T : UnityEngine.Object
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
                if (customText != null && customText.transform != null && customText.gameObject != null)
                {
                    bool active = customText.transform.position.y <= inventoryTop && customText.transform.position.y >= inventoryBottom;
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
