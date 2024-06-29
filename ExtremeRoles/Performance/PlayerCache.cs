using System;
using System.Collections.Generic;

namespace ExtremeRoles.Performance;

// Il2Cpp
public static class PlayerCache
{
	public static readonly List<PlayerControl> AllPlayerControl = new List<PlayerControl>();

	public static　void AddPlayerControl(PlayerControl pc)
	{
		AllPlayerControl.Remove(pc);
		AllPlayerControl.Add(pc);
	}

	public static void RemovePlayerControl(PlayerControl pc)
	{
		AllPlayerControl.RemoveAll(p => p.Pointer == pc.Pointer);
	}
}
