
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeSkins.Module;

namespace ExtremeSkins.SkinManager;

public static class ExtremeColorManager
{
    public static uint ColorNum;
    public static readonly Dictionary<StringNames, string> LangData = new Dictionary<StringNames, string>();

    public static void Initialize()
    {
        LangData.Clear();


		var customColor = CustomColorPalette.CustomColor;
		var creatorModeColor = CustomColorPalette.AddColor;

		if (customColor.Count == 0 &&
			creatorModeColor.Count == 0) { return; }

		customColor.AddRange(creatorModeColor);

		ColorNum = (uint)(Palette.ColorNames.Length + customColor.Count);

		if (ColorNum > byte.MaxValue)
		{
			ExtremeSkinsPlugin.Logger.LogError(
				"Number of color is Overflow!!, Disable CustomColor Functions");
			return;
		}
		loadCustomColor(customColor);
	}

    private static void loadCustomColor(IReadOnlyCollection<CustomColorPalette.ColorData> colorData)
    {
		List<StringNames> longlist = Palette.ColorNames.ToList();
		List<Color32> colorlist = Palette.PlayerColors.ToList();
		List<Color32> shadowlist = Palette.ShadowColors.ToList();
		List<Color32> textColorlist = Palette.TextColors.ToList();
		List<Color32> textOutLineColorlist = Palette.TextOutlineColors.ToList();

		int id = 50000;

        foreach (var cc in colorData)
        {
			StringNames name = (StringNames)id;

			longlist.Add(name);
            colorlist.Add(cc.MainColor);
            shadowlist.Add(cc.ShadowColor);
			textColorlist.Add(cc.MainColor);
			textOutLineColorlist.Add(new Color32(
				(byte)(byte.MaxValue - cc.ShadowColor.r),
				(byte)(byte.MaxValue - cc.ShadowColor.g),
				(byte)(byte.MaxValue - cc.ShadowColor.b),
				byte.MaxValue));



			LangData.Add(name, cc.Name);

			++id;
        }

        Palette.ColorNames = longlist.ToArray();
        Palette.PlayerColors = colorlist.ToArray();
        Palette.ShadowColors = shadowlist.ToArray();
		Palette.TextColors = textColorlist.ToArray();
		Palette.TextOutlineColors = textOutLineColorlist.ToArray();
    }
}
