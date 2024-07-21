using ExtremeRoles.Resources;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Asset;

internal sealed class SummonerAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:SummonerAsset Test -----");

		LoadFromExR(ExtremeRoleId.Summoner, ObjectPath.SummonerMarking);
		LoadFromExR(ExtremeRoleId.Summoner, ObjectPath.SummonerSummon);
	}
}
