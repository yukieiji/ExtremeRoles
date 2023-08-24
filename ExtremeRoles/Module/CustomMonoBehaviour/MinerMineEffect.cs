using System;

using UnityEngine;
using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.Solo.Neutral;


#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class MinerMineEffect : MonoBehaviour, IMeetingResetObject
{
	private bool isActive = false;
	private static AudioClip? cacheedClip;
	private static Sprite? cachedActiveSprite;
	private static Sprite? cachedDeactiveSprite;

	private float minDistance;
	private float maxDistance;
	private float range;

	private bool isUseEffect = false;
	private Miner.ShowMode showMode;
	private bool isShowNoneActiveImg;

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

	[HideFromIl2Cpp]
	public void SetParameter(
		bool isRolePlayer,
		float activeRange,
		in Miner.MineEffectParameter param)
	{
		this.isUseEffect = param.RolePlayerShowMode != Miner.ShowMode.MineSeeNone;
		this.isShowNoneActiveImg = isRolePlayer || param.CanShowNoneActiveAtherPlayer;
		this.showMode = isRolePlayer ?
			param.RolePlayerShowMode : param.AnotherPlayerShowMode;

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

		if (!this.isUseEffect ||
			player == null ||
			player.Data == null ||
			CachedShipStatus.Instance == null ||
			!CachedShipStatus.Instance.enabled ||
			GameData.Instance == null ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null ||
			ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return; }

		playerUpdate(player);
	}

	public void Clear()
	{
		if (this != null)
		{
			Destroy(this.gameObject);
		}
	}

	private void playerUpdate(PlayerControl localPlayer)
	{
		switch (this.showMode)
		{
			case Miner.ShowMode.MineSeeOnlySe:
				if (!this.isActive)
				{
					setDeactivateSprite();
				}
				else
				{
					updateVolume(localPlayer);
				}
				break;
			case Miner.ShowMode.MineSeeOnlyImg:
				if (this.isActive)
				{
					setActivateSprite();
				}
				else if (this.isShowNoneActiveImg)
				{
					setDeactivateSprite();
				}
				break;
			case Miner.ShowMode.MineSeeBoth:
				if (this.isActive)
				{
					setActivateSprite();
					updateVolume(localPlayer);
				}
				else if (this.isShowNoneActiveImg)
				{
					setDeactivateSprite();
				}
				break;
			default:
				break;
		}
	}

	private void setActivateSprite()
	{
		if (cachedActiveSprite == null)
		{
			cachedActiveSprite = Loader.CreateSpriteFromResources(
				Path.MinerActiveMineImg);
		}
		this.rend.sprite = cachedActiveSprite;
	}
	private void setDeactivateSprite()
	{
		if (cachedDeactiveSprite == null)
		{
			cachedDeactiveSprite = Loader.CreateSpriteFromResources(
				Path.MinerDeactivateMineImg);
		}
		this.rend.sprite = cachedDeactiveSprite;
	}

	private void updateVolume(PlayerControl localPlayer)
	{
		var data = localPlayer.Data;
		if (data.IsDead || data.Disconnected) { return; }

		this.audioSource.volume = 1.0f - calculateNormalizedDistance(
			base.transform.position,
			localPlayer.GetTruePosition(),
			this.range, this.minDistance, this.maxDistance);
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
