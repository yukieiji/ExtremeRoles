using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Asset;

internal sealed class GlitchAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:GlitchAsset Test -----");

		LoadFromExR(ExtremeRoleId.Glitch);
	}
}
