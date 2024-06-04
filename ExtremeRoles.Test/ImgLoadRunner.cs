using System;
using System.Reflection;
using ExtremeRoles.Resources;

using ExtremeRoles.Test.Img;

namespace ExtremeRoles.Test;

internal sealed class ImgLoadRunner
	: TestRunnerBase
{
	public override void Run()
	{
		Log.LogInfo($"----- Start: Img Load Test -----");

		foreach (FieldInfo field in typeof(Path).GetFields(BindingFlags.Public | BindingFlags.Static))
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
		runSuite<ExSpawnMinigameImgLoadRunner>();
		runSuite<MeryImgLoadRunner>();
	}

	private void runSuite<T>() where T : TestRunnerBase, new()
	{
		T runner = new T();
		runner.Run();
	}

	private void tryImgLoad(string path)
	{
		try
		{
			var sprite = Loader.CreateSpriteFromResources(path);
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
