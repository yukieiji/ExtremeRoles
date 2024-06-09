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
			var sprite = Loader.GetSpriteFromResources(id);
			Log.LogInfo($"Img Loaded:{id}.ButtonIcon");
			NullCheck(sprite);
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
			var sprite = Loader.GetSpriteFromResources(id, name);
			Log.LogInfo($"Img Loaded:{id}.{name}");
			NullCheck(sprite);
		}
		catch (Exception ex)
		{
			Log.LogError(
				$"Img:{id}.{name} not load   {ex.Message}");
		}
	}

	protected void LoadFromUnityObjectExR<T, W>(W id, string name)
		where T : UnityEngine.Object
		where W : Enum
	{
		try
		{
			var sprite = Loader.GetUnityObjectFromResources<T, W>(id, name);
			Log.LogInfo($"Asset Loaded:{id}.{name}");
			NullCheck(sprite);
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
			NullCheck(sprite);
		}
		catch (Exception ex)
		{
			Log.LogError(
				$"Img:{asset} | {img} not load   {ex.Message}");
		}
	}
#endif
	protected void NullCheck<T>(in T? sprite)
	{
		if (sprite == null)
		{
			throw new Exception("Sprite is Null");
		}
	}
}
