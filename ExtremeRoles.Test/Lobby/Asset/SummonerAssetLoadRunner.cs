using ExtremeRoles.Resources;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Test.Lobby.Asset;

public class SummonerAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:SummonerAsset Test -----");

		LoadFromExR(ExtremeRoleId.Summoner, ObjectPath.SummonerMarking);
		LoadFromExR(ExtremeRoleId.Summoner, ObjectPath.SummonerSummon);
		yield break;
	}
}
