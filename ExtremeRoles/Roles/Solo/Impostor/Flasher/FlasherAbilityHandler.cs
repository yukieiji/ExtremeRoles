using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Helper;



#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.Flasher;

/// <summary>
/// フラッシュの能力ロジックとRPCの送受信を担う。
/// </summary>
public sealed class FlasherAbilityHandler(
	FlasherStatusModel status,
	FlasherRole role,
	IFlashScreenEffect screenEffect,
	float activeTime,
	IUnityObjectFactory factory)
{
	private readonly FlasherStatusModel status = status;
	private readonly FlasherRole role = role;
	private readonly IFlashScreenEffect screenEffect = screenEffect;

	/// <summary>画面の塗りつぶしが継続する時間、RPCが届かない場合の上限計算に使う</summary>
	private readonly float activeTime = activeTime;

	private readonly IUnityObjectFactory factory = factory;

	/// <summary>
	/// ボタン能力「フラッシュ」の挙動を生成する。
	/// </summary>
	public ChargingAndActivatingCountBehaviour CreateBehavior()
	{
		var behavior = new ChargingAndActivatingCountBehaviour(
			text: Tr.GetString("FlasherFlash"),
			img: this.factory.LoadRoleSprite(ExtremeRoleId.Flasher),
			isUse: IsUse,
			ability: TryFlash,
			onCharge: OnCharge,
			reduceTiming: ChargingAndActivatingCountBehaviour.ReduceTiming.OnActive,
			isCharge: IsCharging,
			canActivating: CanActivating,
			abilityOff: ToAfterglow,
			forceAbilityOff: ForceEnd);

		behavior.ChargeTime = FlasherStatusModel.ChargeTime;

		return behavior;
	}

	/// <summary>
	/// ボタンが押せるかどうかを返す。対象の有無は発動時にだけ判定する。
	/// </summary>
	public bool IsUse(bool isCharging, float chargeGage)
		=> this.role.IsAbilityUse();

	/// <summary>
	/// フラッシュを発動し、範囲内のプレイヤーへ画面の塗りつぶしの開始を送る。
	/// </summary>
	public bool TryFlash(float chargeGage)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}

		List<PlayerControl> targets = getTargets(localPlayer);

		// 0でも打たせる、人がいるって分かってしまうので
		if (targets.Count == 0)
		{
			return true;
		}

		this.status.StartSequence(chargeGage);

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.FlasherOps))
		{
			caller.WriteByte((byte)FlasherRole.FlasherRpcOps.FlashStart);
			caller.WriteByte(localPlayer.PlayerId);
			caller.WriteByte((byte)this.status.FlashColor);
			caller.WriteByte((byte)targets.Count);

			foreach (PlayerControl target in targets)
			{
				caller.WriteByte(target.PlayerId);
			}
		}

		return true;
	}

	/// <summary>
	/// 画面の塗りつぶしの終了を、余韻への移行として送る。
	/// </summary>
	public void ToAfterglow()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null || !this.status.IsSequenceActive)
		{
			return;
		}

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.FlasherOps))
		{
			caller.WriteByte((byte)FlasherRole.FlasherRpcOps.FlashAfterglow);
			caller.WriteByte(localPlayer.PlayerId);
			caller.WriteFloat(this.status.CalculateAfterglowTime());
		}

		// 余韻への移行を送った時点で本人のシーケンスは終了。以降の会議で余計な終了送信はしない
		this.status.EndSequence();
	}

	/// <summary>
	/// 演出を完全に閉じる処理。強制終了と会議開始で呼ばれる。
	/// </summary>
	public void ForceEnd()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null || !this.status.IsSequenceActive)
		{
			return;
		}

		this.status.EndSequence();

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.FlasherOps))
		{
			caller.WriteByte((byte)FlasherRole.FlasherRpcOps.FlashEnd);
			caller.WriteByte(localPlayer.PlayerId);
		}
	}

	/// <summary>
	/// 受信したRPCを処理する。
	/// </summary>
	public void ApplyRpcOps(FlasherRole.FlasherRpcOps ops, in MessageReader reader)
	{
		switch (ops)
		{
			case FlasherRole.FlasherRpcOps.FlashStart:
				applyFlashStart(reader);
				break;
			case FlasherRole.FlasherRpcOps.FlashAfterglow:
				this.screenEffect.StartAfterglow(reader.ReadSingle());
				break;
			case FlasherRole.FlasherRpcOps.FlashEnd:
				this.screenEffect.Stop();
				break;
			default:
				break;
		}
	}

	private bool OnCharge() => true;

	private bool IsCharging() => this.role.IsAbilityUse();

	private bool CanActivating() => PlayerControl.LocalPlayer.IsAlive();

	/// <summary>
	/// 範囲内の対象を返す。壁の判定は行わず、死亡者・自身・ベント内・同陣営・無敵を除外する。
	/// </summary>
	private List<PlayerControl> getTargets(PlayerControl localPlayer)
	{
		var result = new List<PlayerControl>();

		if (GameData.Instance == null)
		{
			return result;
		}

		Vector2 localPosition = localPlayer.GetTruePosition();

		foreach (NetworkedPlayerInfo playerInfo in
			GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (!IsValidPlayer(localPlayer, playerInfo))
			{
				continue;
			}

			PlayerControl target = playerInfo.Object;

			if ((target.GetTruePosition() - localPosition).magnitude > this.status.Range)
			{
				continue;
			}

			result.Add(target);
		}

		return result;
	}

	private void applyFlashStart(in MessageReader reader)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null)
		{
			return;
		}

		var flashColor = (FlasherColor)reader.ReadByte();
		byte targetCount = reader.ReadByte();
		bool isTarget = false;

		for (int i = 0; i < targetCount; i++)
		{
			if (reader.ReadByte() == localPlayer.PlayerId)
			{
				isTarget = true;
			}
		}

		if (!isTarget || !localPlayer.IsAlive())
		{
			return;
		}

		this.screenEffect.StartHold(
			this.activeTime + FlasherStatusModel.AfterglowMaxTime,
			FlasherStatusModel.ToColor(flashColor));
		Sound.PlaySound(Sound.Type.FlasherFlash, 0.875f);
	}

	public bool IsValidPlayer(
		PlayerControl sourcePlayer,
		NetworkedPlayerInfo targetPlayer)
	{
		if (targetPlayer == null)
		{
			return false;
		}
		byte targetPlayerId = targetPlayer.PlayerId;
		byte sourcePlayerId = sourcePlayer.PlayerId;

		return (
			targetPlayerId != sourcePlayerId &&
			targetPlayer.IsAlive() &&
			!targetPlayer.Object.inVent &&
			ExtremeRoleManager.TryGetRole(targetPlayerId, out var targetPlayerRole) &&
			(
				!this.role.IsSameTeam(targetPlayerRole) ||
				(this.status.IsAffectImpostor && targetPlayerRole.IsImpostor())
			) &&
			(targetPlayerRole.AbilityClass is not IInvincible invincible || invincible.IsValidAbilitySource(sourcePlayerId))
		);
	}
}
