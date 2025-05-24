using ExtremeRoles.Resources;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

// 順番を変えるとなぜか動くので一番最初においておく
public class AAAATuckerAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:TuckerAsset Test -----");

		LoadFromExR(ExtremeRoleId.Tucker, ObjectPath.TuckerShadow);
		LoadFromExR(ExtremeRoleId.Tucker, ObjectPath.TuckerCreateChimera);
		LoadFromExR(ExtremeRoleId.Tucker, ObjectPath.TuckerRemoveShadow);
	}
}
