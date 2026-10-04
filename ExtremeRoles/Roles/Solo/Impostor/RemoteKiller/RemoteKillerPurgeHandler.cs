using System.Linq;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Resources;
using ExtremeRoles.Extension.Player;

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
			img: HudManager.Instance.KillButton.graphic.sprite,
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
			return p.IsAlive();
		});
	}

	public bool PurgeOpenMenu()
	{
		this.selectedPurgeTarget = null;
		this.minigame ??= new ShapeShiftMinigameWrapper();
		return this.minigame.IsOpen || this.minigame.OpenUi(
			OnPurgeTargetSelected, this.status.ExecutionTargets);
	}

	public void OnPurgeTargetSelected(PlayerControl target)
	{
		if (target.IsAlive() &&
			this.status.HasExecutionTarget(target.PlayerId) &&
			this.role.Button != null &&
			this.role.Button.Transform.TryGetComponent<PassiveButton>(out var button))
		{
			this.selectedPurgeTarget = target;
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
		=> this.status.IsPurging &&
			this.selectedPurgeTarget.IsAlive() &&
			PlayerControl.LocalPlayer.IsAlive() &&
			MeetingHud.Instance != null;

	public void PurgeCleanUp()
	{
		if (this.selectedPurgeTarget.IsAlive())
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
