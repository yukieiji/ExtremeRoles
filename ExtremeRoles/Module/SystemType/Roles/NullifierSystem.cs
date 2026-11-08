using System.Collections.Generic;

using Hazel;
using TMPro;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor.Nullifier;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class NullifierSystem(
	float range,
	int requiredClicks,
	float restoreTime,
	NullifierTargetTeam targetTeam,
	IUnityObjectFactory unityObjectFactory) : IDirtableSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.NullifierSystem;

	public enum Ops : byte
	{
		ReserveSeal,
		DestroySeal,
	}

	public sealed class NullifierConsoleBehavior(int sealId, int initialClicks, TextMeshPro textLabel) : ExtremeConsole.IBehavior
	{
		public float CoolTime => 0.0f;
		public bool IsCheckWall => false;

		private readonly int sealId = sealId;
		private readonly TextMeshPro textLabel = textLabel;
		private readonly int maxClicks = initialClicks;

		private int remainingClicks = initialClicks;
		private float inactiveTimer = 0.0f;

		public void UpdateTimer(float deltaTime, float restoreTime)
		{
			if (this.remainingClicks >= this.maxClicks)
			{
				return;
			}
			this.inactiveTimer += deltaTime;
			
			if (this.inactiveTimer < restoreTime)
			{
				return;
			}

			this.remainingClicks = this.maxClicks;
			this.inactiveTimer = 0.0f;
			UpdateText();
		}

		public void UpdateText()
		{
			this.textLabel.text = this.remainingClicks.ToString();
			this.textLabel.gameObject.SetActive(this.remainingClicks != this.maxClicks);
		}

		public bool CanUse(NetworkedPlayerInfo pc)
		{
			return pc.IsAlive();
		}

		public void Use()
		{
			this.remainingClicks--;
			this.inactiveTimer = 0.0f;
			UpdateText();

			if (this.remainingClicks > 0)
			{
				return;
			}

			ExtremeSystemTypeManager.RpcUpdateSystem(
				Type,
				writer =>
				{
					writer.Write((byte)Ops.DestroySeal);
					writer.WritePacked(this.sealId);
				});
		}
	}

	public sealed class SealCircle(NullifierConsoleBehavior behavior, GameObject obj)
	{
		public Vector2 Position => this.obj.transform.position;
		private readonly GameObject obj = obj;
		private readonly NullifierConsoleBehavior behavior = behavior;

		public void UpdateTimer(float deltaTime, float restoreTime)
			=> this.behavior.UpdateTimer(deltaTime, restoreTime);

		public void Destroy()
		{
			if (this.obj != null)
			{
				Object.Destroy(this.obj);
			}
		}
	}

	public sealed class ReservePos
	{
		public Vector2 Postion { get; }
		private GameObject? obj;

		public ReservePos(GameObject obj)
		{
			this.obj = obj;
			this.Postion = obj.transform.position;
		}

		public ReservePos(Vector2 pos)
		{
			this.Postion = pos;
		}

		public void Destroy()
		{
			if (this.obj != null)
			{
				Object.Destroy(this.obj);
			}
		}
	}

	private readonly float range = range;
	private readonly float size = range * 2;
	private readonly int requiredClicks = requiredClicks;
	private readonly float restoreTime = restoreTime;
	private readonly NullifierTargetTeam targetTeam = targetTeam;
	private readonly IUnityObjectFactory unityObjectFactory = unityObjectFactory;
	private readonly ButtonLockSystem abilityLocker = ButtonLockSystem.CreateOrGetAbilityButtonLockSystem();
	private readonly ExtremeConsoleSystem consoleSystem = ExtremeConsoleSystem.Create();

	private readonly List<ReservePos> scheduledReservations = [];
	private readonly Dictionary<int, SealCircle> placedSeals = [];
	private int nextSealId;

	public bool IsDirty => false;

	public NullifierSystem(
		float range,
		int requiredClicks,
		float restoreTime,
		NullifierTargetTeam targetTeam)
		: this(range, requiredClicks, restoreTime, targetTeam, new DefaultUnityObjectFactory())
	{
	}

	public void ReserveSeal(Vector2 position, bool isLoaclPlayer)
	{
		var reserve = isLoaclPlayer ? 
			new ReservePos(createReserveLocalPlayerObj(position)) : new ReservePos(position);
		this.scheduledReservations.Add(reserve);
	}

	public void RpcReserveSeal(Vector2 position)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			Type,
			writer =>
			{
				writer.Write((byte)Ops.ReserveSeal);
				writer.Write(position.x);
				writer.Write(position.y);
			});
	}

	public bool IsPlayerInSeal(PlayerControl? player)
	{
		if (player.IsInValid())
		{
			return false;
		}

		Vector2 playerPos = player.GetTruePosition();
		foreach (var seal in this.placedSeals.Values)
		{
			var diff = playerPos - seal.Position;
			if (diff.magnitude <= this.range)
			{
				return true;
			}
		}

		return false;
	}

	public bool IsTargetPlayer(PlayerControl? player)
	{
		if (player.IsInValid() ||
			!ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) ||
			role.IsImpostor())
		{
			return false;
		}

		return this.targetTeam switch
		{
			NullifierTargetTeam.AllImpostorOther => true,
			NullifierTargetTeam.CrewmateOnly => role.IsCrewmate(),
			NullifierTargetTeam.NeutralOnly => role.IsNeutral(),
			NullifierTargetTeam.LiberalOnly => role.IsLiberal(),
			NullifierTargetTeam.BothCrewAndNeutral => role.IsCrewmate() || role.IsNeutral(),
			NullifierTargetTeam.BothCrewAndLiberal => role.IsCrewmate() || role.IsLiberal(),
			NullifierTargetTeam.BothLiberalAndNeutral => role.IsNeutral() || role.IsLiberal(),
			_ => false,
		};
	}

	public void Deteriorate(float deltaTime)
	{
		if (MeetingHud.Instance != null || ExileController.Instance != null)
		{
			return;
		}

		// 各個人のBehaviorタイマー進行（ローカルで管理）
		foreach (var seal in this.placedSeals.Values)
		{
			seal.UpdateTimer(deltaTime, this.restoreTime);
		}

		// ローカルプレイヤーが対象陣営かつ封印陣の範囲内にいるか判定
		var localPlayer = PlayerControl.LocalPlayer;
		if (IsTargetPlayer(localPlayer) && IsPlayerInSeal(localPlayer))
		{
			this.abilityLocker.Lock((int)ButtonLockSystem.ConditionId.Nullifier);
		}
		else
		{
			this.abilityLocker.UnLock((int)ButtonLockSystem.ConditionId.Nullifier);
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing is ResetTiming.MeetingStart)
		{
			this.abilityLocker.UnLock((int)ButtonLockSystem.ConditionId.Nullifier);
		}
		else if (timing is ResetTiming.MeetingEnd)
		{
			// 会議終了時に予約された位置に陣を設置
			foreach (var res in this.scheduledReservations)
			{
				res.Destroy();
				createSeal(this.nextSealId++, res.Postion);
			}
			this.scheduledReservations.Clear();
		}
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.ReserveSeal:
				float rx = msgReader.ReadSingle();
				float ry = msgReader.ReadSingle();
				var pos = new Vector2(rx, ry);
				ReserveSeal(
					pos, 
					PlayerControl.LocalPlayer != null &&
					player.PlayerId == PlayerControl.LocalPlayer.PlayerId);
				break;

			case Ops.DestroySeal:
				int destroyId = msgReader.ReadPackedInt32();
				destroySeal(destroyId);
				break;
		}
	}

	private void createSeal(int id, Vector2 pos)
	{
		if (this.placedSeals.ContainsKey(id))
		{
			return;
		}

		var hud = HudManager.Instance;
		var console = this.consoleSystem.CreateConsoleObj(pos, $"NullifierSeal_{id}");
		if (console.Image != null)
		{
			setSpriteRenderer(console.gameObject, console.Image);
		}

		var textObj = Object.Instantiate(
			hud.KillButton.cooldownTimerText,
			console.transform,
			false);
		textObj.gameObject.SetActive(true);
		textObj.transform.localPosition = new Vector3(0.0f, -0.05f, -1.0f);
		float textSize = 1.0f / this.size;
		textObj.transform.localScale = new Vector3(textSize, textSize, 0.5f);
		textObj.alignment = TextAlignmentOptions.Center;
		textObj.color = Color.white;

		var behavior = new NullifierConsoleBehavior(id, this.requiredClicks, textObj);
		console.Behavior = behavior;

		var collider = console.gameObject.AddComponent<CircleCollider2D>();
		collider.isTrigger = true;
		collider.radius = 0.5f;

		behavior.UpdateText();

		this.placedSeals[id] = new SealCircle(behavior, console.gameObject);
	}

	private GameObject createReserveLocalPlayerObj(Vector2 pos)
	{
		var obj = this.unityObjectFactory.CreateGameObject("NullifierReseve");
		obj.transform.position = this.unityObjectFactory.CreateMapPos(pos);

		float size = range * 2;
		obj.transform.localScale = new Vector3(size, size, 1.0f);

		var rend = obj.AddComponent<SpriteRenderer>();
		setSpriteRenderer(obj, rend);
		
		var color = Color.gray;
		color.a = 0.5f;
		rend.color = color;

		return obj;
	}

	private void setSpriteRenderer(GameObject obj, SpriteRenderer rend)
	{
		rend.sprite = this.unityObjectFactory.LoadRoleSprite(ExtremeRoleId.Nullifier);
		var color = rend.color;
		color.a = 0.8f;
		rend.color = color;
		obj.transform.localScale = new Vector3(this.size, this.size, 1.0f);
	}

	private void destroySeal(int id)
	{
		if (this.placedSeals.TryGetValue(id, out var seal))
		{
			seal.Destroy();
			this.placedSeals.Remove(id);
		}

		// 破壊後に自分が範囲外になった場合の即時チェック
		var localPlayer = PlayerControl.LocalPlayer;
		this.abilityLocker.UnLock((int)ButtonLockSystem.ConditionId.Nullifier);
	}

	public void MarkClean()
	{
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{
	}
}
