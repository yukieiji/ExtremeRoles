using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Asset;

internal sealed class YokoAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:YokofLoad Test -----");

		LoadFromExR(ExtremeRoleId.Yoko);
		LoadUnityObjectFromExR<GameObject, ExtremeRoleId>(
			ExtremeRoleId.Yoko,
			Path.GetRoleMinigamePath(ExtremeRoleId.Yoko));
	}
}
