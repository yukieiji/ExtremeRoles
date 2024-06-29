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
		imgTest(Path.Bomb);
		imgTest(Path.Meeting);
	}

	private void imgTest(string img)
	{
		try
		{
			var sprite = Loader.GetUnityObjectFromExRResources<Sprite>(img);
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
