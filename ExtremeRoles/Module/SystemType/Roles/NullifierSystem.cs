using System;
using System.Collections.Generic;
using Hazel;
using TMPro;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.Solo.Impostor.Nullifier;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class NullifierSystem : IDirtableSystemType
{
	public const ExtremeSystemType Type = ExtremeSystemType.Nullifier;

	public enum Ops : byte
	{
		ReserveSeal,
		DestroySeal,
	}

	public sealed class NullifierConsoleBehavior(int sealId, int initialClicks, TextMeshPro? textLabel) : ExtremeConsole.IBehavior
	{
		public float CoolTime => 0.0f;
		public bool IsCheckWall => false;

		private readonly int sealId = sealId;
		private readonly TextMeshPro? textLabel = textLabel;
		private int remainingClicks = initialClicks;
		private float inactiveTimer = 0.0f;

		public int RemainingClicks => this.remainingClicks;
		public float InactiveTimer => this.inactiveTimer;

		public void UpdateTimer(float deltaTime, float restoreTime, int maxClicks)
		{
			if (this.remainingClicks < maxClicks)
			{
				this.inactiveTimer += deltaTime;
				if (this.inactiveTimer >= restoreTime)
				{
					this.remainingClicks = maxClicks;
					this.inactiveTimer = 0.0f;
					UpdateText();
				}
			}
		}

		public void UpdateText()
		{
			if (this.textLabel != null)
			{
				this.textLabel.text = this.remainingClicks.ToString();
			}
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

			if (this.remainingClicks <= 0)
			{
				ExtremeSystemTypeManager.RpcUpdateSystem(
					Type,
					writer =>
					{
						writer.Write((byte)Ops.DestroySeal);
						writer.WritePacked(this.sealId);
					});
			}
		}
	}

	public sealed class SealCircle
	{
		public int Id { get; }
		public Vector2 Position { get; }
		public ExtremeConsole? Console { get; set; }
		public NullifierConsoleBehavior? Behavior { get; set; }

		public SealCircle(int id, Vector2 position)
		{
			this.Id = id;
			this.Position = position;
		}

		public void Destroy()
		{
			if (this.Console != null && this.Console.gameObject != null)
			{
				UnityEngine.Object.Destroy(this.Console.gameObject);
			}
		}
	}

	private readonly float range;
	private readonly int requiredClicks;
	private readonly float restoreTime;
	private readonly NullifierTargetTeam targetTeam;
	private readonly IUnityObjectFactory unityObjectFactory;
	private readonly ButtonLockSystem abilityLocker = ButtonLockSystem.CreateOrGetAbilityButtonLockSystem();
	private readonly ExtremeConsoleSystem consoleSystem = ExtremeConsoleSystem.Create();

	private readonly List<Vector2> scheduledReservations = [];
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

	public NullifierSystem(
		float range,
		int requiredClicks,
		float restoreTime,
		NullifierTargetTeam targetTeam,
		IUnityObjectFactory unityObjectFactory)
	{
		this.range = range;
		this.requiredClicks = requiredClicks;
		this.restoreTime = restoreTime;
		this.targetTeam = targetTeam;
		this.unityObjectFactory = unityObjectFactory;
	}

	public IReadOnlyDictionary<int, SealCircle> PlacedSeals => this.placedSeals;
	public IReadOnlyList<Vector2> ScheduledReservations => this.scheduledReservations;

	public void ReserveSeal(Vector2 position)
	{
		this.scheduledReservations.Add(position);
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
		if (player.IsInValid())
		{
			return false;
		}

		if (!ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) || role == null)
		{
			return false;
		}

		if (role.IsImpostor())
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
			if (seal.Behavior != null)
			{
				seal.Behavior.UpdateTimer(deltaTime, this.restoreTime, this.requiredClicks);
			}
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
			foreach (var pos in this.scheduledReservations)
			{
				createSeal(this.nextSealId++, pos);
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
				ReserveSeal(new Vector2(rx, ry));
				break;

			case Ops.DestroySeal:
				int destroyId = msgReader.ReadPackedInt32();
				destroySeal(destroyId);
				break;
		}
	}

	internal void AddSealForTest(int id, Vector2 pos)
	{
		this.placedSeals[id] = new SealCircle(id, pos);
	}

	private void createSeal(int id, Vector2 pos)
	{
		if (this.placedSeals.ContainsKey(id))
		{
			return;
		}

		var seal = new SealCircle(id, pos);

		if (HudManager.InstanceExists &&
			HudManager.Instance != null &&
			HudManager.Instance.KillButton != null &&
			HudManager.Instance.KillButton.cooldownTimerText != null)
		{
			var hud = HudManager.Instance;
			var console = this.consoleSystem.CreateConsoleObj(pos, $"NullifierSeal_{id}");
			if (console.Image != null)
			{
				console.Image.sprite = this.unityObjectFactory.LoadRoleSprite(ExtremeRoleId.Nullifier);
				var color = console.Image.color;
				color.a = 0.8f;
				console.Image.color = color;
			}
			console.transform.localScale = new Vector3(this.range * 2, this.range * 2, 1.0f);

			var textObj = UnityEngine.Object.Instantiate(
				hud.KillButton.cooldownTimerText,
				console.transform,
				false);
			textObj.gameObject.SetActive(true);
			textObj.transform.localPosition = new Vector3(0.0f, 0.0f, -1.0f);
			textObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			textObj.alignment = TextAlignmentOptions.Center;
			textObj.color = Color.white;

			var behavior = new NullifierConsoleBehavior(id, this.requiredClicks, textObj);
			console.Behavior = behavior;

			var collider = console.gameObject.AddComponent<CircleCollider2D>();
			collider.isTrigger = true;
			collider.radius = 0.5f;

			seal.Console = console;
			seal.Behavior = behavior;
			behavior.UpdateText();
		}

		this.placedSeals[id] = seal;
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
		if (!IsPlayerInSeal(localPlayer))
		{
			this.abilityLocker.UnLock((int)ButtonLockSystem.ConditionId.Nullifier);
		}
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
