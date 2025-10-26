using System.Collections;
using System.Collections.Generic;
using System.Linq;

using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Echo : SingleRoleBase, IRoleAutoBuildAbility
{
	public enum Option
	{
		Range,
		ShowTime,
		IsDetectDeadBody,
		CanSeparatePlayer
	}

	private readonly record struct LocationInfo(Vector3 Pos, bool IsDeadbody, float Distance);
	private sealed class PingInfo(PingBehaviour ping, float time)
	{
		public PingBehaviour Ping { get; } = ping;
		public float Time { get; private set; } = time;

		public void Reduce(float delta)
		{
			this.Time -= delta; 
		}
	}

	private float echoLocationRangeSquare;
	private bool isDetectDeadBody;
	private float pingTime;

	private List<Coroutine?> coroutine = [];
	public ExtremeAbilityButton? Button { get; set; }
	private ObjectPoolBehavior? pool;

	public Echo() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Echo,
			ColorPalette.AgencyYellowGreen),
		false, true, false, false)
	{

	}

	public void CreateAbility()
	{

		this.CreateAbilityCountButton(
			"echoLocation",
			UnityObjectLoader.LoadSpriteFromResources(ObjectPath.TestButton));
		this.Button?.SetLabelToCrewmate();
	}

	public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse();

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void ResetOnMeetingStart()
	{
		foreach (var c in this.coroutine)
		{
			if (c != null)
			{
				HudManager.Instance.StopCoroutine(c);
			}
		}
		this.coroutine.Clear();

		if (this.pool == null)
		{
			return;	
		}

		foreach (var pool in this.pool.activeChildren)
		{
			if (!pool.IsTryCast<PingBehaviour>(out var arrow))
			{
				return;
			}
			arrow.target = Vector3.zero;
			arrow.SetImageEnabled(false);
			arrow.gameObject.SetActive(false);
		}
	}

	public bool UseAbility()
	{
		var newCoroutine = HudManager.Instance.StartCoroutine(emitEchoLocation());
		this.coroutine.Add(newCoroutine);
		return true;
	}

	private IEnumerator emitEchoLocation()
	{
		var source = PlayerControl.LocalPlayer;
		var sourcePos = source.GetTruePosition();

		if (this.pool == null)
		{
			var prefab = GameManagerCreator.Instance.HideAndSeekManagerPrefab.PingPool;
			this.pool = Object.Instantiate(prefab, source.transform);
		}

		var allPlayer = PlayerCache.AllPlayerControl;
		var target = new List<LocationInfo>(allPlayer.Count);

		foreach (var player in allPlayer)
		{
			if (!player.IsValid() || 
				player.PlayerId == source.PlayerId)
			{
				continue;
			}

			var pos = player.GetTruePosition();
			var diff = sourcePos - pos;
			float sqrDistance = diff.sqrMagnitude;
			
			if (sqrDistance <= this.echoLocationRangeSquare)
			{
				target.Add(new LocationInfo(pos, false, sqrDistance / this.echoLocationRangeSquare));
			}
		}

		if (this.isDetectDeadBody)
		{
			foreach (var deadBody in Object.FindObjectsOfType<DeadBody>())
			{
				Vector2 vec2 = deadBody.transform.position;
				var diff = sourcePos - vec2;
				float sqrDistance = diff.sqrMagnitude;
				if (sqrDistance <= this.echoLocationRangeSquare)
				{
					target.Add(new LocationInfo(vec2, true, sqrDistance / this.echoLocationRangeSquare));
				}
			}
		}

		int size = target.Count;
		if (size <= 0)
		{
			yield break;
		}

		var sorted = target.OrderBy(x => x.Distance);

		var showPing = new Queue<PingInfo>(size);
		foreach (var item in sorted)
		{
			float hitTime = item.Distance;

			if (showPing.TryPeek(out var first) &&
				first.Time - hitTime <= 0.0f)
			{
				float totalRedule = 0.0f;
				while 
					(showPing.TryPeek(out var nextPing) && 
					(hitTime - nextPing.Time > 0.0f))
				{
					var removePing = showPing.Dequeue();
					float removePingWaitTime = removePing.Time;
					yield return new WaitForSeconds(removePingWaitTime);
					totalRedule += removePingWaitTime;
				}
				yield return new WaitForSeconds(hitTime - totalRedule);
			}
			else
			{
				yield return new WaitForSeconds(hitTime);
			}

			foreach (var p in showPing)
			{
				p.Reduce(hitTime);
			}

			var ping = this.pool.Get<PingBehaviour>();
			ping.transform.position = new Vector3(0.0f, 0.0f, -900.0f);
			ping.target = item.Pos;
			ping.AmSeeker = false;
			ping.UpdatePosition();
			ping.gameObject.SetActive(true);
			ping.gameObject.layer = 5;
			if (ping.image != null)
			{
				ping.image.sortingOrder = 88659;
			}
			ping.SetImageEnabled(true);
			showPing.Enqueue(new PingInfo(ping, this.pingTime));
		}

		while (showPing.TryDequeue(out var ping))
		{
			yield return new WaitForSeconds(ping.Time);
			hidePing(ping.Ping);
		}
		
		foreach (var p in this.pool.activeChildren)
		{
			if (!p.IsTryCast<PingBehaviour>(out var ping))
			{
				continue;
			}
			hidePing(ping);
		}
	}

	private static void hidePing(PingBehaviour ping)
	{
		ping.SetImageEnabled(false);
		ping.target = Vector3.zero;
		ping.gameObject.SetActive(false);
	}


	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 3, 50);
		factory.CreateFloatOption(Option.Range, 10.0f, 5.0f, 30.0f, 0.5f);
		factory.CreateFloatOption(Option.ShowTime, 2.0f, 0.5f, 15.0f, 0.25f, format: OptionUnit.Second);
		var deadDodyOpt = factory.CreateBoolOption(Option.IsDetectDeadBody, false);
		factory.CreateBoolOption(Option.CanSeparatePlayer, false, deadDodyOpt);
	}

	protected override void RoleSpecificInit()
	{
		float range = this.Loader.GetValue<Option, float>(Option.Range);
		this.echoLocationRangeSquare = range * range;
		this.pingTime = this.Loader.GetValue<Option, float>(Option.ShowTime);
		this.isDetectDeadBody = this.Loader.GetValue<Option, bool>(Option.IsDetectDeadBody);
	}
}
