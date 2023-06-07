using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Neutral;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class MinerMineEffect : MonoBehaviour, IMeetingResetObject
{
	public int Id { private get; set; }

	private bool isActive = false;

#pragma warning disable CS8618
	private SpriteRenderer rend;

	public MinerMineEffect(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618

	public void Awake()
	{
		this.rend = base.gameObject.AddComponent<SpriteRenderer>();
	}

	public void SwithAcitve()
	{
		this.isActive = true;
	}

	public void Update()
	{
		var player = CachedPlayerControl.LocalPlayer;

		if (player == null ||
			player.Data == null ||
			CachedShipStatus.Instance == null ||
			!CachedShipStatus.Instance.enabled ||
			GameData.Instance == null ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null ||
			ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }
	}

	public void Clear()
	{
		Destroy(base.gameObject);
	}
}
