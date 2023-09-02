using ExtremeRoles.Performance;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.ApiHandler;

public readonly record struct ColorInfo(int RGBA, byte R, byte G, byte B, byte A)
{
	public static implicit operator ColorInfo(Color32 color) => new ColorInfo(color.rgba, color.r, color.g, color.b, color.a);
}

public readonly record struct PlayerNameInfo(string PlayerName, ColorInfo ShowColor);

public readonly record struct PlayerColorInfo(int ColorId, StringNames ColorNameId, ColorInfo MainColor, ColorInfo ShadowColor)
{
	public static PlayerColorInfo Create(GameData.PlayerOutfit outfit)
	{
		int colorId = outfit.ColorId;
		StringNames colorName = Palette.ColorNames[colorId];
		var mainColor = Palette.PlayerColors[colorId];
		var shadowColor = Palette.ShadowColors[colorId];

		return new PlayerColorInfo(colorId, colorName, mainColor, shadowColor);
	}
}

public readonly record struct PlayerCosmicInfo(
	PlayerColorInfo Color,
	string HatId, string VisorId,
	string SkinId, string PetId, string NamePlateId)
{
	public static PlayerCosmicInfo Create(GameData.PlayerOutfit outfit)
		=> new PlayerCosmicInfo(
			PlayerColorInfo.Create(outfit),
			outfit.HatId,
			outfit.VisorId,
			outfit.SkinId,
			outfit.PetId,
			outfit.NamePlateId);
}