using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Module.RoleAssign;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
	[Il2CppRegister]
	public sealed class TimeMasterHistory : MonoBehaviour
	{
		public readonly record struct History(Vector3 Pos, bool CanMove, bool InVent, bool IsUsed);

		public bool BlockAddHistory { get; set; }
		public int Size { get; private set; }

		// 座標、動けるか、ベント内か, 何か使ってるか
		private Queue<History> history = new Queue<History>();
		private bool init = false;

		public TimeMasterHistory(IntPtr ptr) : base(ptr) { }

		public void Awake()
		{
			this.Clear();
		}

		public void FixedUpdate()
		{
			if (!this.init || this.BlockAddHistory ||
				!RoleAssignState.Instance.IsRoleSetUpEnd ||
				AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started ||
				MeetingHud.Instance ||
				ExileController.Instance)
			{
				return;
			}

			PlayerControl player = CachedPlayerControl.LocalPlayer;

			int overflow = this.history.Count - this.Size;
			for (int i = 0; i < overflow; ++i)
			{
				this.history.Dequeue();
			}

			this.history.Enqueue(
				new History(
					player.transform.position,
					player.CanMove,
					player.inVent,
					!(
						player.Collider.enabled ||
						player.NetTransform.enabled ||
						player.moveable
					)
				)
			);
		}

		public void Clear()
		{
			ResetAfterRewind();
			this.init = false;
			this.Size = 0;
		}

		public void ResetAfterRewind()
		{
			this.history.Clear();
			this.BlockAddHistory = false;
		}

		public void Initialize(float historySecond)
		{
			this.Size = (int)Mathf.Round(historySecond / Time.fixedDeltaTime);
			this.init = true;
		}

		[HideFromIl2Cpp]
		public IEnumerable<History> GetAllHistory() => history.Reverse();
	}
}
