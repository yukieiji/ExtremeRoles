using System;
using System.Collections.Generic;

namespace ExtremeRoles.Performance;

// Il2Cpp
public static class PlayerCache
{
	public static IReadOnlyList<PlayerControl> AllPlayerControl => allPlayerControlCache;

	private static readonly List<PlayerControl> allPlayerControlCache = [];

	public static　void AddPlayerControl(PlayerControl pc)
	{
		lock (allPlayerControlCache)
		{
			allPlayerControlCache.Remove(pc);
			allPlayerControlCache.Add(pc);
		}
	}

	public static void RemovePlayerControl(PlayerControl pc)
	{
		RemovePlayerControl(p => p.Pointer == pc.Pointer);
	}

	public static void RemovePlayerControl(Predicate<PlayerControl> predicate)
	{
		lock (allPlayerControlCache)
		{
			allPlayerControlCache.RemoveAll(predicate);
		}
	}
}
