using System.Collections.Generic;

using TMPro;

using UnityEngine;

using ExtremeSkins.Patches.AmongUs.Tab;
using Innersloth.Assets;

namespace ExtremeSkins.Helper;

public static class CustomCosmicTab
{
    public static TMP_Text? TextTemplate;

    public const string InnerslothPackageName = "innerslothMake";

    public const string CreatorTabAssetBundle = "ExtremeSkins.Resources.Asset.creatortab.asset";
    public const string CreatorTabAssetPrefab = "assets/extremeskins/creatortab.prefab";

    public const float HeaderSize = 0.8f;
    private const float headerX = 0.8f;
    private const float inventoryZ = -2f;

	public static void EnableTab(InventoryTab tab)
	{
		var preview = tab.PlayerPreview;
		preview.gameObject.SetActive(true);
		if (tab.HasLocalPlayer())
		{
			preview.UpdateFromLocalPlayer(PlayerMaterial.MaskType.None);
		}
		else
		{
			preview.UpdateFromDataManager(PlayerMaterial.MaskType.None);
		}
	}

	public static void AddTmpTextPackageName(
        InventoryTab instance,
        float yPos,
        string packageName,
        ref List<TMP_Text> textList,
        ref float offset)
    {
        if (TextTemplate != null)
        {
            TMP_Text title = Object.Instantiate(
				TextTemplate, instance.scroller.Inner);
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

	public static void SetPreviewToRawSprite(ColorChip colorChip, Sprite? sprite, int color)
	{
		var renderer = colorChip.Inner.FrontLayer;
		renderer.sprite = sprite;
		AddressableAssetHandler.AddToGameObject(renderer.gameObject);
		if (Application.isPlaying)
		{
			PlayerMaterial.SetColors(color, renderer);
		}
	}

	public static void RemoveAllTabs()
    {
#if WITHHAT
		if (HatsTabPatch.Tab != null)
        {
            Object.DestroyImmediate(HatsTabPatch.Tab.gameObject);
            HatsTabPatch.Tab = null;
        }
#endif
#if WITHNAMEPLATE
		if (NameplatesTabPatch.Tab != null)
        {
            Object.DestroyImmediate(NameplatesTabPatch.Tab.gameObject);
            NameplatesTabPatch.Tab = null;
        }
#endif
#if WITHVISOR
		if (VisorsTabPatch.Tab != null)
        {
            Object.DestroyImmediate(VisorsTabPatch.Tab.gameObject);
            VisorsTabPatch.Tab = null;
        }
#endif
	}

	public static void DestoryList<T>(List<T> items) where T : Object
    {
        if (items == null) { return; }
        foreach (T item in items)
        {
            Object.DestroyImmediate(item);
        }
    }

    public static void HideTmpTextPackage(
        in IReadOnlyList<TMP_Text> packageText,
        float inventoryTop,
        float inventoryBottom)
    {
        // Manually hide all custom TMPro.TMP_Text objects that are outside the ScrollRect
        foreach (TMP_Text customText in packageText)
        {
			if (customText == null ||
				customText.transform == null ||
				customText.gameObject == null)
			{
				continue;
			}

			float y = customText.transform.position.y;
			bool active = y <= inventoryTop && y >= inventoryBottom;
			float epsilon = Mathf.Min(
				Mathf.Abs(y - inventoryTop),
				Mathf.Abs(y - inventoryBottom));
			if (active != customText.gameObject.active && epsilon > 0.1f)
			{
				customText.gameObject.SetActive(active);
			}
		}
    }

    public static ColorChip SetColorChip(
        in InventoryTab instance, int setIndex, float offset)
    {
        float xPos = instance.XRange.Lerp(
            (setIndex % instance.NumPerRow) / (instance.NumPerRow - 1f));
        float yPos = offset - (setIndex / instance.NumPerRow) * instance.YOffset;
        ColorChip colorChip = Object.Instantiate(
            instance.ColorTabPrefab, instance.scroller.Inner);

        colorChip.transform.localPosition = new Vector3(xPos, yPos, inventoryZ);

        return colorChip;
    }

}
