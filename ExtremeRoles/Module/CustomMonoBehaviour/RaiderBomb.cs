using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using BepInEx.Unity.IL2CPP.Utils.Collections;

using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class RaiderBomb : MonoBehaviour
{
	public sealed record Parameter(float Range, float Time, bool IsShowOtherPlayer);

	public RaiderBomb(IntPtr ptr) : base(ptr) { }

	private SpriteRenderer? rend;
	private Parameter? param;
	private Coroutine? coroutine;

	private float timer = 0.0f;
	private bool isShowOther;

	public void SetParameter(Parameter param)
	{
		this.param = param;
		var role = ExtremeRoleManager.GetLocalPlayerRole();
		this.rend = base.gameObject.AddComponent<SpriteRenderer>();
		this.rend.sprite = UnityObjectLoader.LoadFromResources<Sprite, ExtremeRoleId>(
			ExtremeRoleId.Raider,
			ObjectPath.GetRoleImgPath(ExtremeRoleId.Raider, "Bomb"));
		this.isShowOther = param.IsShowOtherPlayer;
		this.rend.enabled = false;

		var lunchSe = base.gameObject.AddComponent<AudioSource>();
		lunchSe.clip = UnityObjectLoader.LoadFromResources<AudioClip, ExtremeRoleId>(
			ExtremeRoleId.Raider,
			string.Format(
				ObjectPath.RoleSePathFormat,
				$"{ExtremeRoleId.Raider}.Launch"));

		// オリジナルのクリップの長さ
		float originalDuration = lunchSe.clip.length;
		lunchSe.pitch = originalDuration / this.param.Time;

		lunchSe.spatialBlend = 1.0f;
		lunchSe.rolloffMode = AudioRolloffMode.Linear;
		lunchSe.minDistance = this.param.Range * 0.75f;
		lunchSe.maxDistance = this.param.Range * 2.5f;
		lunchSe.outputAudioMixerGroup = SoundManager.Instance.SfxChannel;

		lunchSe.Play();
	}

	public void FixedUpdate()
	{
		if (this.param is null ||
			this.rend == null)
		{
			return;
		}

		updateRendEnable();

		this.timer += Time.fixedDeltaTime;
		float time = this.param.Time;
		float scale = this.timer / time;
		base.transform.localScale = new Vector3(scale, scale, scale);

		if (this.timer < this.param.Time)
		{
			return;
		}

		if (AmongUsClient.Instance.AmHost)
		{
			killPlayer(this.param.Range);
		}

		this.rend.enabled = false;
		if (this.coroutine != null)
		{
			return;
		}

		this.coroutine = StartCoroutine(coDestroy().WrapToIl2Cpp());
	}

	private IEnumerator coDestroy()
	{
		if (this.param is null)
		{
			yield break;
		}

		var explode = base.gameObject.AddComponent<AudioSource>();
		explode.clip = UnityObjectLoader.LoadFromResources<AudioClip, ExtremeRoleId>(
			ExtremeRoleId.Raider,
			string.Format(
				ObjectPath.RoleSePathFormat,
				$"{ExtremeRoleId.Raider}.Explosion"));

		explode.spatialBlend = 1.0f;
		explode.rolloffMode = AudioRolloffMode.Linear;
		explode.minDistance = this.param.Range * 0.75f;
		explode.maxDistance = this.param.Range * 2.5f;
		explode.outputAudioMixerGroup = SoundManager.Instance.SfxChannel;
		explode.Play();

		var waiter = new WaitForFixedUpdate();

		while (explode.isPlaying)
		{
			yield return waiter;
		}
		Destroy(base.gameObject);
	}

	private void killPlayer(float range)
	{
		Vector2 pos = base.transform.position;
		var killedPlayer = new HashSet<byte>(GameData.Instance.AllPlayers.Count);
		foreach (NetworkedPlayerInfo playerInfo in
			GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (playerInfo == null ||
				playerInfo.IsDead ||
				playerInfo.Disconnected ||
				playerInfo.Object == null)
			{
				continue;
			}

			PlayerControl target = playerInfo.Object;
			byte targetPlayerId = playerInfo.PlayerId;

			if ((
					ExtremeRoleManager.TryGetSafeCastedRole<Assassin>(playerInfo.PlayerId, out var assassin) &&
					assassin.CanKilled
				))
			{
				continue;
			}

			Vector2 vector = target.GetTruePosition() - pos;
			float magnitude = vector.magnitude;
			if (magnitude <= range &&
				!PhysicsHelpers.AnyNonTriggersBetween(
					pos, vector.normalized,
					magnitude, Constants.ShipAndObjectsMask))
			{
				if (BodyGuard.TryGetShiledPlayerId(targetPlayerId, out byte shieldId))
				{
					targetPlayerId = shieldId;
				}
				killedPlayer.Add(targetPlayerId);
			}
		}

		foreach (byte playerId in killedPlayer)
		{
			Helper.Player.RpcUncheckMurderPlayer(
				playerId, playerId, byte.MinValue);
		}
	}

	private void updateRendEnable()
	{
		if (this.rend == null ||
			PlayerControl.LocalPlayer == null ||
			PlayerControl.LocalPlayer.Data == null)
		{
			return;
		}

		var role = ExtremeRoleManager.GetLocalPlayerRole();
		this.rend.enabled =
			this.isShowOther ||
			role.IsImpostor() ||
			PlayerControl.LocalPlayer.Data.IsDead;
	}

}
