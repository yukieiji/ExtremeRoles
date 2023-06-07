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
using ExtremeRoles.Resources;
using ExtremeRoles.Helper;


#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class MinerMineEffect : MonoBehaviour, IMeetingResetObject
{
	public int Id { private get; set; }

	private bool isActive = false;
	private static AudioClip? cacheedClip;

#pragma warning disable CS8618
	private SpriteRenderer rend;
	private AudioSource audioSource;

	public MinerMineEffect(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618

	public void Awake()
	{
		this.rend = base.gameObject.AddComponent<SpriteRenderer>();
		this.audioSource = base.gameObject.AddComponent<AudioSource>();
		this.audioSource.outputAudioMixerGroup = SoundManager.Instance.SfxChannel;

		this.audioSource.loop = true;

		if (cacheedClip == null)
		{
			cacheedClip = Loader.GetUnityObjectFromResources<AudioClip>(
				Path.SoundEffect, string.Format(
					Sound.SoundPlaceHolder, "Mine"));
		}

		this.audioSource.clip = cacheedClip;
		this.audioSource.Play();
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

		Vector2 pos = base.transform.position;
		Vector2 diff = player.PlayerControl.GetTruePosition() - pos;

	}

	public void Clear()
	{
		Destroy(base.gameObject);
	}
}
