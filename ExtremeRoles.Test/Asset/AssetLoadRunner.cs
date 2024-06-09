using System;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;

namespace ExtremeRoles.Test.Asset;

internal abstract class AssetLoadRunner
	: TestRunnerBase
{
	protected void LoadFromExR<W>(W id) where W : Enum
	{
		try
		{
			var sprite = Loader.GetUnityObjectFromResources<Sprite, W>(id);
			Log.LogInfo($"Img Loaded:{id}.ButtonIcon");
			SpriteCheck(sprite);
		}
		catch (Exception ex)
		{
			Log.LogError(
				$"Img:{id}.ButtonIcon not load   {ex.Message}");
		}
	}
	protected void LoadFromExR<W>(W id, string name) where W : Enum
	{
		try
		{
			var sprite = Loader.GetUnityObjectFromResources<Sprite, W>(id, name);
			Log.LogInfo($"Img Loaded:{id}.{name}");
			SpriteCheck(sprite);
		}
		catch (Exception ex)
		{
			Log.LogError(
				$"Img:{id}.{name} not load   {ex.Message}");
		}
	}

#if DEBUG
	protected void LoadFromExR(string asset, string img)
	{
		try
		{
			var sprite = Loader.GetUnityObjectFromExRResources<Sprite>(asset, img);
			Log.LogInfo($"Img Loaded:{asset} | {img}");
			SpriteCheck(sprite);
		}
		catch (Exception ex)
		{
			Log.LogError(
				$"Img:{asset} | {img} not load   {ex.Message}");
		}
	}
#endif
	protected void SpriteCheck(in Sprite sprite)
	{
		if (sprite == null)
		{
			throw new Exception("Sprite is Null");
		}
	}
}
