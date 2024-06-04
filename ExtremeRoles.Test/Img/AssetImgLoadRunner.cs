using System;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;

namespace ExtremeRoles.Test.Img;

internal abstract class AssetImgLoadRunner
	: TestRunnerBase
{
	protected void LoadFromExR(ExtremeRoleId id)
	{
		try
		{
			var sprite = Loader.GetUnityObjectFromResources<Sprite>(id);
			Log.LogInfo($"Img Loaded:{id}.ButtonIcon");
			SpriteCheck(sprite);
		}
		catch (Exception ex)
		{
			Log.LogError(
				$"Img:{id}.ButtonIcon not load   {ex.Message}");
		}
	}

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
	protected void SpriteCheck(in Sprite sprite)
	{
		if (sprite == null)
		{
			throw new Exception("Sprite is Null");
		}
	}
}
