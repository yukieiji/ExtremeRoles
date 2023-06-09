using System;

using UnityEngine;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Helper;


#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class MinerMineEffect : MonoBehaviour, IMeetingResetObject
{
	private bool isActive = false;
	private static AudioClip? cacheedClip;

	private float minDistance;
	private float maxDistance;
	private float range;

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
			cacheedClip = Sound.GetAudio(Sound.SoundType.MinerMineSE);
		}

		this.audioSource.clip = cacheedClip;
		this.audioSource.volume = 0.0f;
		this.audioSource.Play();
	}

	public void SetParameter(float activeRange)
	{
		this.minDistance = activeRange + 0.5f;
		this.maxDistance = this.minDistance * 2.0f;
		this.range = this.maxDistance - this.minDistance;
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

		if (!this.isActive)
		{
			return;
		}


		if (player.Data.IsDead ||
			player.Data.Disconnected) { return; }

		this.audioSource.volume = 1.0f - calculateNormalizedDistance(
			base.transform.position,
			player.PlayerControl.GetTruePosition(),
			this.range, this.minDistance, this.maxDistance);
	}

	public void Clear()
	{
		Destroy(base.gameObject);
	}

	private static float calculateNormalizedDistance(
		Vector2 objPos, Vector2 targetPos,
		float volumeRange, float minDistance, float maxDistance)
	{
		Vector2 diff = objPos - targetPos;
		float distance = diff.magnitude;

		float clampDistance = Mathf.Clamp(distance, minDistance, maxDistance);
		float normalizedDistance = (clampDistance - minDistance) / volumeRange;

		return normalizedDistance;
	}
}
