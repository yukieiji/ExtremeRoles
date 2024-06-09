using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Asset;

internal sealed class ThiefAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:ThiefLoad Test -----");

		LoadFromExR(ExtremeRoleId.Thief);
		LoadFromExR(ExtremeRoleId.Thief, Path.MapIcon);
		LoadUnityObjectFromExR<VideoClip, ExtremeRoleId>(
			ExtremeRoleId.Thief,
			Path.GetRoleVideoPath(ExtremeRoleId.Thief));
	}
}
