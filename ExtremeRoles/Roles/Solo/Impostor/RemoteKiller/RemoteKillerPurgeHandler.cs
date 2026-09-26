using System.Linq;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;

public sealed class RemoteKillerPurgeHandler(RemoteKillerStatusModel status, RemoteKillerRole role)
{
	private readonly RemoteKillerStatusModel status = status;
	private readonly RemoteKillerRole role = role;
	private ShapeShiftMinigameWrapper? minigame;
	private PlayerControl? selectedPurgeTarget;

	public ChargingAndActivatingCountBehaviour CreateBehavior(float coolTime, int purgeCount)
	{
		var behavior = new ChargingAndActivatingCountBehaviour(
			text: Tr.GetString("remoteKillerPurge"),
			img: UnityObjectLoader.LoadSpriteFromResources(ObjectPath.SucideSprite),
			isUse: IsUsePurgeCheck,
			ability: PurgeStartAbility,
			onCharge: PurgeOpenMenu,
			reduceTiming: ChargingAndActivatingCountBehaviour.ReduceTiming.OnActiveDone,
			isCharge: () => this.minigame != null && this.minigame.IsOpen,
			canActivating: IsPurgeCheck,
			abilityOff: PurgeCleanUp,
			forceAbilityOff: PurgeForceCleanUp);

		behavior.SetCoolTime(coolTime);
		behavior.ChargeTime = float.MaxValue;
		behavior.ActiveTime = this.status.PurgeTime;
		behavior.SetAbilityCount(purgeCount);

		return behavior;
	}

	public bool IsUsePurgeCheck(bool isCharging, float chargeGage)
	{
		var isSab = PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer);
		if (isSab)
		{
			if (isCharging && Minigame.Instance != null)
			{
				Minigame.Instance.ForceClose();
			}
			return false;
		}

		if (isCharging)
		{
			return this.role.IsAbilityUseWithMinigame();
		}

		if (!this.role.IsAbilityUse())
		{
			return false;
		}

		return this.status.ExecutionTargets.Any(id =>
		{
			var p = Player.GetPlayerControlById(id);
			return p != null && p.Data != null && !p.Data.IsDead && !p.Data.Disconnected;
		});
	}

	public bool PurgeOpenMenu()
	{
		this.selectedPurgeTarget = null;
		this.minigame ??= new ShapeShiftMinigameWrapper();
		return this.minigame.IsOpen || this.minigame.OpenUi(
			OnPurgeTargetSelected,
			p => p != null && p.Data != null && !p.Data.IsDead && !p.Data.Disconnected && this.status.HasExecutionTarget(p.PlayerId));
	}

	public void OnPurgeTargetSelected(PlayerControl target)
	{
		if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
		{
			return;
		}

		if (!this.status.HasExecutionTarget(target.PlayerId))
		{
			return;
		}

		this.selectedPurgeTarget = target;

		if (Minigame.Instance != null)
		{
			Minigame.Instance.Close();
		}
		this.minigame?.Reset();

		if (this.role.Button != null && this.role.Button.Transform.TryGetComponent<PassiveButton>(out var button))
		{
			button.OnClick.Invoke();
		}
	}

	public bool PurgeStartAbility(float chargeGage)
	{
		if (this.selectedPurgeTarget == null)
		{
			return false;
		}

		byte localId = PlayerControl.LocalPlayer.PlayerId;

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.RemoteKillerOps))
		{
			caller.WriteByte((byte)RemoteKillerRole.RemoteKillerRpc.PurgeStart);
			caller.WriteByte(localId);
		}

		return true;
	}

	public bool IsPurgeCheck()
	{
		if (this.selectedPurgeTarget == null ||
			this.selectedPurgeTarget.Data == null ||
			this.selectedPurgeTarget.Data.IsDead ||
			this.selectedPurgeTarget.Data.Disconnected ||
			PlayerControl.LocalPlayer.Data.IsDead ||
			MeetingHud.Instance != null)
		{
			return false;
		}

		return true;
	}

	public void PurgeCleanUp()
	{
		if (this.selectedPurgeTarget != null)
		{
			byte localId = PlayerControl.LocalPlayer.PlayerId;
			byte targetId = this.selectedPurgeTarget.PlayerId;

			// 粛清状態の解除RPCを送信
			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.RemoteKillerOps))
			{
				caller.WriteByte((byte)RemoteKillerRole.RemoteKillerRpc.PurgeCancel);
				caller.WriteByte(localId);
			}

			// 遠隔キル（自殺アニメーション）を実行
			Player.RpcUncheckMurderPlayer(targetId, targetId, 0);

			// 執行対象リストから除外
			this.status.RemoveExecutionTarget(targetId);
		}

		this.selectedPurgeTarget = null;
	}

	public void PurgeForceCleanUp()
	{
		if (this.status.IsPurging)
		{
			byte localId = PlayerControl.LocalPlayer.PlayerId;

			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.RemoteKillerOps))
			{
				caller.WriteByte((byte)RemoteKillerRole.RemoteKillerRpc.PurgeCancel);
				caller.WriteByte(localId);
			}
		}

		this.selectedPurgeTarget = null;
	}

	public void ResetMinigame()
	{
		this.minigame?.Reset();
	}
}
