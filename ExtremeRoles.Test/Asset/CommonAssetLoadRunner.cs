using System;

using ExtremeRoles.Resources;

using UnityEngine;

namespace ExtremeRoles.Test.Asset;

internal sealed class CommonAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:SingleImgLoad Test -----");
#if DEBUG
		imgTest(ObjectPath.Bomb);
		imgTest(ObjectPath.Meeting);
	}

	private void imgTest(string img)
	{
		try
		{
			var sprite = UnityObjectLoader.GetUnityObjectFromExRResources<Sprite>(img);
			Log.LogInfo($"Img Loaded: {img}");
			NullCheck(sprite);
		}
		catch (Exception ex)
		{
			Log.LogError(
				$"Img: {img} not load   {ex.Message}");
		}
#endif
	}
}
