using ExtremeRoles.Roles.Solo.Host;

namespace ExtremeRoles.Test.Img;

internal sealed class XionImgLoadRunner
	: AssetImgLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:XionImgLoad Test -----");

		xionImgLoad("ZoomIn");
		xionImgLoad("ZoomOut");
		xionImgLoad("SpeedUp");
		xionImgLoad("SpeedDown");
	}
	private void xionImgLoad(string imgName)
	{
		Log.LogInfo($"{imgName} loading....");
		var sprite = Xion.GetSprite(imgName);
		SpriteCheck(sprite);
	}
}
