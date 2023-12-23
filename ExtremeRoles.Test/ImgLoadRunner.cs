using System;
using System.Reflection;

using ExtremeRoles.Resources;

namespace ExtremeRoles.Test;

internal sealed class ImgLoadRunner
	: TestRunnerBase
{
	public override void Run()
	{
		this.Log.LogInfo($"----- Start: Img Load Test -----");

		foreach (FieldInfo field in typeof(Path).GetFields(BindingFlags.Public | BindingFlags.Static))
		{
			if (!field.IsLiteral) { continue; }

			object? value = field.GetRawConstantValue();

			if (value is string imgPath &&
				imgPath.Contains(".png") &&
				!imgPath.Contains("{"))
			{
				this.Log.LogInfo($"Load:{field.Name}  path:{imgPath}");
				tryImgLoad(imgPath);
			}
		}
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
			this.Log.LogError($"Img:{path} not load   {ex.Message}");
		}
	}
}
