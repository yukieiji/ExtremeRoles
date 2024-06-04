using System;

using ExtremeRoles.Resources;

using UnityEngine;

namespace ExtremeRoles.Test.Img;

internal sealed class CommonImgLoadRunner
	: AssetImgLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:SingleImgLoad Test -----");

		imgTest(Path.Bomb);
		imgTest(Path.Meeting);
	}

	private void imgTest(string img)
	{
		try
		{
			var sprite = Loader.GetUnityObjectFromExRResources<Sprite>(img);
			Log.LogInfo($"Img Loaded: {img}");
			SpriteCheck(sprite);
		}
		catch (Exception ex)
		{
			Log.LogError(
				$"Img: {img} not load   {ex.Message}");
		}
	}
}
