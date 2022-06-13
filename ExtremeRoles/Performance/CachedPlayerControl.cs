using System;
using System.Collections.Generic;

using UnityEngine;

// from TOR : https://github.com/Eisbison/TheOtherRoles/blob/main/TheOtherRoles/Players/CachedPlayer.cs

namespace ExtremeRoles.Performance
{
    public class CachedPlayerControl
	{
		public static readonly Dictionary<IntPtr, CachedPlayerControl> PlayerPtrs = new Dictionary<IntPtr, CachedPlayerControl>();
		public static readonly List<CachedPlayerControl> AllPlayerControl = new List<CachedPlayerControl>();
		public static CachedPlayerControl LocalPlayer;

		public Transform transform;
		public PlayerControl PlayerControl;
		public PlayerPhysics PlayerPhysics;
		public CustomNetworkTransform NetTransform;
		public GameData.PlayerInfo Data;
		public byte PlayerId;

		public CachedPlayerControl(PlayerControl pc)
        {
			transform = pc.transform;
			PlayerControl = pc;
			PlayerPhysics = pc.MyPhysics;
            NetTransform = pc.NetTransform;
			AllPlayerControl.Add(this);
			PlayerPtrs[pc.Pointer] = this;
		}

		public static void Remove(PlayerControl pc)
        {
			AllPlayerControl.RemoveAll(
				p => p.PlayerControl.Pointer == pc.Pointer);
			PlayerPtrs.Remove(pc.Pointer);
		}


		public static implicit operator bool(CachedPlayerControl player)
		{
			return player != null && player.PlayerControl;
		}

		public static implicit operator PlayerControl(CachedPlayerControl player) => player.PlayerControl;

		public static implicit operator PlayerPhysics(CachedPlayerControl player) => player.PlayerPhysics;
	}
}
