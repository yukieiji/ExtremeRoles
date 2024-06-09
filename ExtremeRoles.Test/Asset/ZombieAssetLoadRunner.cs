using System;
using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Asset;

internal sealed class ZombieAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:ZombieLoad Test -----");

		LoadFromExR(ExtremeRoleId.Zombie);
		LoadFromExR(ExtremeRoleId.Zombie, Path.MapIcon);
		LoadUnityObjectFromExR<VideoClip, ExtremeRoleId>(
			ExtremeRoleId.Zombie,
			Path.GetRoleVideoPath(ExtremeRoleId.Zombie));
	}
}
