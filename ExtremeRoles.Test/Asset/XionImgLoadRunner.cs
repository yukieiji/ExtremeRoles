using ExtremeRoles.Roles.Solo.Host;

namespace ExtremeRoles.Test.Asset;

internal sealed class XionAssetLoadRunner
	: AssetLoadRunner
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
