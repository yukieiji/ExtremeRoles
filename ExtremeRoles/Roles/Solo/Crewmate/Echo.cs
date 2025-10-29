using BepInEx.Unity.IL2CPP.Utils;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using Hazel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;



#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Echo : SingleRoleBase, IRoleAutoBuildAbility
{
	public enum Option
	{
		Range,
		ShowTime,
		AttentionMode,
		IsDetectDeadBody,
		CanSeparatePlayer
	}

	public enum EmitAttentionMode
	{
		EmitAll,
		EmitNotCrewmate,
		EmitDisable,
	}

	public enum RpcOps
	{
		Emit,
		Reset,
	}

	private readonly record struct LocationInfo(Vector3 Pos, bool IsDeadbody, float NormedDistance);
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
	private bool canSeparatePlayer;
	private float pingTime;
	private EmitAttentionMode mode;

	private List<Coroutine?> coroutine = [];
	public ExtremeAbilityButton? Button { get; set; }
	private ObjectPoolBehavior pool
	{
		get
		{
			if (this.innerPool == null)
			{
				var localPlayer = PlayerControl.LocalPlayer;
				var prefab = GameManagerCreator.Instance.HideAndSeekManagerPrefab.PingPool;
				this.innerPool = UnityObject.Instantiate(prefab, localPlayer.transform);
			}
			return this.innerPool;
		}
	}
	private ObjectPoolBehavior? innerPool;

	private static Color emitColor = Palette.CrewmateRoleBlue;

	public Echo() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Echo,
			ColorPalette.AgencyYellowGreen),
		false, true, false, false)
	{

	}

	public static void Rpc(in MessageReader reader)
	{
		byte playerId = reader.ReadByte();
		var ops = (RpcOps)reader.ReadByte();
		
		if (!ExtremeRoleManager.TryGetSafeCastedRole<Echo>(playerId, out var echo))
		{
			return;
		}
		switch (ops)
		{
			case RpcOps.Emit:
				float x = reader.ReadSingle();
				float y = reader.ReadSingle();
				if (!echo.isShowEchoEmitPos())
				{
					return;
				}
				// 大きく見せるため近くで打ったとして表示させる
				var ping = echo.setUpPing(echo.pool, new LocationInfo(new Vector2(x, y), false, 0.1f));
				var newCoroutine = HudManager.Instance.StartCoroutine(echo.showMePing(ping));
				echo.coroutine.Add(newCoroutine);
				break;
			case RpcOps.Reset:
				echo.reset();
				break;
		}
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
		if (this.mode is not EmitAttentionMode.EmitDisable)
		{
			var localPlayer = PlayerControl.LocalPlayer;
			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.EchoOps))
			{
				caller.WriteByte(localPlayer.PlayerId);
				caller.WriteByte((byte)RpcOps.Reset);
			}
		}
		reset();
	}

	private void reset()
	{
		foreach (var c in this.coroutine)
		{
			if (c != null)
			{
				HudManager.Instance.StopCoroutine(c);
			}
		}
		this.coroutine.Clear();

		foreach (var p in this.pool.activeChildren)
		{
			if (!p.IsTryCast<PingBehaviour>(out var ping))
			{
				continue;
			}
			ping.target = Vector3.zero;
			ping.SetImageEnabled(false);
			ping.gameObject.SetActive(false);
		}
	}

	public bool UseAbility()
	{
		if (this.mode is not EmitAttentionMode.EmitDisable)
		{
			var localPlayer = PlayerControl.LocalPlayer;
			var pos = localPlayer.GetTruePosition();
			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.EchoOps))
			{
				caller.WriteByte(localPlayer.PlayerId);
				caller.WriteByte((byte)RpcOps.Emit);
				caller.WriteFloat(pos.x);
				caller.WriteFloat(pos.y);
			}
		}

		var newCoroutine = HudManager.Instance.StartCoroutine(emitEchoLocation());
		this.coroutine.Add(newCoroutine);
		return true;
	}

	private bool isShowEchoEmitPos()
	{
		var role = ExtremeRoleManager.GetLocalPlayerRole();
		return this.mode switch
		{
			EmitAttentionMode.EmitNotCrewmate => !role.IsCrewmate(),
			EmitAttentionMode.EmitAll => true,
			_ => false
		};
	}

	private IEnumerator showMePing(PingBehaviour ping)
	{
		yield return new WaitForSeconds(this.pingTime);
		hidePing(ping);
	}

	// エコーを放ってから距離に従って順次表示、表示された順番から経過時間ごとに消していく
	private IEnumerator emitEchoLocation()
	{
		var source = PlayerControl.LocalPlayer;
		var sourcePos = source.GetTruePosition();

		var allPlayer = PlayerCache.AllPlayerControl;
		var target = new List<LocationInfo>(allPlayer.Count);

		// 1. まずはPingを立てる場所を探す
		/// 通常プレイヤー
		addPlayerLocationInfo(source, target);

		/// 死体
		if (this.isDetectDeadBody)
		{
			addDeadBodyLocationInfo(sourcePos, target);
		}

		int size = target.Count;
		if (size <= 0)
		{
			yield break;
		}

		// 2. 1秒を使い近い順からPingを建てる、ただしすでに建てたPingが経過時間以上経ってると落とすようにする
		target.Sort((a, b) => a.NormedDistance.CompareTo(b.NormedDistance));
		var showPing = new Queue<PingInfo>(size);
		foreach (var item in target)
		{
			yield return waitNextTarget(item, showPing);

			// エコーは1秒で全範囲を探査するのでNormedDistance分経過したとする
			foreach (var p in showPing)
			{
				p.Reduce(item.NormedDistance);
			}

			var ping = setUpPing(this.pool, item);
			showPing.Enqueue(new PingInfo(ping, this.pingTime));
		}

		// 3. 経過時間が残っているPingをすべて探査して消す
		while (showPing.TryDequeue(out var ping))
		{
			yield return new WaitForSeconds(ping.Time);
			hidePing(ping.Ping);
		}

		// 4. あと掃除
		foreach (var p in this.pool.activeChildren)
		{
			if (!p.IsTryCast<PingBehaviour>(out var ping))
			{
				continue;
			}
			hidePing(ping);
		}
	}

	private void addDeadBodyLocationInfo(Vector3 sourcePos, in List<LocationInfo> result)
	{
		foreach (var deadBody in UnityObject.FindObjectsOfType<DeadBody>())
		{
			var pos = deadBody.transform.position;
			var diff = sourcePos - pos;
			float sqrDistance = diff.sqrMagnitude;
			if (sqrDistance <= this.echoLocationRangeSquare)
			{
				// 死体だと見分けられるときだけtrueを入れて判別可能にする
				result.Add(new LocationInfo(pos, this.canSeparatePlayer, sqrDistance / this.echoLocationRangeSquare));
			}
		}
	}

	private void addPlayerLocationInfo(PlayerControl source, in List<LocationInfo> result)
	{
		var sourcePos = source.GetTruePosition();
		foreach (var player in PlayerCache.AllPlayerControl)
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
				result.Add(new LocationInfo(pos, false, sqrDistance / this.echoLocationRangeSquare));
			}
		}
	}

	private PingBehaviour setUpPing(ObjectPoolBehavior pool, in LocationInfo info)
	{
		// 最大距離最小スケール0.2f、最小距離最大スケール1.0fに変換
		float floatedScale = (1.0f - info.NormedDistance) * 0.8f + 0.2f;

		var ping = pool.Get<PingBehaviour>();
		ping.transform.position = new Vector3(0.0f, 0.0f, -900.0f);
		ping.target = info.Pos;
		ping.AmSeeker = false;
		ping.UpdatePosition();
		ping.gameObject.SetActive(true);
		ping.gameObject.layer = 5;
		ping.MaxScale = 0.9f * floatedScale;

		if (ping.image != null)
		{
			ping.image.sortingOrder = 88659;
			if (!info.IsDeadbody)
			{
				ping.image.color = emitColor;
			}
		}
		ping.SetImageEnabled(true);
		return ping;
	}

	private static IEnumerator waitNextTarget(LocationInfo next, Queue<PingInfo> curShowPing)
	{
		float hitTime = next.NormedDistance;
		
		float lastWaitTime = hitTime;
		
		// 次のPingを建てる前に前のPingの表示時間が過ぎているかどうかを確認して
		// 過ぎているのであれば非表示にする
		if (curShowPing.TryPeek(out var first) &&
			first.Time - hitTime <= 0.0f)
		{
			float totalReduce = 0.0f;
			while
				(curShowPing.TryPeek(out var nextPing) &&
				(hitTime - nextPing.Time > 0.0f))
			{
				var removePing = curShowPing.Dequeue();
				float removePingWaitTime = removePing.Time;
				yield return new WaitForSeconds(removePingWaitTime);
				totalReduce += removePingWaitTime;
			}
			lastWaitTime = hitTime - totalReduce;
		}
		yield return new WaitForSeconds(lastWaitTime);
	}

	private static void hidePing(PingBehaviour ping)
	{
		ping.SetImageEnabled(false);
		ping.target = Vector3.zero;
		ping.gameObject.SetActive(false);
	}


	protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
	{
		var factory = categoryScope.Builder;
		IRoleAbility.CreateAbilityCountOption(factory, 3, 50);
		factory.CreateFloatOption(Option.Range, 10.0f, 5.0f, 30.0f, 0.5f);
		factory.CreateSelectionOption<Option, EmitAttentionMode>(Option.AttentionMode);

		factory.CreateFloatOption(Option.ShowTime, 2.0f, 0.5f, 15.0f, 0.25f, format: OptionUnit.Second);
		var deadBodyOpt = factory.CreateBoolOption(Option.IsDetectDeadBody, false);
		factory.CreateBoolOption(Option.CanSeparatePlayer, false, deadBodyOpt);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		float range = loader.GetValue<Option, float>(Option.Range);
		this.echoLocationRangeSquare = range * range;

		this.canSeparatePlayer = loader.GetValue<Option, bool>(Option.CanSeparatePlayer);
		this.pingTime = loader.GetValue<Option, float>(Option.ShowTime);
		this.isDetectDeadBody = loader.GetValue<Option, bool>(Option.IsDetectDeadBody);
		this.mode = (EmitAttentionMode)loader.GetValue<Option, int>(Option.AttentionMode);
	}
}
