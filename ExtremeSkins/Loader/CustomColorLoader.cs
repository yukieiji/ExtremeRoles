using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Module;

using ExtremeSkins.Helper;
using ExtremeSkins.Module;

using ColorData = ExtremeSkins.Module.CustomColorPalette.ColorData;

namespace ExtremeSkins.Loader;

public sealed class CustomColorLoader : NullableSingleton<CustomColorLoader>
{
	public static int AllColorNum { get; private set; }

	private readonly List<ColorData> addData = new List<ColorData>();

	public void Load()
	{
		var customColor = CustomColorPalette.CustomColor;

		if (customColor.Count == 0 &&
			this.addData.Count == 0) { return; }

		customColor.AddRange(this.addData);

		int checkSize = (Palette.ColorNames.Length + customColor.Count);

		if (checkSize > byte.MaxValue)
		{
			ExtremeSkinsPlugin.Logger.LogError(
				"Number of color is Overflow!!, Disable CustomColor Functions");
			return;
		}

		loadCustomColor(customColor);
		AllColorNum = checkSize;
	}

	public void AddCustomColor(ColorData data)
	{
		this.addData.Add(data);
	}

	private static void loadCustomColor(IReadOnlyCollection<ColorData> colorData)
	{
		List<StringNames> longlist = Palette.ColorNames.ToList();
		List<Color32> colorlist = Palette.PlayerColors.ToList();
		List<Color32> shadowlist = Palette.ShadowColors.ToList();

		int id = 50000;

		foreach (var cc in colorData)
		{
			StringNames name = (StringNames)id;

			longlist.Add(name);
			colorlist.Add(cc.MainColor);
			shadowlist.Add(cc.ShadowColor);

			Translation.AddColorText(name, cc.Name);

			++id;
		}

		Palette.ColorNames = longlist.ToArray();
		Palette.PlayerColors = colorlist.ToArray();
		Palette.ShadowColors = shadowlist.ToArray();
	}

	public static void StaticLoad()
	{
		Instance.Load();
		TryDestroy();
	}
}
