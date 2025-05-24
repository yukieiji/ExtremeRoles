using System;
using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using UnityEngine;
using UnityEngine.Video;

namespace ExtremeRoles.Test.Lobby.Asset;

public class ZombieAssetLoadRunner
	: AssetLoadRunner
{
	public override void Run()
	{
		Log.LogInfo($"----- Unit:ZombieLoad Test -----");

		LoadFromExR(ExtremeRoleId.Zombie);
		LoadFromExR(ExtremeRoleId.Zombie, ObjectPath.MapIcon);
		LoadUnityObjectFromExR<VideoClip, ExtremeRoleId>(
			ExtremeRoleId.Zombie,
			ObjectPath.GetRoleVideoPath(ExtremeRoleId.Zombie));
	}
}
