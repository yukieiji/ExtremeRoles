using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public class GlitchAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:GlitchAsset Test -----");

		LoadFromExR(ExtremeRoleId.Glitch);
	}
}
