using System;
using System.Reflection;
using ExtremeRoles.Resources;

using ExtremeRoles.Test.Asset;

namespace ExtremeRoles.Test;

internal sealed class AllAssetLoadRunner
	: TestRunnerBase
{
	public override void Run()
	{
		Log.LogInfo($"----- Start: Img Load Test -----");

		foreach (FieldInfo field in typeof(ObjectPath).GetFields(BindingFlags.Public | BindingFlags.Static))
		{
			if (!field.IsLiteral) { continue; }

			object? value = field.GetRawConstantValue();

			if (value is string imgPath &&
				imgPath.Contains(".png") &&
				!imgPath.Contains("{"))
			{
				Log.LogInfo($"Load:{field.Name}  path:{imgPath}");
				tryImgLoad(imgPath);
			}
		}
		Run<CommonAssetLoadRunner>();
		Run<ExSpawnMinigameLoadRunner>();

		Run<XionAssetLoadRunner>();

		Run<GuessorAssetLoadRunner>();
		Run<AcceleratorAssetLoadRunner>();
		Run<KidsAssetLoadRunner>();

		Run<TeleporterAssetLoadRunner>();
		Run<JailerAssetLoadRunner>();

		Run<MeryAssetLoadRunner>();
		Run<HypnotistAssetLoadRunner>();
		Run<ThiefAssetLoadRunner>();
		Run<TeroAssetLoadRunner>();
		Run<ZombieAssetLoadRunner>();

		Run<YokoAssetLoadRunner>();
	}

	private void tryImgLoad(string path)
	{
		try
		{
			var sprite = UnityObjectLoader.LoadSpriteFromResources(path);
			if (sprite == null)
			{
				throw new Exception("Sprite is Null");
			}
		}
		catch (Exception ex)
		{
			Log.LogError($"Img:{path} not load   {ex.Message}");
		}
	}
}
