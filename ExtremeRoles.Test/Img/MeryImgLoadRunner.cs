using ExtremeRoles.Resources;

namespace ExtremeRoles.Test.Img;

internal sealed class MeryImgLoadRunner
	: AssetImgLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:MeryImgLoad Test -----");

		for (int index = 0; index < 18; ++index)
		{
			LoadFromExR(
				Path.MeryAsset,
				string.Format(
					Path.RoleImgPathFormat,
					$"{Path.MeryVent}{index}"));
		}

		LoadFromExR(
			Path.MeryAsset,
			string.Format(
				Path.RoleImgPathFormat,
				Path.MeryNoneActive));
		LoadFromExR(
			Path.MeryAsset,
			string.Format(
				Path.RoleImgPathFormat,
				Path.MeryButtonIcon));
	}
}
